using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController {

    [Header("Weapon Selector")]
    private WeaponSelector weaponSelector;

    [Header("Weapons/Primary/Secondary Actions")]
    [SerializeField] private WeaponData[] defaultWeapons; // default weapons (from inspector) that player begins with
    [SerializeField] private Weapon blankWeapon;
    private WeaponDatabase weaponDatabase;
    private WeaponPair currWeaponPair; // to avoid searching dictionary every frame
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

        weaponDatabase = GetComponent<WeaponDatabase>();
        weaponSelector = FindObjectOfType<WeaponSelector>();
        deathSubscriptions = new List<WeaponPair>();

        weaponDatabase.Initialize(); // initialize weapon database

        // disable all primary actions
        foreach (PrimaryAction primaryAction in GetComponents<PrimaryAction>()) // set player controller for all primary actions
            primaryAction.enabled = false;

        // disable all secondary actions
        foreach (SecondaryAction secondaryAction in GetComponents<SecondaryAction>()) // set player controller for all secondary actions
            secondaryAction.enabled = false;

    }

    private new void Start() {

        base.Start();

        earthPrimaryAction = GetComponent<EarthPrimaryAction>();
        fireSecondaryAction = GetComponent<FireSecondaryAction>();
        timeSecondaryAction = GetComponent<TimeSecondaryAction>();

    }

    private void Update() {

        if (isDead)
            return; // player is dead, no need to update

        PrimaryAction primaryAction = null;
        SecondaryAction secondaryAction = null;

        #region ACTIONS

        if (weaponSelector.GetCurrentWeapon()) { // make sure slot has a weapon in it

            primaryAction = currWeaponPair.GetPrimaryAction();
            secondaryAction = currWeaponPair.GetSecondaryAction();

        }

        /* PRIMARY ACTIONS */
        if (primaryAction) { // make sure slot has a primary action in it

            if (primaryAction.IsRegularAction()) { // primary action is regular action

                if ((primaryAction.IsAutoAction() && Input.GetMouseButton(0)) || // primary action is auto
                    (!primaryAction.IsAutoAction() && Input.GetMouseButtonDown(0))) { // primary action is not auto

                    primaryAction.OnTriggerRegular(); // trigger regular primary action

                }
            } else { // primary action is hold action

                if (Input.GetMouseButtonDown(0)) // start hold
                    primaryAction.OnTriggerHold(true);
                else if (Input.GetMouseButtonUp(0)) // stop hold
                    primaryAction.OnTriggerHold(false);

            }
        }

        /* SECONDARY ACTIONS */
        if (secondaryAction) { // make sure slot has a weapon/secondary action in it

            if (secondaryAction.IsRegularAction()) { // secondary action is regular action

                if ((secondaryAction.IsAutoAction() && Input.GetMouseButton(1)) || // secondary action is auto
                    (!secondaryAction.IsAutoAction() && Input.GetMouseButtonDown(1))) { // secondary action is not auto

                    secondaryAction.OnTriggerRegular(); // trigger regular secondary action

                }
            } else { // secondary action is hold action

                if (Input.GetMouseButtonDown(1)) // start hold
                    secondaryAction.OnTriggerHold(true);
                else if (Input.GetMouseButtonUp(1)) // stop hold
                    secondaryAction.OnTriggerHold(false);

            }
        }

        #endregion

        #region WEAPON SWITCHING

        /* SCROLL WHEEL WEAPON SWITCHING */
        if (Input.mouseScrollDelta.y != 0f && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady() && !timeSecondaryAction.IsChanneling()) { // check if scroll wheel has moved, make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

            // disable previous primary action if it exists
            if (primaryAction)
                primaryAction.enabled = false;

            // disable previous secondary action if it exists
            if (secondaryAction)
                secondaryAction.enabled = false;

            weaponSelector.CycleSlot(Input.mouseScrollDelta.y < 0f ? 1 : -1); // cycle slot based on scroll wheel direction

        }

        /* KEY WEAPON SWITCHING */
        for (int i = 0; i < weaponSelector.GetSlotCount(); i++) {

            if (Input.GetKeyDown((i + 1) + "") && !fireSecondaryAction.IsFlamethrowerEquipped() && !earthPrimaryAction.IsSummoningRock() && !earthPrimaryAction.IsRockThrowReady() && !timeSecondaryAction.IsChanneling()) { // make sure flamethrower isn't equipped, rock isn't being summoned, & rock throw isn't ready before switching

                // disable previous primary action if it exists
                if (primaryAction)
                    primaryAction.enabled = false;

                // disable previous secondary action if it exists
                if (secondaryAction)
                    secondaryAction.enabled = false;

                weaponSelector.SelectSlot(i); // select slot based on key pressed

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
        foreach (WeaponPair weaponPair in deathSubscriptions) {

            if (weaponPair.GetPrimaryAction())
                health.OnDeath -= weaponPair.GetPrimaryAction().OnDeath;

            if (weaponPair.GetSecondaryAction())
                health.OnDeath -= weaponPair.GetSecondaryAction().OnDeath;

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
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // wait for animation to end (so rock can be dropped during animation because this coroutine won't be null)

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

    // IMPORTANT: DO NOT USE THIS METHOD TO ADD WEAPONS, USE WEAPON SELECTOR ONE INSTEAD
    public void SetWeapon(WeaponData weaponData, int slotIndex) {

        WeaponPair weaponPair = weaponDatabase.GetWeaponPair(weaponData); // get weapon pair from database
        PrimaryAction primaryAction = weaponPair.GetPrimaryAction();
        SecondaryAction secondaryAction = weaponPair.GetSecondaryAction();

        // disable primary & secondary actions (if they exist)
        if (primaryAction) {

            primaryAction.Initialize(weaponData);
            primaryAction.enabled = false;

        }

        if (secondaryAction) {

            secondaryAction.Initialize(weaponData);
            secondaryAction.enabled = false;

        }

        if (primaryAction)
            health.OnDeath += primaryAction.OnDeath; // subscribe to death event

        if (secondaryAction)
            health.OnDeath += secondaryAction.OnDeath; // subscribe to death event

        deathSubscriptions.Add(weaponPair); // add to list for unsubscribing later

    }

    // IMPORTANT: DO NOT USE THIS METHOD TO REMOVE WEAPONS, USE WEAPON SELECTOR ONE INSTEAD
    public void RemoveWeapon(int slotIndex) {

        WeaponPair weaponPair = weaponDatabase.GetWeaponPair(weaponSelector.GetWeaponAt(slotIndex)); // get weapon pair from database
        PrimaryAction primaryAction = weaponPair.GetPrimaryAction();
        SecondaryAction secondaryAction = weaponPair.GetSecondaryAction();

        // disable primary & secondary actions (if they exist)
        if (primaryAction)
            primaryAction.enabled = false;

        if (secondaryAction)
            secondaryAction.enabled = false;

        if (primaryAction)
            health.OnDeath -= primaryAction.OnDeath; // unsubscribe from death event

        if (secondaryAction)
            health.OnDeath -= secondaryAction.OnDeath; // unsubscribe from death event

        deathSubscriptions.Remove(weaponPair); // remove from list

    }

    // IMPORTANT: DO NOT USE THIS METHOD TO UPDATE WEAPONS, USE WEAPON SELECTOR ONE INSTEAD
    public void UpdateCurrentWeapon() {

        WeaponData weaponData = weaponSelector.GetCurrentWeapon(); // get weapon data from current slot

        if (weaponData) { // make sure weapon exists

            currWeaponPair = weaponDatabase.GetWeaponPair(weaponData); // update current weapon pair
            Weapon currWeapon = currWeaponPair.GetWeapon(); // set new weapon
            charWeaponHandler.ChangeWeapon(currWeapon, currWeapon.WeaponID); // change weapon

            // placed here to make sure weapon exists before starting cooldown
            if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
            switchCoroutine = StartCoroutine(HandleSwitchCooldown()); // start weapon cooldown

            PrimaryAction primaryAction = currWeaponPair.GetPrimaryAction(); // get primary action
            SecondaryAction secondaryAction = currWeaponPair.GetSecondaryAction(); // get secondary action

            // initialize & enable new primary action if it exists
            if (primaryAction) {

                primaryAction.Initialize(weaponData);
                primaryAction.enabled = true;

            }

            // initialize & enable new secondary action if it exists
            if (secondaryAction) {

                secondaryAction.Initialize(weaponData);
                secondaryAction.enabled = true;

            }

        } else {

            charWeaponHandler.ChangeWeapon(blankWeapon, blankWeapon.WeaponID); // equip blank weapon if no weapon exists

        }
    }

    public Weapon GetCurrentWeapon() => currWeaponPair.GetWeapon();

    public WeaponData[] GetDefaultWeapons() => defaultWeapons;

    private IEnumerator HandleSwitchCooldown() {

        charWeaponHandler.AbilityPermitted = false; // disable ability use
        yield return new WaitForSeconds(currWeaponPair.GetSwitchCooldown()); // wait for switch cooldown
        charWeaponHandler.AbilityPermitted = true; // enable ability use

        currWeaponPair.GetPrimaryAction().OnSwitchCooldownComplete(); // trigger primary switch cooldown complete event
        currWeaponPair.GetSecondaryAction().OnSwitchCooldownComplete(); // trigger secondary switch cooldown complete event

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
