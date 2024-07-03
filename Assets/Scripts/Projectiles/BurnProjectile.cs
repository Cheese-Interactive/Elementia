using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnProjectile : BaseProjectile {

    // IMPORTANT: actual burn effect is implemented with Corgi Engine's DamageOnTouch script
    [Header("References")]
    private DamageOnTouch damageOnTouch;

    [Header("Settings")]
    [SerializeField] private float burnDamage;
    [SerializeField] private int burnTicks;
    [SerializeField] private float burnDuration;

    [Header("Direction")]
    private Vector2 lastPos;

    void Start() {

        damageOnTouch = GetComponent<DamageOnTouch>();

        lastPos = transform.position;

    }

    void Update() => lastPos = transform.position;

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0) // make sure hit object is active & is in target layer
            collision.gameObject.GetComponent<BurnEffect>()?.Burn(gameObject, burnDamage, burnTicks, burnDuration, damageOnTouch.DamageTakenInvincibilityDuration, (Vector2) transform.position - lastPos, false); // apply burn effect to object

    }
}
