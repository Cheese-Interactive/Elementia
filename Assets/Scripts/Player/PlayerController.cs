using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController {

    [Header("Item Selector")]
    private ItemSelector itemSelector;

    [Header("Weapons/Primary/Secondary Actions")]
    [SerializeField] private WeaponPair[] defaultWeaponPairs; // default weapon action pairs (from inspector) that player begins with
    private List<WeaponPair> weaponPairs; // stores current weapons that player has
    private Coroutine switchCoroutine;

    [Header("Fire")]
    private FireSecondaryAction fireSecondaryAction;

    [Header("Earth")]
    private EarthPrimaryAction earthPrimaryAction;
    private Rock currRock;
    private Coroutine rockSummonCoroutine;

    [Header("Time")]
    private TimeSecondaryAction timeSecondaryAction;

    [Header("Death")]
    private bool isDead; // to deal with death delay

    [Header("Health")]
    private List<WeaponPair> deathSubscriptions; // for unsubscribing later

    private new void Awake() {

        base.Awake();

        // set up mechanic statuses early so scripts can change them earlier too
        Array mechanics = Enum.GetValues(typeof(MechanicType)); // get all mechanic type values

        // add all mechanic types to dictionary
        foreach (MechanicType mechanicType in mechanics)
            mechanicStatuses.Add(mechanicType, true); // set all mechanics to true by default

        itemSelector = FindObjectOfType<ItemSelector>();
        weaponPairs = new List<WeaponPair>();
        deathSubscriptions = new List<WeaponPair>();

        // disable all primary actions by default
        foreach (PrimaryAction action in GetComponents<PrimaryAction>())
            action.enabled = false;

        // disable all secondary actions by default
        foreach (SecondaryAction action in GetComponents<SecondaryAction>())
            action.enabled = false;

        // add each default weapon action pair to player
        foreach (WeaponPair pair in defaultWeaponPairs)
            AddWeapon(pair);

        // enable primary and secondary actions for first weapon (if it exists)
        if (weaponPairs.Count > 0) {

            WeaponPair weaponPair = weaponPairs[0];
            PrimaryAction primaryAction = weaponPair.GetPrimaryAction();
            SecondaryAction secondaryAction = weaponPair.GetSecondaryAction();

            if (primaryAction) {

                primaryAction.Initialize(weaponPair.GetWeaponData());
                primaryAction.enabled = true;

            }

            if (secondaryAction) {

                secondaryAction.Initialize(weaponPair.GetWeaponData());
                secondaryAction.enabled = true;

            }
        }
    }

    private new void Start() {

        base.Start();

        earthPrimaryAction = GetComponent<EarthPrimaryAction>();
        fireSecondaryAction = GetComponent<FireSecondaryAction>();
        timeSecondaryAction = GetComponent<TimeSecondaryAction>();

        UpdateCurrentWeapon(); // update current weapon

    }

    private void Update() {

        if (isDead)
            return; // player is dead, no need to update

        PrimaryAction currPrimaryAction = null;
        SecondaryAction currSecondaryAction = null;

        #region ACTIONS

        if (itemSelector.GetCurrSlotIndex() < weaponPairs.Count) { // make sure slot has a weapon in it

            WeaponPair pair = weaponPairs[itemSelector.GetCurrSlotIndex()]; // get current weapon action pair
            currPrimaryAction = pair.GetPrimaryAction();
            currSecondaryAction = pair.GetSecondaryAction();

        }

        /* PRIMARY ACTIONS */
        if (currPrimaryAction) { // make sure slot has a primary action in it

            if (currPrimaryAction.IsRegularAction()) { // primary action is regular action

                if ((currPrimaryAction.IsAutoAction() && Input.GetMouseButton(0)) || // primary action is auto
                    (!currPrimaryAction.IsAutoAction() && Input.GetMouseButtonDown(0))) { // primary action is not auto

                    currPrimaryAction.OnTriggerRegular(); // trigger regular primary action

                }
            } else { // primary action is hold action

                if (Input.GetMouseButtonDown(0)) // start hold
                    currPrimaryAction.OnTriggerHold(true);
                else if (Input.GetMouseButtonUp(0)) // stop hold
                    currPrimaryAction.OnTriggerHold(false);

            }
        }

        /* SECONDARY ACTIONS */
        if (currSecondaryAction) { // make sure slot has a weapon/secondary action in it

            if (currSecondaryAction.IsRegularAction()) { // secondary action is regular action

                if ((currSecondaryAction.IsAutoAction() && Input.GetMouseButton(1)) || // secondary action is auto
                    (!currSecondaryAction.IsAutoAction() && Input.GetMouseButtonDown(1))) { // secondary action is not auto

                    currSecondaryAction.OnTriggerRegular(); // trigger regular secondary action

                }
            } else { // secondary action is hold action

                if (Input.GetMouseButtonDown(1)) // start hold
                    currSecondaryAction.OnTriggerHold(true);
                else if (Input.GetMouseButtonUp(1)) // stop hold
                    currSecondaryAction.OnTriggerHold(false);

            }
        }

        #endregion

        #region WEAPON SWITCHING

        /* SCROLL WHEEL WEAPON SWITCHING */
        if (Input.mouseScrollDelta.y != 0f && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady() && !timeSecondaryAction.IsChanneling()) { // check if scroll wheel has moved, make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            // disable previous primary action if it exists
            if (currPrimaryAction)
                currPrimaryAction.enabled = false;

            // disable previous secondary action if it exists
            if (currSecondaryAction)
                currSecondaryAction.enabled = false;

            itemSelector.CycleSlot(Input.mouseScrollDelta.y < 0f ? 1 : -1);

            UpdateCurrentWeapon();

        }

        /* KEY WEAPON SWITCHING */
        for (int i = 0; i < itemSelector.GetSlotCount(); i++) {

            if (Input.GetKeyDown((i + 1) + "") && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady() && !timeSecondaryAction.IsChanneling()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

                // disable previous primary action if it exists
                if (currPrimaryAction)
                    currPrimaryAction.enabled = false;

                // disable previous secondary action if it exists
                if (currSecondaryAction)
                    currSecondaryAction.enabled = false;

                itemSelector.SelectSlot(i);

                UpdateCurrentWeapon();

            }
        }

        #endregion

    }

    private new void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Water"))  // barrier can save player from water
            health.Kill();

    }

    private new void OnDestroy() {

        base.OnDestroy();

        // unsubscribe from all events
        foreach (WeaponPair action in deathSubscriptions) {

            if (action.GetPrimaryAction())
                health.OnDeath -= action.GetPrimaryAction().OnDeath;

            if (action.GetSecondaryAction())
                health.OnDeath -= action.GetSecondaryAction().OnDeath;

        }
    }

    #region EARTH

    public Rock OnSummonRock(EarthPrimaryAction action, Rock rockPrefab, float maxThrowDuration) {

        rockSummonCoroutine = StartCoroutine(HandleSummonRock(action, rockPrefab, maxThrowDuration));
        return currRock;

    }

    private IEnumerator HandleSummonRock(EarthPrimaryAction action, Rock rockPrefab, float maxThrowDuration) {

        DisableAllMechanics(); // disable all mechanics while rock is being summoned (except primary action)
        EnableMechanic(MechanicType.PrimaryAction); // enable only primary action while rock is summoned
        DisableCoreScripts(); // disable all scripts while rock is being summoned (including weapon handler)

        currRock = Instantiate(rockPrefab, transform.position, Quaternion.identity); // instantiate rock (will play summon animation & rotate itself automatically)
        anim.SetBool("isSummoningRock", true); // play rock summon animation

        yield return null; // wait for animation to start
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length); // wait for animation to end (so rock can be dropped during animation because this coroutine won't be null)

        SetWeaponHandlerEnabled(true); // enable weapon handler when rock is fully summoned
        action.OnThrowReady(); // trigger throw ready event

        rockSummonCoroutine = null;

    }

    public void OnRockThrow() => StartCoroutine(HandleRockThrow()); // handle rock throw

    private IEnumerator HandleRockThrow() {

        // must wait for two frames to allow projectile to be fired
        yield return null;
        yield return null;

        charWeaponHandler.ShootStop(); // stop shooting weapon (to deal with infinite shooting bug | do this before disabling the core scripts)
        EnableCoreScripts(); // enable all scripts after rock is dropped/thrown (except weapon handler | don't want player to use weapon unless rock is fully summoned)
        SetWeaponHandlerEnabled(false); // disable weapon handler when rock is thrown
        EnableAllMechanics(); // enable all mechanics after rock is dropped/thrown

    }

    public void OnDestroyRock(bool activateMechanics) {

        if (rockSummonCoroutine != null) StopCoroutine(rockSummonCoroutine); // stop rock summon coroutine if it's running
        rockSummonCoroutine = null;

        Destroy(currRock.gameObject); // destroy rock

        corgiController.SetForce(Vector2.zero); // reset player movement
        anim.SetBool("isSummoningRock", false);

        if (activateMechanics) {

            EnableCoreScripts(); // enable all scripts after rock is dropped/thrown (except weapon handler | don't want player to use weapon unless rock is fully summoned)
            SetWeaponHandlerEnabled(false); // disable weapon handler when rock is dropped
            EnableAllMechanics(); // enable all mechanics after rock is dropped/thrown

        }
    }

    #endregion

    #region UTILITIES

    public void AddWeapon(WeaponPair weaponPair) {

        PrimaryAction primaryAction = weaponPair.GetPrimaryAction();
        SecondaryAction secondaryAction = weaponPair.GetSecondaryAction();

        // disable primary & secondary actions (if they exist)
        if (primaryAction) {

            primaryAction.Initialize(weaponPair.GetWeaponData());
            primaryAction.enabled = false;

        }

        if (secondaryAction) {

            secondaryAction.Initialize(weaponPair.GetWeaponData());
            secondaryAction.enabled = false;

        }

        itemSelector.AddWeapon(weaponPair.GetWeaponData()); // add weapon item to item selector

        if (primaryAction)
            health.OnDeath += primaryAction.OnDeath; // subscribe to death event

        if (secondaryAction)
            health.OnDeath += secondaryAction.OnDeath; // subscribe to death event

        weaponPairs.Add(weaponPair); // add to weapon action pairs list
        deathSubscriptions.Add(weaponPair); // add to list for unsubscribing later

    }

    public void UpdateCurrentWeapon() {

        if (itemSelector.GetCurrSlotIndex() < weaponPairs.Count) { // make sure weapon exists

            Weapon currWeapon = weaponPairs[itemSelector.GetCurrSlotIndex()].GetWeapon(); // set new weapon

            // placed here to make sure weapon exists before starting cooldown
            if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
            switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

            charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

            PrimaryAction currPrimaryAction = weaponPairs[itemSelector.GetCurrSlotIndex()].GetPrimaryAction();
            SecondaryAction currSecondaryAction = weaponPairs[itemSelector.GetCurrSlotIndex()].GetSecondaryAction();

            if (currPrimaryAction) { // make sure primary action exists

                currPrimaryAction = weaponPairs[itemSelector.GetCurrSlotIndex()].GetPrimaryAction(); // update primary action

                // enable new primary action if it exists
                if (currPrimaryAction)
                    currPrimaryAction.enabled = true;

            }

            if (currSecondaryAction) { // make sure secondary action exists

                currSecondaryAction = weaponPairs[itemSelector.GetCurrSlotIndex()].GetSecondaryAction(); // update secondary action

                // enable new secondary action if it exists
                if (currSecondaryAction)
                    currSecondaryAction.enabled = true;

            }
        } else {

            charWeaponHandler.ChangeWeapon(null, null); // remove weapon if it doesn't exist

        }

        itemSelector.UpdateWeaponHUD(); // update weapon HUD

    }

    public Weapon GetCurrentWeapon() => weaponPairs[itemSelector.GetCurrSlotIndex()].GetWeapon();

    private IEnumerator HandleSwitchCooldown() {

        charWeaponHandler.AbilityPermitted = false; // disable ability use
        yield return new WaitForSeconds(weaponPairs[itemSelector.GetCurrSlotIndex()].GetSwitchCooldown()); // wait for switch cooldown
        charWeaponHandler.AbilityPermitted = true; // enable ability use

    }

    protected override void OnDeath() {

        base.OnDeath();
        isDead = true;

    }

    protected override void OnRespawn() {

        base.OnRespawn();
        isDead = false;

    }

    public Vector2 GetDirectionRight() => character.IsFacingRight ? transform.right : -transform.right;

    public bool IsGrounded() => corgiController.State.IsGrounded;

    public Animator GetAnimator() => anim;

    #endregion

}
