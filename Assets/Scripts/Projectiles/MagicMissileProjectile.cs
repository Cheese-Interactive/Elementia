using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileProjectile : BaseProjectile {

    [Header("Direction")]
    private Vector2 lastPos;

    [Header("Settings")]
    [SerializeField] private Vector2 objectImpactForce;

    private void Start() => lastPos = transform.position;

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy) { // make sure hit object is active

            /* FORCE DEPENDS ON PROJECTILE DIRECTION/POSITION */
            Vector2 force = ((Vector2) transform.position - lastPos).normalized;
            force.x *= objectImpactForce.x; // increase horizontal pull force
            force.y *= objectImpactForce.y; // increase vertical pull force
            collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(force, ForceMode2D.Impulse); // push object away

        }
    }
}
