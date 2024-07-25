using MoreMountains.CorgiEngine;
using UnityEngine;

public abstract class Action : MonoBehaviour {

    [Header("References")]
    protected PlayerController playerController;
    protected CharacterHandleWeapon charWeaponHandler;
    protected WeaponSelector weaponSelector;
    private CooldownManager cooldownManager;
    protected Health health;

    [Header("Settings")]
    [SerializeField] protected float cooldown;
    [SerializeField] private bool isAuto;
    [SerializeField] protected bool canUseInAir;
    protected float cooldownTimer;

    public void Initialize(Health health) {

        this.health = health;
        health.OnDeath += OnDeath; // initialize action on death

    }

    protected void Awake() {

        playerController = GetComponent<PlayerController>();
        charWeaponHandler = GetComponent<CharacterHandleWeapon>();
        weaponSelector = FindObjectOfType<WeaponSelector>();
        cooldownManager = FindObjectOfType<CooldownManager>();

    }

    protected void OnEnable() {

        CooldownData cooldownData = cooldownManager.GetCooldown(this); // get cooldown data

        if (cooldownData != null) { // if cooldown data exists

            float cooldown = cooldownData.GetCooldownTimer() - (Time.time - cooldownData.GetUnequipTime()); // calculate current cooldown
            cooldownTimer = cooldown > 0f ? cooldown : 0f; // set cooldown timer to current cooldown or 0 if cooldown has ended

        }
    }

    // runs before weapon is switched
    protected void OnDisable() => cooldownManager.SetCooldown(this, cooldownTimer, Time.time); // set cooldown data

    private void OnDestroy() => health.OnDeath -= OnDeath; // remove action on death

    protected void Update() {

        // tick down cooldown timer and clamp to 0
        if (cooldownTimer > 0f) {

            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer < 0f)
                cooldownTimer = 0f;

        }
    }

    public virtual void OnTriggerRegular() { }

    public virtual void OnTriggerHold(bool startHold) { }

    public bool IsAutoAction() => isAuto;

    public virtual void OnDeath() => cooldownTimer = 0f; // reset cooldown timer on death (allow actions to be used again)

    public abstract bool IsRegularAction(); // regular or hold action

    public abstract bool IsUsing(); // is action being used (used for hold actions)

    public float GetNormalizedCooldown() => cooldownTimer / cooldown;

    public float GetCooldownTimer() => cooldownTimer;

}
