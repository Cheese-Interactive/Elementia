using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : MonoBehaviour {

    [Header("References")]
    private Health health;

    // IMPORTANT: actual burn effect is implemented with Corgi Engine's DamageOnTouch script
    [Header("Overlay")]
    [SerializeField] private Overlay burnOverlay;
    private Coroutine burnCoroutine;

    private void Start() {

        health = GetComponent<Health>();

        if (!health) Debug.LogError("Health component not found on " + gameObject.name + " object.");

        burnOverlay.HideOverlay(); // hide burn overlay by default

    }

    public void Burn(GameObject instigator, float damage, int ticks, float duration, float invincibilityDuration, Vector3 damageDirection, bool instantTick) {

        burnOverlay.ShowOverlay(); // show burn overlay

        if (burnCoroutine != null) StopCoroutine(burnCoroutine); // stop previous burn coroutine if it exists

        burnCoroutine = StartCoroutine(HandleBurn(instigator, damage, ticks, duration, invincibilityDuration, damageDirection, instantTick)); // start burn coroutine

    }

    private IEnumerator HandleBurn(GameObject instigator, float damage, int ticks, float duration, float invincibilityDuration, Vector3 damageDirection, bool instantTick) {

        for (int i = 0; i < ticks; i++) {

            if (!instantTick) // if instant tick is disabled, wait before first tick
                yield return new WaitForSeconds(duration / ticks); // wait for next tick (do this before applying damage to object)

            health.Damage(damage, instigator, invincibilityDuration, invincibilityDuration, damageDirection); // apply damage to object

            if (instantTick) // if instant tick is enabled, wait after first tick
                yield return new WaitForSeconds(duration / ticks); // wait for next tick (do this after applying damage to object)

        }

        burnOverlay.HideOverlay(); // hide burn overlay after burn effect is over

    }
}
