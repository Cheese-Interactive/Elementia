using MoreMountains.CorgiEngine;
using UnityEngine;

public class TimeProjectile : BaseProjectile {

    [Header("Settings")]
    [SerializeField] private float timeDuration;

    private new void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        base.OnTriggerEnter2D(collision); // call base method

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0) // make sure hit object is active & is in target layer
            collision.gameObject.GetComponent<TimeEffect>()?.FreezeTime(timeDuration); // apply freeze time effect to object

    }
}
