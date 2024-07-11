using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowEffect : BaseEffect {

    [Header("References")]
    private CharacterHorizontalMovement charMovement;
    private CharacterJump charJump;

    [Header("Slow")]
    private float initialSpeed;
    private float initialJump;
    private Coroutine slowResetCoroutine;

    [Header("Overlay")]
    [SerializeField] private Overlay freezeOverlay;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        charJump = GetComponent<CharacterJump>();

        initialSpeed = charMovement.WalkSpeed;

        if (charJump) // only set if character can jump
            initialJump = charJump.JumpHeight;

        freezeOverlay.HideOverlay(); // hide slow overlay by default

    }

    public void Slow(float movementMultiplier, float jumpMultiplier, float duration) {

        if (slowResetCoroutine != null) { // coroutine is already running -> restart

            StopCoroutine(slowResetCoroutine);

            if (initialSpeed * movementMultiplier < charMovement.MovementSpeed) { // only set if new speed is slower than current speed

                charMovement.MovementSpeed *= movementMultiplier; // set new speed
                freezeOverlay.ShowOverlay(); // show slow overlay

            }

            if (charJump) { // only set if character can jump

                if (initialJump * jumpMultiplier < charJump.JumpHeight) { // only set if new jump height is lower than current jump height

                    charJump.JumpHeight *= jumpMultiplier; // set new jump height
                    freezeOverlay.ShowOverlay(); // show slow overlay

                }
            }
        } else { // coroutine is not running -> start

            charMovement.MovementSpeed *= movementMultiplier; // set new speed
            freezeOverlay.ShowOverlay(); // show slow overlay

            if (charJump) { // only set if character can jump

                charJump.JumpHeight *= jumpMultiplier; // set new jump height
                freezeOverlay.ShowOverlay(); // show slow overlay

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
        slowResetCoroutine = null;

        charMovement.MovementSpeed = initialSpeed;

        if (charJump)
            charJump.JumpHeight = initialJump;

        freezeOverlay.HideOverlay(); // hide slow overlay

    }
}
