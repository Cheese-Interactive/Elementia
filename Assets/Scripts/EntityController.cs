using MoreMountains.CorgiEngine;
using UnityEngine;

public class EntityController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform basePos;
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
    protected BaseEffect[] effects;
    protected CharacterHandleWeapon charWeaponHandler;

    [Header("Animations")]
    [SerializeField][Tooltip("Minimum movement threshold to trigger walking animation")] private float minMovementThreshold;

    [Header("Death")]
    protected bool isDead; // to deal with death/respawn delay

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
        effects = GetComponents<BaseEffect>();
        charWeaponHandler = GetComponent<CharacterHandleWeapon>();

        // subscribe to on death event
        foreach (BaseEffect effect in effects)
            health.OnDeath += effect.RemoveEffect;

        health.OnDeath += OnDeath;
        health.OnRevive += OnRespawn;

    }

    protected void Start() => anim = GetComponent<Animator>();

    protected void Update() {

        // player is dead, no need to update
        if (isDead)
            return;

        anim.SetBool("isWalking", IsGrounded() && Mathf.Abs(corgiController.ForcesApplied.x) > minMovementThreshold && !isDead); // play walking animation if player is grounded, moving, and not dead (place before dead check to allow walking to be reset)

    }

    protected void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Water"))
            health.Kill();

    }

    protected void OnDestroy() {

        // remove subscriptions to on death event
        foreach (BaseEffect effect in effects)
            health.OnDeath -= effect.RemoveEffect;

    }

    #region UTILITIES

    public void EnableCoreScripts() {

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

    public void SetCharacterEnabled(bool enabled) {

        if (character)
            character.enabled = enabled;

    }

    public void SetWeaponHandlerEnabled(bool enabled) {

        if (charWeaponHandler)
            charWeaponHandler.AbilityPermitted = enabled;

    }

    public void SetWeaponAimEnabled(bool enabled) {

        if (charWeaponHandler)
            charWeaponHandler.CurrentWeapon.GetComponent<WeaponAim>().enabled = enabled;

    }

    public void SetInvulnerable(bool invulnerable) {

        if (health)
            health.Invulnerable = invulnerable;

    }

    public Vector2 GetBottomPosition() => basePos.position;

    protected virtual void OnDeath() => isDead = true;

    protected virtual void OnRespawn() => isDead = false;

    public bool IsGrounded() => corgiController.State.IsGrounded;

    #endregion

}
