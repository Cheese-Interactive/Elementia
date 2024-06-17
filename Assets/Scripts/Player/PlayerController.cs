using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Mechanics")]
    private Dictionary<MechanicType, bool> mechanicStatuses;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    private float horizontalInput;
    private bool isFacingRight;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;

    [Header("Health")]
    [SerializeField] private float maxHealth;
    private float currHealth;

    [Header("Spells")]
    [SerializeField] private Transform castPoint;

    [Header("Barrier")]
    [SerializeField] private SpriteRenderer barrier;
    private float barrierAlpha;
    private bool isBarrierDeployed;
    private Coroutine barrierCoroutine;
    private Tweener barrierTweener;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask environmentMask;
    private bool isGrounded;

    private void Awake() {

        // set up mechanic statuses early so scripts can change them earlier too
        mechanicStatuses = new Dictionary<MechanicType, bool>();
        Array mechanics = Enum.GetValues(typeof(MechanicType)); // get all mechanic type values

        // add all mechanic types to dictionary
        foreach (MechanicType mechanicType in mechanics)
            mechanicStatuses.Add(mechanicType, true); // set all mechanics to true by default

    }

    private void Start() {

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currHealth = maxHealth; // set current health to max health

        isFacingRight = true; // player faces right by default

        barrierAlpha = barrier.color.a;
        barrier.gameObject.SetActive(false); // barrier is not deployed by default

    }

    private void Update() {

        /* GROUND CHECK */
        isGrounded = Physics2D.OverlapCircle(leftFoot.position, groundCheckRadius, environmentMask) || Physics2D.OverlapCircle(rightFoot.position, groundCheckRadius, environmentMask); // check both feet for ground check

        /* MOVEMENT */
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.Space) && isGrounded)
            Jump();

        /* FLIPPING */
        CheckFlip();

        /* SPELLS */
        if (Input.GetMouseButtonDown(0) && IsMechanicEnabled(MechanicType.PrimaryAction)) {

            // primary action
            GetComponent<Element>().PrimaryAction();

        } else if ((Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1)) && IsMechanicEnabled(MechanicType.SecondaryAction)) {

            // secondary action
            GetComponent<Element>().SecondaryAction();

        }
    }

    private void FixedUpdate() {

        if (IsMechanicEnabled(MechanicType.Movement)) {

            rb.velocity = new Vector2(horizontalInput * walkSpeed, rb.velocity.y); // adjust input based on rotation (if wand is out, player walks, else sprint)
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

    #region Barrier

    public void DeployBarrier() {

        if (barrierCoroutine != null) {

            StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running
            isBarrierDeployed = false; // set barrier to not deployed

        }

        if (barrierTweener != null && barrierTweener.IsActive()) {

            barrierTweener.Kill(); // kill barrier tweener if it's active
            isBarrierDeployed = false; // set barrier to not deployed

        }

        barrierCoroutine = StartCoroutine(HandleDeployBarrier());

    }

    public void RetractBarrier() {

        if (barrierCoroutine != null) {

            StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running
            isBarrierDeployed = true; // set barrier to deployed

        }

        if (barrierTweener != null && barrierTweener.IsActive()) {

            barrierTweener.Kill(); // kill barrier tweener if it's active
            isBarrierDeployed = true; // set barrier to deployed

        }

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

        barrierTweener = barrier.DOFade(barrierAlpha, anim.GetCurrentAnimatorStateInfo(0).length).SetEase(Ease.InBounce).OnComplete(() => {

            isBarrierDeployed = true; // flip bool after animation is done
            barrierCoroutine = null;

        }); // fade barrier in based on animation length
    }

    private IEnumerator HandleRetractBarrier() {

        barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, barrierAlpha); // set barrier alpha to full
        anim.SetBool("isBarrierDeployed", false);
        yield return null; // wait for animation to start

        barrierTweener = barrier.DOFade(0f, anim.GetCurrentAnimatorStateInfo(0).length).SetEase(Ease.OutBounce).OnComplete(() => {

            barrier.gameObject.SetActive(false); // hide barrier
            isBarrierDeployed = false;
            EnableAllMechanics(); // enable all mechanics after barrier is retracted
            barrierCoroutine = null;

        }); // fade barrier in based on animation length
    }

    #endregion

    #region HEALTH

    public void TakeDamage(float damage) {

        currHealth -= damage;

        if (currHealth <= 0f)
            Die();

    }

    public void Die() {

        // TODO: death stuff

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
