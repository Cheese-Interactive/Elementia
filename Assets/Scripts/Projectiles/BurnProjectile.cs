using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnProjectile : BaseProjectile {

    [Header("Settings")]
    [Space]
    [SerializeField] private float burnDamage;
    [SerializeField] private int burnTicks;
    [SerializeField] private float burnDuration;

    private new void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        base.OnTriggerEnter2D(collision); // call base method

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0) // make sure hit object is active & is in target layer
            collision.gameObject.GetComponent<BurnEffect>()?.Burn(gameObject, burnDamage, burnTicks, burnDuration, damageOnTouch.DamageTakenInvincibilityDuration, (Vector2) transform.position - startPos, false); // apply burn effect to object

    }
}
