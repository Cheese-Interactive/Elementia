using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class SlowEffect : BaseEffect {

    [Header("References")]
    private CharacterHorizontalMovement charMovement;
    private CharacterJump charJump;

    [Header("Settings")]
    [SerializeField] private bool affectSpeed;
    [SerializeField] private bool affectJump;
    private float prevSpeed;
    private float prevJump;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        charJump = GetComponent<CharacterJump>();
        overlay.HideOverlay(); // hide slow overlay by default

        // store initial movement speed if character can move and effect affects movement speed
        if (affectSpeed && charMovement)
            prevSpeed = charMovement.WalkSpeed;

        // store initial jump height if character can jump and effect affects jump height
        if (affectJump && charJump)
            prevJump = charJump.JumpHeight;

    }

    public void AddEffect(float movementMultiplier, float jumpMultiplier, float duration) {

        if (resetEffectCoroutine != null) { // coroutine is already running -> restart

            StopCoroutine(resetEffectCoroutine); // stop previous effect coroutine if it exists

            if (affectSpeed && charMovement) { // only set if character can move

                if (prevSpeed * movementMultiplier < charMovement.MovementSpeed) { // only set if new movement speed is slower than current movement speed

                    charMovement.MovementSpeed *= movementMultiplier; // set new movement speed
                    overlay.ShowOverlay(); // show slow overlay

                }
            }

            if (affectJump && charJump) { // only set if character can jump

                if (prevJump * jumpMultiplier < charJump.JumpHeight) { // only set if new jump height is lower than current jump height

                    charJump.JumpHeight *= jumpMultiplier; // set new jump height
                    overlay.ShowOverlay(); // show slow overlay

                }
            }
        } else { // coroutine is not running -> start

            if (affectSpeed && charMovement) { // only set if character can move

                prevSpeed = charMovement.WalkSpeed; // store current movement speed each time before slow effect
                charMovement.MovementSpeed *= movementMultiplier; // set new movement speed
                overlay.ShowOverlay(); // show slow overlay

            }

            if (affectJump && charJump) { // only set if character can jump

                prevJump = charJump.JumpHeight; // store current jump height each time before slow effect
                charJump.JumpHeight *= jumpMultiplier; // set new jump height
                overlay.ShowOverlay(); // show slow overlay

            }
        }

        resetEffectCoroutine = StartCoroutine(ResetEffect(duration)); // start effect coroutine

    }

    private IEnumerator ResetEffect(float duration) {

        yield return new WaitForSeconds(duration);
        RemoveEffect(); // remove effect after duration

    }

    public override void RemoveEffect() {

        if (resetEffectCoroutine != null) StopCoroutine(resetEffectCoroutine); // stop effect coroutine if it exists
        resetEffectCoroutine = null; // reset coroutine

        // reset movement speed
        if (affectSpeed && charMovement)
            charMovement.MovementSpeed = prevSpeed;

        // reset jump height
        if (affectJump && charJump)
            charJump.JumpHeight = prevJump;

        overlay.HideOverlay(); // hide slow overlay

    }
}
