using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : EntityController {

    [Header("Mechanics")]
    private Dictionary<MechanicType, bool> mechanicStatuses;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private float horizontalInput;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;

    [Header("Elements")]
    private Element element; // make sure to update this variable when the element changes

    [Header("Spells")]
    [SerializeField] private Transform castPoint;

    [Header("Barrier")]
    [SerializeField] private SpriteRenderer barrier;
    private float barrierAlpha;
    private Coroutine barrierCoroutine;
    private Tweener barrierTweener;
    private bool retracted; // for barrier max duration

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask environmentMask;
    private bool isGrounded;

    private new void Awake() {

        base.Awake(); // call base awake method

        // set up mechanic statuses early so scripts can change them earlier too
        mechanicStatuses = new Dictionary<MechanicType, bool>();
        Array mechanics = Enum.GetValues(typeof(MechanicType)); // get all mechanic type values

        // add all mechanic types to dictionary
        foreach (MechanicType mechanicType in mechanics)
            mechanicStatuses.Add(mechanicType, true); // set all mechanics to true by default

    }

    private void Start() {

        element = GetComponent<Element>();

        barrierAlpha = barrier.color.a;
        barrier.gameObject.SetActive(false); // barrier is not deployed by default

    }

    private void Update() {

        /* GROUND CHECK */
        isGrounded = Physics2D.Raycast(leftFoot.position, Vector2.down, groundCheckDistance, environmentMask) | Physics2D.Raycast(rightFoot.position, Vector2.down, groundCheckDistance, environmentMask); // check both feet for ground check

        /* MOVEMENT */
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.Space) && isGrounded)
            Jump();

        /* FLIPPING */
        CheckFlip();

        /* SPELLS */
        if ((((element.IsPrimaryAuto() && Input.GetMouseButton(0)) // primary action is auto
            || (!element.IsPrimaryAuto() && Input.GetMouseButtonDown(0))) // primary action is not auto
            || (element.IsPrimaryToggle() && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))) // primary action is toggle
            && IsMechanicEnabled(MechanicType.PrimaryAction)) { // checks if mechanic is enabled

            // primary action
            // GetComponent<Element>().PrimaryAction(); <- use if updating element variable is inconvenient
            element.PrimaryAction();

        } else if ((((element.IsSecondaryAuto() && Input.GetMouseButton(1)) // secondary action is auto
            || (!element.IsSecondaryAuto() && Input.GetMouseButtonDown(1))) // secondary action is not auto
            || (element.IsSecondaryToggle() && (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1)))) // secondary action is toggle
            && IsMechanicEnabled(MechanicType.SecondaryAction)) { // checks if mechanic is enabled

            // secondary action
            // GetComponent<Element>().SecondaryAction(); <- use if updating element variable is inconvenient
            element.SecondaryAction();

        }
    }

    private void FixedUpdate() {

        if (IsMechanicEnabled(MechanicType.Movement)) {

            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y); // adjust input based on rotation (if wand is out, player walks, else sprint)
            anim.SetBool("isMoving", horizontalInput != 0f && isGrounded); // player is moving on ground

        } else {

            rb.velocity = new Vector2(0f, rb.velocity.y); // stop player horizontal movement
            anim.SetBool("isMoving", false); // player is not moving

        }
    }

    private void CheckFlip() {

        if (!IsMechanicEnabled(MechanicType.Movement)) return; // movement mechanic is disabled

        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f) {

            transform.Rotate(0f, 180f, 0f);
            isFacingRight = !isFacingRight;

        }
    }

    private void Jump() {

        if (!IsMechanicEnabled(MechanicType.Jump)) return;

        rb.velocity = transform.up * new Vector2(rb.velocity.x, jumpForce);

    }

    #region BARRIER

    public void DeployBarrier(float duration) {

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running

        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        barrierCoroutine = StartCoroutine(HandleDeployBarrier());

        retracted = false; // barrier is not retracted yet (for max duration)
        StartCoroutine(HandleBarrierDuration(duration));

    }

    public void RetractBarrier() {

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running

        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        retracted = true; // barrier is retracted (for max duration)
        barrierCoroutine = StartCoroutine(HandleRetractBarrier());

        /* the following is done without a fade animation */
        //barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, barrierAlpha); // set barrier alpha to full
        //anim.SetBool("isBarrierDeployed", false);
        //barrier.gameObject.SetActive(false); // hide barrier
        //isBarrierDeployed = false;
        //EnableAllMechanics(); // enable all mechanics after barrier is retracted

    }

    private IEnumerator HandleDeployBarrier() {

        DisableAllMechanics(); // disable all mechanics while barrier is being deployed
        EnableMechanic(MechanicType.SecondaryAction); // enable only secondary action while barrier is deployed

        barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, 0f); // set barrier alpha to none
        barrier.gameObject.SetActive(true); // show barrier
        anim.SetBool("isBarrierDeployed", true); // play barrier deploy animation
        yield return null; // wait for animation to start

        barrierTweener = barrier.DOFade(barrierAlpha, anim.GetCurrentAnimatorStateInfo(0).length).SetEase(Ease.InBounce).OnComplete(() => barrierCoroutine = null); // fade barrier in based on animation length

    }

    private IEnumerator HandleRetractBarrier() {

        barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, barrierAlpha); // set barrier alpha to full
        anim.SetBool("isBarrierDeployed", false);
        yield return null; // wait for animation to start

        barrierTweener = barrier.DOFade(0f, anim.GetCurrentAnimatorStateInfo(0).length).SetEase(Ease.OutBounce).OnComplete(() => {

            barrier.gameObject.SetActive(false); // hide barrier
            EnableAllMechanics(); // enable all mechanics after barrier is retracted
            barrierCoroutine = null;

        }); // fade barrier in based on animation length
    }

    private IEnumerator HandleBarrierDuration(float duration) {

        float timer = 0f;

        while (timer < duration) {

            if (retracted) { // barrier is retracted before max duration

                retracted = false; // reset retracted status
                yield break;

            }

            timer += Time.deltaTime;
            yield return null;

        }

        RetractBarrier();

    }

    #endregion

    #region MECHANICS

    public void EnableAllMechanics() {

        // enable all mechanics
        foreach (MechanicType mechanicType in mechanicStatuses.Keys.ToList())
            mechanicStatuses[mechanicType] = true;

    }

    public void EnableMechanic(MechanicType mechanicType) {

        mechanicStatuses[mechanicType] = true;

    }

    public void DisableAllMechanics() {

        // disable all mechanics
        foreach (MechanicType mechanicType in mechanicStatuses.Keys.ToList())
            mechanicStatuses[mechanicType] = false;

        // send to idle animation
        anim.SetBool("isMoving", false); // stop moving animation

    }

    public void DisableMechanic(MechanicType mechanicType) {

        mechanicStatuses[mechanicType] = false;

    }

    public bool IsMechanicEnabled(MechanicType mechanicType) => mechanicStatuses[mechanicType];

    #endregion

    #region UTILITIES

    public bool IsGrounded() => isGrounded;

    public bool IsFacingRight() => isFacingRight;

    public Transform GetCastPoint() => castPoint;

    #endregion
}
