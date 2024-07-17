using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController {

    [Header("References")]
    private GameManager gameManager;

    [Header("Weapon Selector")]
    private WeaponSelector weaponSelector;

    [Header("Weapons")]
    [SerializeField] private WeaponData[] defaultWeapons; // default weapons (from inspector) that player begins with
    [SerializeField] private Weapon blankWeapon;
    private WeaponDatabase weaponDatabase;
    private WeaponPair currWeaponPair; // to avoid searching dictionary every frame
    private Coroutine switchCoroutine;

    [Header("Earth")]
    private Rock currRock;
    private Coroutine rockSummonCoroutine;

    [Header("Death")]
    private bool isDead; // to deal with death delay

    [Header("Health")]
    private List<WeaponPair> deathSubscriptions; // for unsubscribing later

    private new void Awake() {

        base.Awake();

        weaponDatabase = GetComponent<WeaponDatabase>();
        weaponSelector = FindObjectOfType<WeaponSelector>();
        deathSubscriptions = new List<WeaponPair>();

        weaponDatabase.Initialize(); // initialize weapon database

        // disable all primary actions
        foreach (PrimaryAction primaryAction in GetComponents<PrimaryAction>())
            primaryAction.enabled = false;

        // disable all secondary actions
        foreach (SecondaryAction secondaryAction in GetComponents<SecondaryAction>())
            secondaryAction.enabled = false;

    }

    private new void Start() {

        base.Start();

        gameManager = FindObjectOfType<GameManager>();

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
        if (Input.mouseScrollDelta.y != 0f && (primaryAction ? !primaryAction.IsUsing() : true) && (secondaryAction ? !secondaryAction.IsUsing() : true)) { // make sure primary and secondary actions are not being used if they exist

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

            if (i == weaponSelector.GetCurrSlotIndex()) continue; // skip current slot (already selected)

            if (Input.GetKeyDown((i + 1) + "") && (primaryAction ? !primaryAction.IsUsing() : true) && (secondaryAction ? !secondaryAction.IsUsing() : true)) { // make sure primary and secondary actions are not being used if they exist

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

        }
    }

    #endregion

    #region UTILITIES

    // IMPORTANT: DO NOT USE THIS METHOD TO ADD WEAPONS, USE WEAPON SELECTOR ONE INSTEAD
    public void AddWeapon(WeaponData weaponData) {

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

            PrimaryAction primaryAction = currWeaponPair.GetPrimaryAction(); // get primary action
            SecondaryAction secondaryAction = currWeaponPair.GetSecondaryAction(); // get secondary action

            if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
            switchCoroutine = StartCoroutine(HandleSwitchCooldown(primaryAction, secondaryAction)); // start weapon cooldown

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

            if (switchCoroutine != null) StopCoroutine(switchCoroutine); // stop switch coroutine if it's running
            charWeaponHandler.ChangeWeapon(blankWeapon, blankWeapon.WeaponID); // equip blank weapon if no weapon exists

        }
    }

    public Weapon GetCurrentWeapon() => currWeaponPair.GetWeapon();

    public WeaponData[] GetDefaultWeapons() => defaultWeapons;

    private IEnumerator HandleSwitchCooldown(PrimaryAction primaryAction, SecondaryAction secondaryAction) {

        charWeaponHandler.AbilityPermitted = false; // disable ability use
        primaryAction.enabled = false; // disable primary action
        secondaryAction.enabled = false; // disable secondary action

        yield return new WaitForSeconds(currWeaponPair.GetWeapon().TimeBetweenUses); // wait for switch cooldown

        secondaryAction.enabled = true; // enable secondary action
        primaryAction.enabled = true; // enable primary action
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

        UpdateCurrentWeapon(); // update current weapon (to activate cooldowns)
        gameManager.ResetAllResettables(); // reset all resettables

    }

    public Vector2 GetDirectionRight() => character.IsFacingRight ? transform.right : -transform.right;

    public bool IsGrounded() => corgiController.State.IsGrounded;

    public Animator GetAnimator() => anim;

    #endregion

}
