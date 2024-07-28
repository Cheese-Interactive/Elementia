using UnityEngine;

public class ElectricPrimaryAction : PrimaryAction {

    [Header("Settings")]
    [SerializeField] private float lightningDuration;
    [Space]
    [SerializeField] protected bool hasDistanceMultiplier;
    [Space]
    [SerializeField] protected bool isPushProjectile;
    [SerializeField] private Vector2 entityForceMultiplier;
    [SerializeField] private Vector2 objectForceMultiplier;

    private new void OnEnable() {

        base.OnEnable();

        if (lightningDuration > cooldown)
            Debug.LogWarning("Lightning duration is greater than cooldown, this may cause issues with the lightning visuals.");

        charWeaponHandler.CurrentWeapon.GetComponent<ElectricWeapon>().Initialize(lightningDuration, hasDistanceMultiplier, isPushProjectile, entityForceMultiplier, objectForceMultiplier); // initialize electric weapon

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
