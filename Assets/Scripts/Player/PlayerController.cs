using MoreMountains.CorgiEngine;
using UnityEngine;

public class PlayerController : EntityController {

    [Header("References")]
    private LevelManager levelManager;
    private GameManager gameManager;

    [Header("Weapon Selector")]
    private WeaponSelector weaponSelector;

    [Header("Weapons")]
    [SerializeField] private WeaponData[] defaultWeapons; // default weapons (from inspector) that player begins with
    [SerializeField] private Weapon blankWeapon;
    private WeaponDatabase weaponDatabase;
    private WeaponPair currWeaponPair; // to avoid searching dictionary every frame

    [Header("Interacting")]
    [SerializeField] private LayerMask interactLayer; // layer mask for interactable objects
    [SerializeField] private float interactRadius;
    private InteractIndicator interactIndicator;

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
        levelManager = FindObjectOfType<LevelManager>();
        interactIndicator = FindObjectOfType<InteractIndicator>(true);
        interactIndicator.Initialize(); // initialize interact indicator

    }

    private new void Update() {

        if (Input.GetKeyDown(KeyCode.Escape)) // toggle game pause
            gameManager.TogglePause();

        // player is dead or game is paused, no need to update
        if (isDead || gameManager.IsPaused())
            return;

        base.Update();

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

        #region INTERACTING

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer); // get all colliders in interact radius
        bool interactableFound = false; // flag to check if interactable object is found

        foreach (Collider2D collider in colliders) {

            Interactable interactable = collider.GetComponent<Interactable>(); // get first interactable component

            if (interactable) { // make sure object is interactable

                if (!interactable.IsInteractable()) continue; // skip object if it's not interactable

                interactIndicator.Show(); // show interact indicator

                if (Input.GetKeyDown(KeyCode.E)) { // interact key pressed

                    interactable.Interact(); // interact with object
                    interactIndicator.OnInteract();

                }

                interactableFound = true; // set flag to true
                break;

            }
        }

        if (!interactableFound) // no interactable object found
            interactIndicator.Hide(); // hide interact indicator

        #endregion

        #region CLIMBING

        anim.SetBool("isClimbing", charLadder.CurrentLadderClimbingSpeed.magnitude > 0f); // play climbing animation only when player is climbing (to prevent animation from playing player is still on the ladder)

        #endregion

    }

    private new void OnTriggerEnter2D(Collider2D collision) {

        // kill player if they enter water or hazards
        if (collision.CompareTag("Water") || collision.CompareTag("Hazards"))
            health.ForceKill(); // force kill player so invincibility doesn't protect player

        // disable cooldowns when player enters generator field
        if (collision.CompareTag("GeneratorField"))
            gameManager.SetCooldownsEnabled(false);

    }

    private void OnTriggerExit2D(Collider2D collision) {

        // enable cooldowns when player exits generator field
        if (collision.CompareTag("GeneratorField"))
            gameManager.SetCooldownsEnabled(true);

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
            if (primaryAction)
                primaryAction.enabled = true;

            // initialize & enable new secondary action if it exists
            if (secondaryAction)
                secondaryAction.enabled = true;

        } else {

            charWeaponHandler.ChangeWeapon(blankWeapon, blankWeapon.WeaponID); // equip blank weapon if no weapon exists
            weaponSelector.ResetCooldownValues(); // reset cooldown values

        }
    }

    public Weapon GetCurrentWeapon() => currWeaponPair.GetWeapon();

    public WeaponData[] GetDefaultWeapons() => defaultWeapons;

    protected override void OnDeath() {

        base.OnDeath();
        SetWeaponAimEnabled(false); // disable weapon aiming
        ResetAllAnimations(); // reset all animations
        weaponSelector.ResetCooldownValues(); // reset cooldown values
        gameManager.ResetAllResettables(levelManager.RespawnDelay); // reset all resettables

    }

    protected override void OnRespawn() {

        base.OnRespawn();
        SetWeaponAimEnabled(true); // enable weapon aiming

    }

    // IMPORTANT: RESPAWN METHOD GETS CALLED AT THE BEGINNING OF THE GAME, SO THE WEAPON IS ALREADY BEING UPDATED AT THE START

    public Vector2 GetDirectionRight() => character.IsFacingRight ? transform.right : -transform.right;

    public bool IsDead() => isDead;

    #endregion

}
