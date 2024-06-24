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
    private Coroutine slowResetCoroutine;

    [Header("Overlay")]
    [SerializeField] private GameObject overlay;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        charJump = GetComponent<CharacterJump>();

        initialSpeed = charMovement.MovementSpeed;

        if (charJump) // only set if character can jump
            initialJump = charJump.JumpHeight;

        overlay.gameObject.SetActive(false); // deactivate slow overlay by default

    }

    public void Slow(float multiplier, float duration) {

        if (slowResetCoroutine != null) {

            StopCoroutine(slowResetCoroutine);

            if (initialSpeed * multiplier < charMovement.MovementSpeed) { // only set if new speed is slower than current speed

                charMovement.MovementSpeed *= multiplier; // set new speed
                overlay.gameObject.SetActive(true); // activate slow overlay

                if (charJump) // only set if character can jump
                    charJump.JumpHeight *= multiplier; // set new jump height

            }
        } else {

            charMovement.MovementSpeed *= multiplier; // set new speed
            overlay.gameObject.SetActive(true); // activate slow overlay

            if (charJump) // only set if character can jump
                charJump.JumpHeight *= multiplier; // set new jump height

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
