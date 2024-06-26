using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnProjectile : MonoBehaviour {

    [Header("References")]
    private DamageOnTouch damageOnTouch;

    private void Start() => damageOnTouch = GetComponent<DamageOnTouch>();

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy) // make sure hit object is active
            collision.gameObject.GetComponent<BurnEffect>()?.Burn((damageOnTouch.RepeatDamageOverTime ? damageOnTouch.AmountOfRepeats - 1 : 1) * damageOnTouch.DurationBetweenRepeats); // burn duration is amount of repeats * duration between repeats | if repeat damage over time is false, only burn once

    }
}
