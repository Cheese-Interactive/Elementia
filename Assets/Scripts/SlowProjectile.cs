using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowProjectile : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private float movementMultiplier;
    [SerializeField] private float slowDuration;

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy")) // if the projectile collides with the enemy
            collision.gameObject.GetComponent<SlowEffect>().Slow(movementMultiplier, slowDuration);

    }
}
