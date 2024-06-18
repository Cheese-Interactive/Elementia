using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissile : Element {

    [Header("Primary Action")]
    [SerializeField] private DamageProjectile projectilePrefab;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileDuration;
    private bool isPrimaryActionReady;
    private bool isSecondaryActionReady;

    [Header("Secondary Action")]
    [SerializeField] private float maxBarrierDuration;
    private bool isBarrierDeployed;

    private void Start() {

        isPrimaryActionReady = true;
        isSecondaryActionReady = true;

    }

    public override void PrimaryAction() {

        if (!isPrimaryActionReady && !isPrimaryToggle) return; // make sure action is not a toggle because it won't toggle off if you don't

        DamageProjectile projectile = Instantiate(projectilePrefab, player.GetCastPoint().position, Quaternion.identity);
        projectile.Initialize(player.GetSpellCollider().GetComponent<Collider2D>(), projectileDamage, projectileSpeed, projectileDuration, player.IsFacingRight());

        isPrimaryActionReady = false;
        Invoke("ReadyPrimaryAction", primaryCooldown);

    }

    public override void SecondaryAction() {

        if (!isSecondaryActionReady && !isSecondaryToggle) return;

        isBarrierDeployed = !isBarrierDeployed; // do this before returning to deal with toggle issues

        if (!player.IsGrounded()) return;

        if (isBarrierDeployed)
            player.DeployBarrier(maxBarrierDuration);
        else
            player.RetractBarrier();

        isSecondaryActionReady = false;
        Invoke("ReadySecondaryAction", secondaryCooldown);

    }

    private void ReadyPrimaryAction() => isPrimaryActionReady = true;

    private void ReadySecondaryAction() => isPrimaryActionReady = true;

}
