using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WindProjectile : BaseProjectile {

    [Header("References")]
    private DamageOnTouch damageOnTouch;

    [Header("Direction")]
    private Vector2 lastPos;

    [Header("Settings")]
    [SerializeField] private Vector2 entityWindForce;
    [SerializeField] private Vector2 objectWindForce;

    void Start() {

        damageOnTouch = GetComponent<DamageOnTouch>();

        lastPos = transform.position;

    }

    void Update() => lastPos = transform.position;

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0) { // make sure hit object is active & is in target layer

            /* FORCE DEPENDS ON PROJECTILE VELOCITY DIRECTION */
            Vector2 entityForce = ((Vector2) transform.position - lastPos).normalized;
            Vector2 objectForce = entityForce;

            // handle entity force
            entityForce.x *= entityWindForce.x; // increase horizontal push force
            entityForce.y *= entityWindForce.y; // increase vertical push force
            collision.gameObject.GetComponent<CorgiController>()?.SetForce(entityForce); // push entity away from projectile

            // handle object force
            objectForce.x *= objectWindForce.x; // increase horizontal pull force
            objectForce.y *= objectWindForce.y; // increase vertical pull force
            collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(objectForce, ForceMode2D.Impulse); // push object away from projectile

        }
    }
}
