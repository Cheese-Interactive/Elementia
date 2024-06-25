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
    [SerializeField] private Overlay overlay;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        charJump = GetComponent<CharacterJump>();

        initialSpeed = charMovement.MovementSpeed;

        if (charJump) // only set if character can jump
            initialJump = charJump.JumpHeight;

        overlay.gameObject.SetActive(false); // deactivate slow overlay by default

    }

    public void Slow(float movementMultiplier, float jumpMultiplier, float duration) {

        if (slowResetCoroutine != null) {

            StopCoroutine(slowResetCoroutine);

            if (initialSpeed * movementMultiplier < charMovement.MovementSpeed) { // only set if new speed is slower than current speed

                charMovement.MovementSpeed *= movementMultiplier; // set new speed
                overlay.gameObject.SetActive(true); // activate slow overlay

            }

            if (charJump) { // only set if character can jump

                if (initialJump * jumpMultiplier < charJump.JumpHeight) { // only set if new jump height is lower than current jump height

                    charJump.JumpHeight *= jumpMultiplier; // set new jump height
                    overlay.gameObject.SetActive(true); // activate slow overlay

                }
            }
        } else {

            charMovement.MovementSpeed *= movementMultiplier; // set new speed
            overlay.gameObject.SetActive(true); // activate slow overlay

            if (charJump) { // only set if character can jump

                charJump.JumpHeight *= jumpMultiplier; // set new jump height
                overlay.gameObject.SetActive(true); // activate slow overlay

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
