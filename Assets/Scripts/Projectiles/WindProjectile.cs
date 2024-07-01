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
    [SerializeField] private float objectWindForce;

    void Start() {

        damageOnTouch = GetComponent<DamageOnTouch>();

        damageOnTouch.DamageCausedKnockbackForce = entityWindForce;
        lastPos = transform.position;

    }

    void Update() => lastPos = transform.position;

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy) // make sure hit object is active
            collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(((Vector2) transform.position - lastPos).normalized * objectWindForce, ForceMode2D.Impulse); // push object away

    }
}
