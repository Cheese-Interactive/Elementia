using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowEffect : MonoBehaviour {

    [Header("References")]
    private CharacterHorizontalMovement charMovement;

    [Header("Slow")]
    private float initialMultiplier;
    private Coroutine slowResetCoroutine;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();

    }

    public void Slow(float multiplier, float duration) {

        if (slowResetCoroutine != null) StopCoroutine(slowResetCoroutine);
        else initialMultiplier = charMovement.MovementSpeedMultiplier; // only set if coroutine wasn't already running (to prevent resetting to slow speed)

        charMovement.MovementSpeedMultiplier = multiplier;
        slowResetCoroutine = StartCoroutine(ResetSlow(duration));

    }

    private IEnumerator ResetSlow(float duration) {

        yield return new WaitForSeconds(duration);
        RemoveEffect();

    }

    public void RemoveEffect() {

        if (slowResetCoroutine != null) StopCoroutine(slowResetCoroutine);

        charMovement.MovementSpeedMultiplier = initialMultiplier;

    }
}
