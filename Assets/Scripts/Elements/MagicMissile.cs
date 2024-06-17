using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissile : Element {

    [Header("Primary Action")]
    [SerializeField] private DamageProjectile projectilePrefab;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileDuration;

    [Header("Secondary Action")]
    [SerializeField] private float maxBarrierDuration;
    private bool isBarrierDeployed;

    public override void PrimaryAction() {

        DamageProjectile projectile = Instantiate(projectilePrefab, player.GetCastPoint().position, Quaternion.identity);
        projectile.Initialize(projectileDamage, projectileSpeed, projectileDuration, player.IsFacingRight());

    }

    public override void SecondaryAction() {

        isBarrierDeployed = !isBarrierDeployed; // do this before returning to deal with toggle issues

        if (!player.IsGrounded()) return;

        if (isBarrierDeployed)
            player.DeployBarrier(maxBarrierDuration);
        else
            player.RetractBarrier();

    }
}
