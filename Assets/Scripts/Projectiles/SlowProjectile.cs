using MoreMountains.CorgiEngine;
using UnityEngine;

public class SlowProjectile : BaseProjectile {

    [Header("Settings")]
    [SerializeField] private float movementMultiplier;
    [SerializeField] private float jumpMultiplier;
    [SerializeField] private float slowDuration;

    private new void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        base.OnTriggerEnter2D(collision); // call base method

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0) // make sure hit object is active & is in target layer
            collision.gameObject.GetComponent<SlowEffect>()?.Slow(movementMultiplier, jumpMultiplier, slowDuration);

    }
}
