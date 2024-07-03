using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileProjectile : BaseProjectile {

    [Header("References")]
    private DamageOnTouch damageOnTouch;

    [Header("Direction")]
    private Vector2 lastPos;

    [Header("Settings")]
    [SerializeField] private Vector2 objectImpactForce;

    private void Start() {

        damageOnTouch = GetComponent<DamageOnTouch>();

        lastPos = transform.position;

    }

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0) { // make sure hit object is active & is in target layer

            /* FORCE DEPENDS ON PROJECTILE VELOCITY DIRECTION */
            Vector2 force = ((Vector2) transform.position - lastPos).normalized;
            force.x *= objectImpactForce.x; // increase horizontal pull force
            force.y *= objectImpactForce.y; // increase vertical pull force
            collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(force, ForceMode2D.Impulse); // push object away

        }
    }
}
