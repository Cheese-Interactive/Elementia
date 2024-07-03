using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowProjectile : BaseProjectile {

    [Header("References")]
    private DamageOnTouch damageOnTouch;

    [Header("Settings")]
    [SerializeField] private float movementMultiplier;
    [SerializeField] private float jumpMultiplier;
    [SerializeField] private float slowDuration;

    private void Start() => damageOnTouch = GetComponent<DamageOnTouch>();

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0) // make sure hit object is active & is in target layer
            collision.gameObject.GetComponent<SlowEffect>()?.Slow(movementMultiplier, jumpMultiplier, slowDuration);

    }
}
