using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SecondaryAction : MonoBehaviour {

    [Header("References")]
    protected PlayerController playerController;
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
        meterController = FindObjectOfType<MeterController>();

    }

    protected void Start() => isReady = true;

    public void Initialize(WeaponData weaponData) => this.weaponData = weaponData;

    public virtual void OnTriggerRegular() { }

    public virtual void OnTriggerHold(bool startHold) { }

    public bool IsAutoAction() => isSecondaryAuto;

    public float GetCooldown() => secondaryCooldown;

    protected void ReadyAction() => isReady = true;

    public virtual void OnDeath() {

        // cancel all secondary action cooldowns if player dies
        CancelInvoke("ReadyAction");
        isReady = true;

    }

    public Meter CreateMeter(float cooldownDuration) => meterController.CreateMeter(cooldownDuration, weaponData.GetSecondaryIcon(), weaponData.GetWeaponColor());

    public abstract bool IsRegularAction(); // regular or hold action

}
