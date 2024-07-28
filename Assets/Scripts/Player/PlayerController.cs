using MoreMountains.CorgiEngine;
using UnityEngine;

public class PlayerController : EntityController {

    [Header("References")]
    private CooldownManager cooldownManager;
    private GameManager gameManager;

    [Header("Weapon Selector")]
    private WeaponSelector weaponSelector;

    [Header("Weapons")]
    [SerializeField] private WeaponData[] defaultWeapons; // default weapons (from inspector) that player begins with
    [SerializeField] private Weapon blankWeapon;
    private WeaponDatabase weaponDatabase;
    private WeaponPair currWeaponPair; // to avoid searching dictionary every frame

    [Header("Animations")]
    [SerializeField][Tooltip("Minimum movement threshold to trigger walking animation")] private float minMovementThreshold;

    [Header("Death")]
    private bool isDead; // to deal with death delay

    private new void Awake() {

        base.Awake();

        weaponDatabase = GetComponent<WeaponDatabase>();
        weaponSelector = FindObjectOfType<WeaponSelector>();

        weaponDatabase.Initialize(); // initialize weapon database

        // disable all primary actions
        foreach (PrimaryAction primaryAction in GetComponents<PrimaryAction>()) {

            primaryAction.Initialize(health);
            primaryAction.enabled = false;

        }

        // disable all secondary actions
        foreach (SecondaryAction secondaryAction in GetComponents<SecondaryAction>()) {

            secondaryAction.Initialize(health);
            secondaryAction.enabled = false;

        }
    }

    private new void Start() {

        base.Start();

        gameManager = FindObjectOfType<GameManager>();
        cooldownManager = FindObjectOfType<CooldownManager>();

    }

    private void Update() {

        anim.SetBool("isWalking", IsGrounded() && Mathf.Abs(corgiController.ForcesApplied.x) > minMovementThreshold && !isDead); // play walking animation if player is grounded, moving, and not dead (place before dead check to allow walking to be reset)

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

        if (collision.CompareTag("Water"))
            health.ForceKill(); // force kill player so invincibility doesn't protect player from drowning

    }

    #region UTILITIES

    // IMPORTANT: DO NOT USE THIS METHOD TO ADD WEAPONS, USE WEAPON SELECTOR ONE INSTEAD
    public void AddWeapon(WeaponData weaponData) {

        WeaponPair weaponPair = weaponDatabase.GetWeaponPair(weaponData); // get weapon pair from database
        PrimaryAction primaryAction = weaponPair.GetPrimaryAction();
        SecondaryAction secondaryAction = weaponPair.GetSecondaryAction();

        // make sure weapon doesn't have a cooldown
        if (weaponPair.GetWeapon().TimeBetweenUses > 0f)
            Debug.LogWarning("Weapon " + weaponPair.GetWeapon().name + " has a set cooldown on its weapon object, which will cause issues with primary action cooldowns.");

        // disable primary & secondary actions (if they exist)
        if (primaryAction)
            primaryAction.enabled = false;

        if (secondaryAction)
            secondaryAction.enabled = false;

        if (primaryAction)
            health.OnDeath += primaryAction.OnDeath; // subscribe to death event

        if (secondaryAction)
            health.OnDeath += secondaryAction.OnDeath; // subscribe to death event

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

            // initialize & enable new primary action if it exists
            if (primaryAction) {

                primaryAction.enabled = true;
                weaponSelector.SetPrimaryCooldownValue(primaryAction.GetNormalizedCooldown(), primaryAction.GetCooldownTimer());

            }

            // initialize & enable new secondary action if it exists
            if (secondaryAction) {

                secondaryAction.enabled = true;
                weaponSelector.SetSecondaryCooldownValue(secondaryAction.GetNormalizedCooldown(), secondaryAction.GetCooldownTimer());

            }
        } else {

            charWeaponHandler.ChangeWeapon(blankWeapon, blankWeapon.WeaponID); // equip blank weapon if no weapon exists
            weaponSelector.ResetCooldownValues(); // reset cooldown values

        }
    }

    public Weapon GetCurrentWeapon() => currWeaponPair.GetWeapon();

    public WeaponData[] GetDefaultWeapons() => defaultWeapons;

    protected override void OnDeath() {

        base.OnDeath();
        isDead = true;
        cooldownManager.ClearCooldownData(); // clear all cooldown data
        weaponSelector.ResetCooldownValues(); // reset cooldown values

    }

    // IMPORTANT: THIS GETS CALLED AT THE BEGINNING OF THE GAME, SO THE WEAPON IS ALREADY BEING UPDATED AT THE START
    protected override void OnRespawn() {

        base.OnRespawn();
        isDead = false;
        gameManager.ResetAllResettables(); // reset all resettables

    }

    public Vector2 GetDirectionRight() => character.IsFacingRight ? transform.right : -transform.right;

    public bool IsGrounded() => corgiController.State.IsGrounded;

    public Animator GetAnimator() => anim;

    #endregion

}
