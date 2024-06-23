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
    private float barrierAlpha;
    private Coroutine barrierCoroutine;
    private Tweener barrierTweener;
    private bool barrierDeployed;
    private bool retracted; // for barrier max duration

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

        hotbar = FindObjectOfType<Hotbar>();

        Weapon weapon = weaponActionPairs[0].GetWeapon(); // get first weapon
        charWeapon.ChangeWeapon(weapon, weapon.WeaponID); // change weapon to first weapon by default

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

    }

    private void Update() {

        if (isDead)
            return; // player is dead, no need to update

        currWeapon = null;
        SecondaryAction currSecondaryAction = null;

        if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

            currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
            currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction();

        }

        /* SCROLL WHEEL WEAPON SWITCHING */
        if (Input.mouseScrollDelta.y > 0f && !barrierDeployed) { // make sure barrier is not deployed before switching

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.CycleSlot(-1); // cycle hotbar slot backwards

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.mouseScrollDelta.y < 0f && !barrierDeployed) { // make sure barrier is not deployed before switching

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.CycleSlot(1); // cycle hotbar slot forwards

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        }

        /* KEY WEAPON SWITCHING */
        if (Input.GetKeyDown(KeyCode.Alpha1) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(0);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha2) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(1);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha3) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(2);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha4) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(3);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha5) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(4);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha6) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(5);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha7) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(6);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha8) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(7);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha9) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(8);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        } else if (Input.GetKeyDown(KeyCode.Alpha0) && !barrierDeployed) {

            if (currWeapon)
                currSecondaryAction.enabled = false; // disable current secondary action

            hotbar.SelectSlot(9);

            if (hotbar.GetCurrWeapon() < weaponActionPairs.Length) { // make sure slot has a weapon in it

                currWeapon = weaponActionPairs[hotbar.GetCurrWeapon()].GetWeapon();
                charWeapon.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

                if (currWeapon) {

                    currSecondaryAction = weaponActionPairs[hotbar.GetCurrWeapon()].GetSecondaryAction(); // update secondary action
                    currSecondaryAction.enabled = true; // enable new action

                }
            } else {

                charWeapon.ChangeWeapon(null, null); // remove weapon

            }
        }

        /* SECONDARY ACTIONS */
        // IMPORTANT: do this after weapon switching to make sure the right secondary action is used
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
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Water") && !barrierDeployed) // barrier can save player from water
            health.Kill();

    }

    private void OnDisable() {

        // unsubscribe from all events
        foreach (SecondaryAction action in deathSubscriptions)
            health.OnDeath -= action.OnDeath;

        health.OnDeath -= slowEffect.RemoveEffect; // remove slow effect on death

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

        currWeapon.gameObject.SetActive(true);
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

        currWeapon.gameObject.SetActive(false);

    }

    private void OnRespawn() => isDead = false;

    private void OnDeath() => isDead = true;

    #endregion
}
