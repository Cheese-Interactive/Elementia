using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class BurnEffect : BaseEffect {

    [Header("References")]
    private Health health;

    private void Start() {

        health = GetComponent<Health>();

        if (!health) Debug.LogError("Health component not found on " + gameObject.name + " object.");

        overlay.HideOverlay(); // hide burn overlay by default

    }

    public void AddEffect(GameObject instigator, float damage, int ticks, float duration, float invincibilityDuration, Vector3 damageDirection, bool instantTick) {

        overlay.ShowOverlay(); // show burn overlay

        if (resetEffectCoroutine != null) StopCoroutine(resetEffectCoroutine); // stop previous effect coroutine if it exists
        resetEffectCoroutine = StartCoroutine(HandleBurn(instigator, damage, ticks, duration, invincibilityDuration, damageDirection, instantTick)); // start effect coroutine

    }

    private IEnumerator HandleBurn(GameObject instigator, float damage, int ticks, float duration, float invincibilityDuration, Vector3 damageDirection, bool instantTick) {

        for (int i = 0; i < ticks; i++) {

            if (!instantTick) // if instant tick is disabled, wait before first tick
                yield return new WaitForSeconds(duration / ticks); // wait for next tick (do this before applying damage to object)

            health.Damage(damage, instigator, invincibilityDuration, invincibilityDuration, damageDirection); // apply damage to object

            if (instantTick) // if instant tick is enabled, wait after first tick
                yield return new WaitForSeconds(duration / ticks); // wait for next tick (do this after applying damage to object)

        }

        RemoveEffect(); // remove effect after all ticks are done

    }

    public override void RemoveEffect() {

        if (resetEffectCoroutine != null) StopCoroutine(resetEffectCoroutine); // stop effect coroutine if it exists
        resetEffectCoroutine = null; // reset effect coroutine

        overlay.HideOverlay(); // hide burn overlay

    }
}
