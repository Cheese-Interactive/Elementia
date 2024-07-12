using MoreMountains.CorgiEngine;
using UnityEngine;

public abstract class SecondaryAction : MonoBehaviour {

    [Header("References")]
    protected PlayerController playerController;
    protected CharacterHandleWeapon charWeaponHandler;
    protected Meter currMeter;
    private MeterController meterController;
    private WeaponData weaponData;

    [Header("Settings")]
    [SerializeField] private bool isSecondaryAuto;
    [SerializeField] protected float secondaryCooldown;
    [SerializeField] protected bool canUseInAir;

    [Header("Actions")]
    protected bool isReady;

    protected void Awake() {

        playerController = GetComponent<PlayerController>();
        charWeaponHandler = GetComponent<CharacterHandleWeapon>();
        meterController = FindObjectOfType<MeterController>();

    }

    protected void OnEnable() => currMeter = CreateMeter(charWeaponHandler.CurrentWeapon.TimeBetweenUses);

    protected void OnDisable() {

        CancelInvoke("ReadyAction"); // cancel invoke on disable

        // destroy meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject); // destroy meter

        isReady = false;

    }

    public void Initialize(WeaponData weaponData) => this.weaponData = weaponData;

    public virtual void OnTriggerRegular() { }

    public virtual void OnTriggerHold(bool startHold) { }

    public bool IsAutoAction() => isSecondaryAuto;

    public float GetCooldown() => secondaryCooldown;

    public virtual void OnSwitchCooldownComplete() { isReady = true; }

    protected void ReadyAction() => isReady = true;

    public virtual void OnDeath() {

        // cancel all secondary action cooldowns if player dies
        CancelInvoke("ReadyAction");
        isReady = true;

    }

    public Meter CreateMeter(float cooldownDuration) => meterController.CreateMeter(cooldownDuration, weaponData.GetSecondaryIcon(), weaponData.GetWeaponColor());

    public abstract bool IsRegularAction(); // regular or hold action

}
