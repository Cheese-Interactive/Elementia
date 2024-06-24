using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowEffect : MonoBehaviour {

    [Header("References")]
    private CharacterHorizontalMovement charMovement;
    private CharacterJump charJump;

    [Header("Slow")]
    private float initialSpeed;
    private float initialJump;
    private float lastMultiplier;
    private Coroutine slowResetCoroutine;

    [Header("Overlay")]
    [SerializeField] private GameObject overlay;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        charJump = GetComponent<CharacterJump>();

        overlay.gameObject.SetActive(false); // deactivate slow overlay by default

    }

    public void Slow(float multiplier, float duration) {

        if (slowResetCoroutine != null) {

            StopCoroutine(slowResetCoroutine);

            if (multiplier < lastMultiplier) { // only set if new speed is slower than current speed

                initialSpeed = charMovement.MovementSpeed;
                charMovement.MovementSpeed *= multiplier;
                overlay.gameObject.SetActive(true); // activate slow overlay

                if (charJump) { // only set if character can jump

                    initialJump = charJump.JumpHeight;
                    charJump.JumpHeight *= multiplier;

                }
            }
        } else {

            initialSpeed = charMovement.MovementSpeed;
            charMovement.MovementSpeed *= multiplier; // only set if coroutine wasn't already running (to prevent resetting to slow speed)
            overlay.gameObject.SetActive(true); // activate slow overlay

            if (charJump) { // only set if character can jump

                initialJump = charJump.JumpHeight;
                charJump.JumpHeight *= multiplier; // only set if coroutine wasn't already running (to prevent resetting to slow jump)

            }
        }

        slowResetCoroutine = StartCoroutine(ResetSlow(duration));

    }

    private IEnumerator ResetSlow(float duration) {

        yield return new WaitForSeconds(duration);
        RemoveEffect();

    }

    public void RemoveEffect() {

        if (slowResetCoroutine != null) StopCoroutine(slowResetCoroutine);

        charMovement.MovementSpeed = initialSpeed;

        if (charJump)
            charJump.JumpHeight = initialJump;

        overlay.gameObject.SetActive(false); // deactivate slow overlay

    }
}
