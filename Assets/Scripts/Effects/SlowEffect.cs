using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class SlowEffect : BaseEffect {

    [Header("References")]
    private CharacterHorizontalMovement charMovement;
    private CharacterJump charJump;

    [Header("Slow")]
    [SerializeField] private bool slowSpeed;
    [SerializeField] private bool slowJump;
    private float startSpeed;
    private float startJump;
    private Coroutine slowResetCoroutine;

    [Header("Overlay")]
    [SerializeField] private Overlay freezeOverlay;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        charJump = GetComponent<CharacterJump>();

        if (charMovement) // only set if character can move
            startSpeed = charMovement.WalkSpeed;

        if (charJump) // only set if character can jump
            startJump = charJump.JumpHeight;

        freezeOverlay.HideOverlay(); // hide slow overlay by default

    }

    public void AddEffect(float movementMultiplier, float jumpMultiplier, float duration) {

        if (slowResetCoroutine != null) { // coroutine is already running -> restart

            StopCoroutine(slowResetCoroutine);

            if (slowSpeed && charMovement) { // only set if character can move

                if (startSpeed * movementMultiplier < charMovement.MovementSpeed) { // only set if new speed is slower than current speed

                    charMovement.MovementSpeed *= movementMultiplier; // set new speed
                    freezeOverlay.ShowOverlay(); // show slow overlay

                }
            }

            if (slowJump && charJump) { // only set if character can jump

                if (startJump * jumpMultiplier < charJump.JumpHeight) { // only set if new jump height is lower than current jump height

                    charJump.JumpHeight *= jumpMultiplier; // set new jump height
                    freezeOverlay.ShowOverlay(); // show slow overlay

                }
            }
        } else { // coroutine is not running -> start

            if (slowSpeed && charMovement) { // only set if character can move

                charMovement.MovementSpeed *= movementMultiplier; // set new speed
                freezeOverlay.ShowOverlay(); // show slow overlay

            }

            if (slowJump && charJump) { // only set if character can jump

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

    public override void RemoveEffect() {

        if (slowResetCoroutine != null) StopCoroutine(slowResetCoroutine);
        slowResetCoroutine = null;

        if (slowSpeed && charMovement)
            charMovement.MovementSpeed = startSpeed;

        if (slowJump && charJump)
            charJump.JumpHeight = startJump;

        freezeOverlay.HideOverlay(); // hide slow overlay

    }
}
