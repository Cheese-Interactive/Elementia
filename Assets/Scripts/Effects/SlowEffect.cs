using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class SlowEffect : BaseEffect {

    [Header("References")]
    [SerializeField] private Overlay slowOverlay;
    private CharacterHorizontalMovement charMovement;
    private CharacterJump charJump;

    [Header("Slow")]
    [SerializeField] private bool slowSpeed;
    [SerializeField] private bool slowJump;
    private Coroutine resetEffectCoroutine;
    private float prevSpeed;
    private float prevJump;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        charJump = GetComponent<CharacterJump>();
        slowOverlay.HideOverlay(); // hide slow overlay by default

        if (slowSpeed && charMovement)
            prevSpeed = charMovement.WalkSpeed; // store initial speed

        if (slowJump && charJump)
            prevJump = charJump.JumpHeight; // store initial jump height

    }

    public void AddEffect(float movementMultiplier, float jumpMultiplier, float duration) {

        if (resetEffectCoroutine != null) { // coroutine is already running -> restart

            StopCoroutine(resetEffectCoroutine); // stop previous effect coroutine if it exists

            if (slowSpeed && charMovement) { // only set if character can move

                if (prevSpeed * movementMultiplier < charMovement.MovementSpeed) { // only set if new speed is slower than current speed

                    charMovement.MovementSpeed *= movementMultiplier; // set new speed
                    slowOverlay.ShowOverlay(); // show slow overlay

                }
            }

            if (slowJump && charJump) { // only set if character can jump

                if (prevJump * jumpMultiplier < charJump.JumpHeight) { // only set if new jump height is lower than current jump height

                    charJump.JumpHeight *= jumpMultiplier; // set new jump height
                    slowOverlay.ShowOverlay(); // show slow overlay

                }
            }
        } else { // coroutine is not running -> start

            if (slowSpeed && charMovement) { // only set if character can move

                prevSpeed = charMovement.WalkSpeed; // store current speed each time before slow effect
                charMovement.MovementSpeed *= movementMultiplier; // set new speed
                slowOverlay.ShowOverlay(); // show slow overlay

            }

            if (slowJump && charJump) { // only set if character can jump

                prevJump = charJump.JumpHeight; // store current jump height each time before slow effect
                charJump.JumpHeight *= jumpMultiplier; // set new jump height
                slowOverlay.ShowOverlay(); // show slow overlay

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

        if (slowSpeed && charMovement)
            charMovement.MovementSpeed = prevSpeed;

        if (slowJump && charJump)
            charJump.JumpHeight = prevJump;

        slowOverlay.HideOverlay(); // hide slow overlay

    }
}
