using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrimaryAction : MonoBehaviour {

    [Header("References")]
    protected PlayerController playerController;

    [Header("Settings")]
    [SerializeField] private bool isPrimaryAuto;
    [SerializeField] protected float primaryCooldown;
    [SerializeField] protected bool canUseInAir;

    [Header("Actions")]
    protected bool isReady;

    protected void Awake() => playerController = GetComponent<PlayerController>();

    protected void Start() => isReady = true;

    public virtual void OnTriggerRegular() { }

    public virtual void OnTriggerHold(bool startHold) { }

    public bool IsAutoAction() => isPrimaryAuto;

    public float GetCooldown() => primaryCooldown;

    protected void ReadyAction() => isReady = true;

    public virtual void OnDeath() {

        // cancel all secondary action cooldowns if player dies
        CancelInvoke("ReadyAction");
        isReady = true;

    }

    public abstract bool IsRegularAction(); // regular or hold action

}
