using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnProjectile : BaseProjectile {

    // IMPORTANT: actual burn effect is implemented with Corgi Engine's DamageOnTouch script
    [Header("References")]
    private DamageOnTouch damageOnTouch;

    [Header("Settings")]
    [SerializeField] private int burnTicks;
    [SerializeField] private float burnDuration;

    private void Start() {

        damageOnTouch = GetComponent<DamageOnTouch>();

        damageOnTouch.RepeatDamageOverTime = true; // enable repeat damage over time (for damage tick)
        damageOnTouch.AmountOfRepeats = burnTicks + 1; // set burn damage to ticks + 1 (1 extra tick for initial damage)
        damageOnTouch.DurationBetweenRepeats = burnDuration / burnTicks; // set duration between ticks to burn duration divided by ticks

    }

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy) // make sure hit object is active
            collision.gameObject.GetComponent<BurnEffect>()?.Burn(burnDuration);

    }
}
