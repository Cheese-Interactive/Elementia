using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice : Element {

    [Header("Primary Action")]
    [SerializeField] private SlowProjectile projectilePrefab;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileLifetime;

    [Header("Secondary Action")]
    [SerializeField] private float iceBlastRadius;
    [SerializeField] private LayerMask waterMask;

    public override void PrimaryAction() {

        if (!isPrimaryActionReady && !isPrimaryToggle) return; // make sure action is not a toggle because it won't toggle off if you don't

        SlowProjectile projectile = Instantiate(projectilePrefab, player.GetCastPoint().position, Quaternion.identity);
        projectile.Initialize(player.GetSpellCollider().GetComponent<Collider2D>(), projectileDamage, projectileSpeed, projectileLifetime, player.IsFacingRight());

        isPrimaryActionReady = false;
        Invoke("ReadyPrimaryAction", primaryCooldown);

    }

    public override void SecondaryAction() {

        if (!isSecondaryActionReady && !isSecondaryToggle) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.GetCastPoint().position, iceBlastRadius, waterMask);

        foreach (Collider2D collider in colliders) {

            // TODO: turn water into ice

        }

        isSecondaryActionReady = false;
        Invoke("ReadySecondaryAction", secondaryCooldown);

    }
}
