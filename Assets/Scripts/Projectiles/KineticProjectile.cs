using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticProjectile : BaseProjectile {

    [Header("References")]
    private Projectile projectile;
    private DamageOnTouch damageOnTouch;

    [Header("Settings")]
    [SerializeField] private Vector2 entityKineticForce;
    [SerializeField] private float totalPullMultiplier;
    [SerializeField] private float verticalPullMultiplier;

    private void Start() {

        projectile = GetComponent<Projectile>();
        damageOnTouch = GetComponent<DamageOnTouch>();

        damageOnTouch.DamageCausedKnockbackForce = entityKineticForce;

    }

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy) // make sure hit object is active
            collision.gameObject.GetComponent<Pullable>()?.Pull(projectile.GetOwner().transform, totalPullMultiplier, verticalPullMultiplier); // pull object towards player

    }
}
