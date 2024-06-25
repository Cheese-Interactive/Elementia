using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowProjectile : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float movementMultiplier;
    [SerializeField] private float jumpMultiplier;
    [SerializeField] private float slowDuration;

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy) // make sure hit object is active
            collision.gameObject.GetComponent<SlowEffect>()?.Slow(movementMultiplier, jumpMultiplier, slowDuration);

    }
}
