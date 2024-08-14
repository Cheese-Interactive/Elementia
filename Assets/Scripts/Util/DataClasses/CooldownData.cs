using UnityEngine;

public class CooldownData {

    [Header("Settings")]
    [SerializeField] private float cooldownTimer;
    [SerializeField] private float unequipTime;

    public CooldownData(float cooldownTimer, float unequipTime) {

        this.cooldownTimer = cooldownTimer;
        this.unequipTime = unequipTime;

    }

    public float GetCooldownTimer() => cooldownTimer;

    public float GetUnequipTime() => unequipTime;

}
