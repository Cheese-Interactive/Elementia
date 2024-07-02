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
    [SerializeField] private Vector2 objectKineticForce;

    private void Start() {

        projectile = GetComponent<Projectile>();
        damageOnTouch = GetComponent<DamageOnTouch>();

        Vector2 entityForce = entityKineticForce;
        entityForce.x *= -1f; // reverse horizontal force (to pull)
        damageOnTouch.DamageCausedKnockbackForce = entityForce;

    }

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy) { // make sure hit object is active

            /* FORCE DEPENDS ON SHOOTER POSITION */
            Vector2 force = (projectile.GetOwner().transform.position - collision.transform.position).normalized;
            force.x *= objectKineticForce.x; // increase horizontal pull force
            force.y *= objectKineticForce.y; // increase vertical pull force
            collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(force, ForceMode2D.Impulse); // pull object towards shooter

        }
    }
}
