using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticProjectile : BaseProjectile {

    [Header("References")]
    private Projectile projectile;

    [Header("Settings")]
    [SerializeField] private Vector2 entityKineticForce;
    [SerializeField] private Vector2 objectKineticForce;

    private void Start() => projectile = GetComponent<Projectile>();

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy) { // make sure hit object is active

            /* FORCE DIRECTION DEPENDS ON SHOOTER POSITION */
            Vector2 entityForce = (projectile.GetOwner().transform.position - collision.transform.position).normalized; // get force direction (vector faces shooter)
            Vector2 objectForce = entityForce;

            // handle entity force
            entityForce.x *= entityKineticForce.x; // increase horizontal push force
            entityForce.y *= entityKineticForce.y; // increase vertical push force
            collision.gameObject.GetComponent<CorgiController>()?.AddForce(entityForce); // push entity away from shooter

            // handle object force
            objectForce.x *= objectKineticForce.x; // increase horizontal pull force
            objectForce.y *= objectKineticForce.y; // increase vertical pull force
            collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(objectForce, ForceMode2D.Impulse); // pull object towards shooter

        }
    }
}
