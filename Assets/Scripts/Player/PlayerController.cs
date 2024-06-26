using DG.Tweening;
using MoreMountains.CorgiEngine;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour {

    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private SlowEffect slowEffect;
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
    private CharacterLookUp charLookUp;
    private CharacterGrip charGrip;
    private CharacterWallClinging charWallCling;
    private CharacterWalljump charWallJump;
    private CharacterLadder charLadder;
    private CharacterButtonActivation charButton;
    private CharacterHandleWeapon charWeapon;
    private Weapon currWeapon;

    [Header("Hotbar")]
    private Hotbar hotbar;

    [Header("Weapons/Secondary Actions")]
    [SerializeField] private WeaponActionPair[] weaponActionPairs;

    [Header("Barrier")]
    [SerializeField] private SpriteRenderer barrier;
    private BarrierAction barrierAction;
    private float barrierAlpha;
    private Coroutine barrierCoroutine;
    private Tweener barrierTweener;
    private bool isBarrierRetractedPreMax; // for barrier max duration

    [Header("Flamethrower")]
    [SerializeField] private Transform flamethrower;
    private FlamethrowerAction flamethrowerAction;
    private Quaternion initialRot;
    private bool isFlamethrowerFlipped;
    private bool isFlamethrowerRetractedPreMax; // for flamethrower max duration

    [Header("Death")]
    private bool isDead; // to deal with death delay

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

        spriteRenderer = GetComponent<SpriteRenderer>();
        slowEffect = GetComponent<SlowEffect>();
        anim = GetComponent<Animator>();

        corgiController = GetComponent<CorgiController>();
        health = GetComponent<Health>();
        charMovement = GetComponent<CharacterHorizontalMovement>();
        charCrouch = GetComponent<CharacterCrouch>();
        charDash = GetComponent<CharacterDash>();
        charDive = GetComponent<CharacterDive>();
        charDangling = GetComponent<CharacterDangling>();
        charJump = GetComponent<CharacterJump>();
        charLookUp = GetComponent<CharacterLookUp>();
        charGrip = GetComponent<CharacterGrip>();
        charWallCling = GetComponent<CharacterWallClinging>();
        charWallJump = GetComponent<CharacterWalljump>();
        charLadder = GetComponent<CharacterLadder>();
        charButton = GetComponent<CharacterButtonActivation>();
        charWeapon = GetComponent<CharacterHandleWeapon>();

        barrierAction = GetComponent<BarrierAction>();
        flamethrowerAction = GetComponent<FlamethrowerAction>();

        hotbar = FindObjectOfType<Hotbar>();

        currWeapon = weaponActionPairs[0].GetWeapon(); // get first weapon
        charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon to first weapon by default

        deathSubscriptions = new List<SecondaryAction>();

        // pick the first secondary action as the default, subscribe to death event, initialize hotbar
        for (int i = 0; i < weaponActionPairs.Length; i++) {

            WeaponActionPair action = weaponActionPairs[i];
            SecondaryAction secondaryAction = action.GetSecondaryAction();

            // enable current secondary action, disable the rest
            if (action == weaponActionPairs[0])
                secondaryAction.enabled = true;
            else
                secondaryAction.enabled = false;

            hotbar.SetWeapon(action.GetWeaponData(), i); // add weapon item to hotbar

            health.OnDeath += secondaryAction.OnDeath; // subscribe to death event
            deathSubscriptions.Add(secondaryAction); // add to list for unsubscribing later

        }

        health.OnDeath += slowEffect.RemoveEffect; // remove slow effect on death
        health.OnDeath += OnDeath; // to flag death bool to deal with death delay
        health.OnRevive += OnRespawn; // to flag death bool to deal with death delay

        barrierAlpha = barrier.color.a;
        barrier.gameObject.SetActive(false); // barrier is not deployed by default

        flamethrower.gameObject.SetActive(false); // hide flamethrower particles
        initialRot = flamethrower.transform.rotation;

    }

    private void Update() {

        /* SECONDARY ACTIONS */
        // IMPORTANT: do this before isDead check to prevent toggle issues on death
        currWeapon = null;
        SecondaryAction currSecondaryAction = null;

        if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

            currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
            currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction();

        }

        if (currWeapon) { // make sure slot has a weapon/secondary action in it

            if ((((currSecondaryAction.IsAuto() && Input.GetMouseButton(1)) // secondary action is auto
                    || (!currSecondaryAction.IsAuto() && Input.GetMouseButtonDown(1))) // secondary action is not auto
                    || (currSecondaryAction.IsToggle() && (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1)))) // secondary action is toggle
                    && IsMechanicEnabled(MechanicType.SecondaryAction)) { // checks if mechanic is enabled

                // secondary action
                // GetComponent<Element>().SecondaryAction(); <- use if updating element variable is inconvenient
                currSecondaryAction.OnTrigger();

            }
        }

        // handle flipping with flamethrower (gets flipped on sprite renderer)
        if (flamethrowerAction.IsFlamethrowerEquipped()) {

            if (spriteRenderer.flipX && !isFlamethrowerFlipped) { // then flip flamethrower

                flamethrower.transform.localPosition = new Vector3(-flamethrower.transform.localPosition.x, flamethrower.transform.localPosition.y, flamethrower.transform.localPosition.z); // flip x axis local position
                flamethrower.transform.rotation *= Quaternion.Euler(0f, 180f, 0f); // flip overlay by adding 180f on the Y axis
                isFlamethrowerFlipped = true;

            } else if (!spriteRenderer.flipX && isFlamethrowerFlipped) { // then unflip flamethrower

                flamethrower.transform.localPosition = new Vector3(-flamethrower.transform.localPosition.x, flamethrower.transform.localPosition.y, flamethrower.transform.localPosition.z); // flip x axis position
                flamethrower.transform.rotation = initialRot; // reset overlay rotation to initial rotation
                isFlamethrowerFlipped = false;

            }
        }

        if (isDead)
            return; // player is dead, no need to update

        /* SCROLL WHEEL WEAPON SWITCHING */
        if (Input.mouseScrollDelta.y > 0f && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) { // make sure barrier is not deployed & flamethrower isn't equipped before switching

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.CycleSlot(-1); // cycle hotbar slot backwards

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.mouseScrollDelta.y < 0f && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) { // make sure barrier is not deployed before switching

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.CycleSlot(1); // cycle hotbar slot forwards

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        }

        /* KEY WEAPON SWITCHING */
        if (Input.GetKeyDown(KeyCode.Alpha1) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(0);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha2) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(1);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha3) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(2);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha4) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(3);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha5) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(4);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha6) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(5);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha7) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(6);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha8) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(7);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha9) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(8);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha0) && !barrierAction.IsBarrierDeployed() && !flamethrowerAction.IsFlamethrowerEquipped()) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(9);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.SetInitialToggled(Input.GetMouseButton(1)); // set initial toggled status to if mouse is already down
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Water") && !barrierAction.IsBarrierDeployed()) // barrier can save player from water
            health.Kill();

    }

    private void OnDisable() {

        // unsubscribe from all events
        foreach (SecondaryAction action in deathSubscriptions)
            health.OnDeath -= action.OnDeath;

        health.OnDeath -= slowEffect.RemoveEffect; // remove slow effect on death

    }

    #region BARRIER

    public void DeployBarrier(float maxDuration) {

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running

        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        barrierCoroutine = StartCoroutine(HandleDeployBarrier());

        DisableAllScripts(); // disable all scripts while barrier is deployed
        charWeapon.CurrentWeapon.gameObject.SetActive(false); // hide weapon (use charWeapon.CurrentWeapon instead of currWeapon because it has the actual instance of the weapon object)

        isBarrierRetractedPreMax = false; // barrier is not retracted yet (for max duration)
        StartCoroutine(HandleBarrierDuration(maxDuration)); // handle barrier max duration

    }

    private IEnumerator HandleDeployBarrier() {

        DisableAllMechanics(); // disable all mechanics while barrier is being deployed (except secondary action)
        EnableMechanic(MechanicType.SecondaryAction); // enable only secondary action while barrier is deployed

        barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, 0f); // set barrier alpha to none
        barrier.gameObject.SetActive(true); // show barrier
        anim.SetBool("isBarrierDeployed", true); // play barrier deploy animation
        yield return null; // wait for animation to start

        barrierTweener = barrier.DOFade(barrierAlpha, anim.GetCurrentAnimatorStateInfo(0).length).SetEase(Ease.InBounce).OnComplete(() => barrierCoroutine = null); // fade barrier in based on animation length

    }

    public void RetractBarrier() {

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running

        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        charWeapon.CurrentWeapon.gameObject.SetActive(true); // show weapon (use charWeapon.CurrentWeapon instead of currWeapon because it has the actual instance of the weapon object)

        isBarrierRetractedPreMax = true; // barrier is retracted (for max duration)
        barrierCoroutine = StartCoroutine(HandleRetractBarrier());

        /* the following is done without a fade animation */
        //barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, barrierAlpha); // set barrier alpha to full
        //anim.SetBool("isBarrierDeployed", false);
        //barrier.gameObject.SetActive(false); // hide barrier
        //isBarrierDeployed = false;
        //EnableAllMechanics(); // enable all mechanics after barrier is retracted

    }

    private IEnumerator HandleRetractBarrier() {

        barrier.color = new Color(barrier.color.r, barrier.color.g, barrier.color.b, barrierAlpha); // set barrier alpha to full
        anim.SetBool("isBarrierDeployed", false);
        yield return null; // wait for animation to start

        barrierTweener = barrier.DOFade(0f, anim.GetCurrentAnimatorStateInfo(0).length).SetEase(Ease.OutBounce).OnComplete(() => {

            barrier.gameObject.SetActive(false); // hide barrier
            EnableAllScripts(); // enable all scripts after barrier is retracted
            EnableAllMechanics(); // enable all mechanics after barrier is retracted
            barrierCoroutine = null;

        }); // fade barrier in based on animation length
    }

    private IEnumerator HandleBarrierDuration(float maxDuration) {

        float timer = 0f;

        while (timer < maxDuration) {

            if (isBarrierRetractedPreMax) { // barrier is retracted before max duration

                isBarrierRetractedPreMax = false; // reset retracted status
                yield break;

            }

            timer += Time.deltaTime;
            yield return null;

        }

        RetractBarrier();

    }

    #endregion

    #region FLAMETHROWER

    public void EquipFlamethrower(float maxDuration) {

        charWeapon.CurrentWeapon.gameObject.SetActive(false); // hide weapon (use charWeapon.CurrentWeapon instead of currWeapon because it has the actual instance of the weapon object)
        isFlamethrowerRetractedPreMax = false; // flamethrower is not unequipped yet (for max duration)

        DisableAllMechanics(); // disable all mechanics while flamethrower is being equipped (except secondary action)
        EnableMechanic(MechanicType.SecondaryAction); // enable only secondary action while flamethrower is equipped

        flamethrower.gameObject.SetActive(true); // show flamethrower particles
        StartCoroutine(HandleFlamethrowerDuration(maxDuration)); // handle flamethrower max duration

    }

    public void UnequipFlamethrower() {

        charWeapon.CurrentWeapon.gameObject.SetActive(true); // show weapon (use charWeapon.CurrentWeapon instead of currWeapon because it has the actual instance of the weapon object)
        currWeapon.gameObject.SetActive(true); // show weapon

        isFlamethrowerRetractedPreMax = true; // flamethrower is unequipped (for max duration)

        EnableAllMechanics(); // enable all mechanics after flamethrower is unequipped

        flamethrower.gameObject.SetActive(false); // hide flamethrower particles

    }

    private IEnumerator HandleFlamethrowerDuration(float maxDuration) {

        float timer = 0f;

        while (timer < maxDuration) {

            if (isFlamethrowerRetractedPreMax) { // flamethrower is retracted before max duration

                isFlamethrowerRetractedPreMax = false; // reset retracted status
                yield break;

            }

            timer += Time.deltaTime;
            yield return null;

        }

        UnequipFlamethrower();

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
        charLookUp.AbilityPermitted = true;
        charGrip.AbilityPermitted = true;
        charWallCling.AbilityPermitted = true;
        charWallJump.AbilityPermitted = true;
        charLadder.AbilityPermitted = true;
        charButton.AbilityPermitted = true;
        charWeapon.AbilityPermitted = true;

    }

    private void DisableAllScripts() {

        corgiController.enabled = false;
        charMovement.AbilityPermitted = false;
        charCrouch.AbilityPermitted = false;
        charDash.AbilityPermitted = false;
        charDive.AbilityPermitted = false;
        charDangling.AbilityPermitted = false;
        charJump.AbilityPermitted = false;
        charLookUp.AbilityPermitted = false;
        charGrip.AbilityPermitted = false;
        charWallCling.AbilityPermitted = false;
        charWallJump.AbilityPermitted = false;
        charLadder.AbilityPermitted = false;
        charButton.AbilityPermitted = false;
        charWeapon.AbilityPermitted = false;

    }

    private void OnRespawn() => isDead = false;

    private void OnDeath() {

        isDead = true;

        if (barrierCoroutine != null) StopCoroutine(barrierCoroutine); // stop barrier coroutine if it's running
        if (barrierTweener != null && barrierTweener.IsActive()) barrierTweener.Kill(); // kill barrier tweener if it's active

        flamethrower.gameObject.SetActive(false); // hide flamethrower particles

    }

    #endregion
}
