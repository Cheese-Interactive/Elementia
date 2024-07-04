using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityController : MonoBehaviour {

    [Header("References")]
    protected Animator anim;
    protected Character character;
    protected CorgiController corgiController;
    protected Health health;
    protected CharacterHorizontalMovement charMovement;
    protected AIWalk aiWalk;
    protected CharacterCrouch charCrouch;
    protected CharacterDash charDash;
    protected CharacterDive charDive;
    protected CharacterDangling charDangling;
    protected CharacterJump charJump;
    protected CharacterLookUp charLookUp;
    protected CharacterGrip charGrip;
    protected CharacterWallClinging charWallCling;
    protected CharacterWalljump charWallJump;
    protected CharacterLadder charLadder;
    protected CharacterButtonActivation charButton;
    protected DamageOnTouch damageOnTouch;
    protected SlowEffect slowEffect;
    protected BurnEffect burnEffect;
    protected TimeEffect timeEffect;
    protected CharacterHandleWeapon charWeaponHandler;

    [Header("Mechanics")]
    protected Dictionary<MechanicType, bool> mechanicStatuses;

    protected void Awake() {

        character = GetComponent<Character>();
        corgiController = GetComponent<CorgiController>();
        health = GetComponent<Health>();
        charMovement = GetComponent<CharacterHorizontalMovement>();
        aiWalk = GetComponent<AIWalk>();
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
        damageOnTouch = GetComponent<DamageOnTouch>();
        slowEffect = GetComponent<SlowEffect>();
        burnEffect = GetComponent<BurnEffect>();
        timeEffect = GetComponent<TimeEffect>();
        charWeaponHandler = GetComponent<CharacterHandleWeapon>();

        // set up mechanic statuses early so scripts can change them earlier too
        mechanicStatuses = new Dictionary<MechanicType, bool>();

        health.OnDeath += slowEffect.RemoveEffect; // remove slow effect on death
        health.OnDeath += burnEffect.RemoveEffect; // remove burn effect on death
        health.OnDeath += timeEffect.RemoveEffect; // remove time effect on death
        health.OnDeath += OnDeath;
        health.OnRevive += OnRespawn;

    }

    protected void Start() => anim = GetComponent<Animator>();

    protected void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Water"))
            health.Kill();

    }

    protected void OnDisable() {

        // remove subscription to on death events
        health.OnDeath -= slowEffect.RemoveEffect;
        health.OnDeath -= burnEffect.RemoveEffect;
        health.OnDeath -= timeEffect.RemoveEffect;

    }

    protected void OnDestroy() {

        // remove subscription to on death event
        health.OnDeath -= slowEffect.RemoveEffect;
        health.OnDeath -= burnEffect.RemoveEffect;
        health.OnDeath -= timeEffect.RemoveEffect;

    }

    #region MECHANICS

    public void EnableAllMechanics() {

        // enable all mechanics
        foreach (MechanicType mechanicType in mechanicStatuses.Keys.ToList())
            mechanicStatuses[mechanicType] = true;

    }

    public void EnableMechanic(MechanicType mechanicType) => mechanicStatuses[mechanicType] = true;

    public void DisableAllMechanics() {

        // disable all mechanics
        foreach (MechanicType mechanicType in mechanicStatuses.Keys.ToList())
            mechanicStatuses[mechanicType] = false;

        // send to idle animation
        print(false);
        anim.SetBool("Walking", false); // stop moving animation

    }

    public void DisableMechanic(MechanicType mechanicType) => mechanicStatuses[mechanicType] = false;

    public bool IsMechanicEnabled(MechanicType mechanicType) => mechanicStatuses[mechanicType];

    #endregion

    #region UTILITIES

    public void EnableCoreScripts() {

        if (character)
            character.enabled = true;

        if (corgiController)
            corgiController.enabled = true;

        if (charMovement)
            charMovement.AbilityPermitted = true;

        if (aiWalk)
            aiWalk.enabled = true;

        if (charCrouch)
            charCrouch.AbilityPermitted = true;

        if (charDash)
            charDash.AbilityPermitted = true;

        if (charDive)
            charDive.AbilityPermitted = true;

        if (charDangling)
            charDangling.AbilityPermitted = true;

        if (charJump)
            charJump.AbilityPermitted = true;

        if (charLookUp)
            charLookUp.AbilityPermitted = true;

        if (charGrip)
            charGrip.AbilityPermitted = true;

        if (charWallCling)
            charWallCling.AbilityPermitted = true;

        if (charWallJump)
            charWallJump.AbilityPermitted = true;

        if (charLadder)
            charLadder.AbilityPermitted = true;

        if (charButton)
            charButton.AbilityPermitted = true;

        SetWeaponHandlerEnabled(true);

    }

    public void DisableCoreScripts() {

        if (character)
            character.enabled = false;

        if (corgiController)
            corgiController.enabled = false;

        if (charMovement)
            charMovement.AbilityPermitted = false;

        if (aiWalk)
            aiWalk.enabled = false;

        if (charCrouch)
            charCrouch.AbilityPermitted = false;

        if (charDash)
            charDash.AbilityPermitted = false;

        if (charDive)
            charDive.AbilityPermitted = false;

        if (charDangling)
            charDangling.AbilityPermitted = false;

        if (charJump)
            charJump.AbilityPermitted = false;

        if (charLookUp)
            charLookUp.AbilityPermitted = false;

        if (charGrip)
            charGrip.AbilityPermitted = false;

        if (charWallCling)
            charWallCling.AbilityPermitted = false;

        if (charWallJump)
            charWallJump.AbilityPermitted = false;

        if (charLadder)
            charLadder.AbilityPermitted = false;

        if (charButton)
            charButton.AbilityPermitted = false;

        SetWeaponHandlerEnabled(false);

    }

    public void SetWeaponHandlerEnabled(bool enabled) {

        if (charWeaponHandler)
            charWeaponHandler.AbilityPermitted = enabled;

    }

    protected virtual void OnRespawn() { }

    protected virtual void OnDeath() { }

    #endregion
}
