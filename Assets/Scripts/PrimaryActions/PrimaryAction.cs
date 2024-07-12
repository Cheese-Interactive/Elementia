using MoreMountains.CorgiEngine;
using UnityEngine;

public abstract class PrimaryAction : MonoBehaviour {

    [Header("References")]
    protected PlayerController playerController;
    protected CharacterHandleWeapon charWeaponHandler;
    protected Meter currMeter;
    private MeterController meterController;
    private WeaponData weaponData;

    [Header("Settings")]
    [SerializeField] protected float primaryCooldown;
    [SerializeField] private bool isPrimaryAuto;
    [SerializeField] protected bool canUseInAir;

    [Header("Actions")]
    protected bool isReady;

    protected void Awake() {

        playerController = GetComponent<PlayerController>();
        charWeaponHandler = GetComponent<CharacterHandleWeapon>();
        meterController = FindObjectOfType<MeterController>();

    }

    protected void Start() => isReady = false; // primary actions are not ready by default because they have a switch cooldown

    // runs before weapon is switched
    private void OnDisable() => charWeaponHandler.CurrentWeapon.OnShoot -= OnShoot; // remove shoot event

    public void Initialize(WeaponData weaponData) => this.weaponData = weaponData;

    public virtual void OnTriggerRegular() { }

    public virtual void OnTriggerHold(bool startHold) { }

    public bool IsAutoAction() => isPrimaryAuto;

    public virtual void OnShoot() => currMeter = CreateMeter(charWeaponHandler.CurrentWeapon.TimeBetweenUses); // create new meter for cooldown (use the weapon cooldown instead of primary action cooldown)

    public float GetCooldown() => primaryCooldown;

    public virtual void OnSwitchCooldownComplete() { isReady = true; }

    protected void ReadyAction() => isReady = true;

    public virtual void OnDeath() {

        // cancel all secondary action cooldowns if player dies
        CancelInvoke("ReadyAction");
        isReady = true;

    }

    public Meter CreateMeter(float cooldownDuration) => meterController.CreateMeter(cooldownDuration, weaponData.GetPrimaryIcon(), weaponData.GetWeaponColor());

    public abstract bool IsRegularAction(); // regular or hold action

}
