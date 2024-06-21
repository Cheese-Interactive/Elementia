using DG.Tweening;
using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("References")]
    private Animator anim;

    [Header("Mechanics")]
    private Dictionary<MechanicType, bool> mechanicStatuses;
    private CorgiController corgiController;
    private Health health;
    private CharacterHorizontalMovement charMovement;
    private CharacterCrouch charCrouch;
    private CharacterDash charDash;
    private CharacterDive charDive;
    private CharacterDangling charDangling;
    private CharacterJump charJump;
    private CharacterJetpack charJetpack;
    private CharacterLookUp charLookUp;
    private CharacterGrip charGrip;
    private CharacterWallClinging charWallCling;
    private CharacterWalljump charWallJump;
    private CharacterLadder charLadder;
    private CharacterButtonActivation charButton;
    private CharacterHandleWeapon charWeapon;
    private WeaponAim weaponAim;
    private Weapon weapon;

    [Header("Elements")]
    private SecondaryAction secondaryAction; // make sure to update this variable when the element changes

    [Header("Barrier")]
    [SerializeField] private SpriteRenderer barrier;
    private float barrierAlpha;
    private Coroutine barrierCoroutine;
    private Tweener barrierTweener;
    private bool barrierDeployed;
    private bool retracted; // for barrier max duration

    [Header("Health")]
    private List<SecondaryAction> deathSubscriptions; // for unsubscribing later

    private void Awake() {

        // set up mechanic statuses early so scripts can change them earlier too
        mechanicStatuses = new Dictionary<MechanicType, bool>();
        Array mechanics = Enum.GetValues(typeof(MechanicType)); // get all mechanic type values

        // add all mechanic types to dictionary
        foreach (MechanicType mechanicType in mechanics)
            mechanicStatuses.Add(mechanicType, true); // set all mechanics to true by default

    }

    private void Start() {

        anim = GetComponent<Animator>();

        corgiController = GetComponent<CorgiController>();
        health = GetComponent<Health>();
        charMovement = GetComponent<CharacterHorizontalMovement>();
        charCrouch = GetComponent<CharacterCrouch>();
        charDash = GetComponent<CharacterDash>();
        charDive = GetComponent<CharacterDive>();
        charDangling = GetComponent<CharacterDangling>();
        charJump = GetComponent<CharacterJump>();
        charJetpack = GetComponent<CharacterJetpack>();
        charLookUp = GetComponent<CharacterLookUp>();
        charGrip = GetComponent<CharacterGrip>();
        charWallCling = GetComponent<CharacterWallClinging>();
        charWallJump = GetComponent<CharacterWalljump>();
        charLadder = GetComponent<CharacterLadder>();
        charButton = GetComponent<CharacterButtonActivation>();
        charWeapon = GetComponent<CharacterHandleWeapon>();

        deathSubscriptions = new List<SecondaryAction>();

        // set element to active element
        foreach (SecondaryAction action in GetComponents<SecondaryAction>()) {

            if (action.enabled) this.secondaryAction = action;
            health.OnDeath += action.OnDeath;
            deathSubscriptions.Add(action);

        }

        barrierAlpha = barrier.color.a;
        barrier.gameObject.SetActive(false); // barrier is not deployed by default

    }

    private void OnDisable() {

        foreach (SecondaryAction action in deathSubscriptions)
            health.OnDeath -= secondaryAction.OnDeath;

    }

    private void Update() {

        /* SECONDARY ACTIONS */
        if ((((secondaryAction.IsAuto() && Input.GetMouseButton(1)) // secondary action is auto
                || (!secondaryAction.IsAuto() && Input.GetMouseButtonDown(1))) // secondary action is not auto
                || (secondaryAction.IsToggle() && (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1)))) // secondary action is toggle
                && IsMechanicEnabled(MechanicType.SecondaryAction)) { // checks if mechanic is enabled

            // secondary action
            // GetComponent<Element>().SecondaryAction(); <- use if updating element variable is inconvenient
            secondaryAction.OnTrigger();

        }
    }

    #region BARRIER

    public void DeployBarrier(float duration) {

        if (barrierDeployed) return; // barrier is already deployed

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running

        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        barrierCoroutine = StartCoroutine(HandleDeployBarrier());

        DisableAllScripts(); // disable all scripts while barrier is deployed
        retracted = false; // barrier is not retracted yet (for max duration)
        StartCoroutine(HandleBarrierDuration(duration));
        barrierDeployed = true;

    }

    public void RetractBarrier() {

        if (!barrierDeployed) return; // barrier is not deployed (no need to retract)

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running

        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        weapon.gameObject.SetActive(true);
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
            EnableAllScripts(); // enable all scripts after barrier is retracted
            EnableAllMechanics(); // enable all mechanics after barrier is retracted
            barrierDeployed = false;
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

    public bool IsGrounded() => corgiController.State.IsGrounded;

    private void EnableAllScripts() {

        corgiController.enabled = true;
        charMovement.AbilityPermitted = true;
        charCrouch.AbilityPermitted = true;
        charDash.AbilityPermitted = true;
        charDive.AbilityPermitted = true;
        charDangling.AbilityPermitted = true;
        charJump.AbilityPermitted = true;
        charJetpack.AbilityPermitted = true;
        charLookUp.AbilityPermitted = true;
        charGrip.AbilityPermitted = true;
        charWallCling.AbilityPermitted = true;
        charWallJump.AbilityPermitted = true;
        charLadder.AbilityPermitted = true;
        charButton.AbilityPermitted = true;
        charWeapon.AbilityPermitted = true;

        //weapon.enabled = true;
        //weaponAim.enabled = true;
        //weaponRest.enabled = true;

    }

    private void DisableAllScripts() {

        corgiController.enabled = false;
        charMovement.AbilityPermitted = false;
        charCrouch.AbilityPermitted = false;
        charDash.AbilityPermitted = false;
        charDive.AbilityPermitted = false;
        charDangling.AbilityPermitted = false;
        charJump.AbilityPermitted = false;
        charJetpack.AbilityPermitted = false;
        charLookUp.AbilityPermitted = false;
        charGrip.AbilityPermitted = false;
        charWallCling.AbilityPermitted = false;
        charWallJump.AbilityPermitted = false;
        charLadder.AbilityPermitted = false;
        charButton.AbilityPermitted = false;
        charWeapon.AbilityPermitted = false;

        weapon = GetComponentInChildren<Weapon>();
        //weaponAim = weapon.GetComponent<WeaponAim>();
        //weaponRest = weaponAim.GetComponent<WeaponRest>();

        //weapon.enabled = false;
        //weaponAim.enabled = false;
        //weaponRest.enabled = false;

        //weaponRest.transform.localPosition = weaponRest.GetRestLocalPos();
        //weaponRest.transform.rotation = Quaternion.Euler(weaponRest.GetRestRotation());

        weapon.gameObject.SetActive(false);

    }

    #endregion
}
