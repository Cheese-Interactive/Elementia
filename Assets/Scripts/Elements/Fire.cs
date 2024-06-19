using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Element {

    [Header("Primary Action")]
    [SerializeField] private DamageProjectile projectilePrefab;
    [SerializeField] private float projectileDamage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float projectileLifetime;

    public override void PrimaryAction() {

        if (!isPrimaryActionReady && !isPrimaryToggle) return; // make sure action is not a toggle because it won't toggle off if you don't

        DamageProjectile projectile = Instantiate(projectilePrefab, player.GetCastPoint().position, Quaternion.identity);
        projectile.Initialize(player.GetSpellCollider().GetComponent<Collider2D>(), projectileDamage, projectileSpeed, projectileLifetime, player.IsFacingRight());

        isPrimaryActionReady = false;
        Invoke("ReadyPrimaryAction", primaryCooldown);

    }

    public override void SecondaryAction() {

        if (!isSecondaryActionReady && !isSecondaryToggle) return;


        isSecondaryActionReady = false;
        Invoke("ReadySecondaryAction", secondaryCooldown);

    }
}
