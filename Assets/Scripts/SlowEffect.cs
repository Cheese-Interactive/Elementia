using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowEffect : MonoBehaviour {

    [Header("References")]
    private CharacterHorizontalMovement charMovement;

    [Header("Slow")]
    private Coroutine slowResetCoroutine;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();

    }

    public void Slow(float multiplier, float duration) {

        float initialMultiplier = charMovement.MovementSpeedMultiplier;
        charMovement.MovementSpeedMultiplier = multiplier;

        if (slowResetCoroutine != null) StopCoroutine(slowResetCoroutine);

        slowResetCoroutine = StartCoroutine(ResetSlow(initialMultiplier, duration));

    }

    private IEnumerator ResetSlow(float initialMultiplier, float duration) {

        yield return new WaitForSeconds(duration);
        charMovement.MovementSpeedMultiplier = initialMultiplier;

        slowResetCoroutine = null;

    }
}
