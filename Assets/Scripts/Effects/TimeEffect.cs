using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class TimeEffect : BaseEffect {

    [Header("References")]
    private EntityController entityController;
    private CharacterHorizontalMovement charMovement;
    private Rigidbody2D rb;

    [Header("Time")]
    private float startSpeed;
    private bool isTimeFrozen;
    private Coroutine unfreezeTimeCoroutine;

    [Header("Overlay")]
    [SerializeField] private Overlay timeOverlay;

    private void Start() {

        entityController = GetComponent<EntityController>();
        charMovement = GetComponent<CharacterHorizontalMovement>();
        rb = GetComponent<Rigidbody2D>();

        if (charMovement) // only set if character can move
            startSpeed = charMovement.WalkSpeed;

        timeOverlay.HideOverlay(); // hide slow overlay by default

    }

    public void FreezeTime(float duration) {

        isTimeFrozen = true;
        timeOverlay.ShowOverlay(); // show time overlay

        if (unfreezeTimeCoroutine != null) StopCoroutine(unfreezeTimeCoroutine);

        if (rb) {

            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll; // freeze rigidbody

        }

        if (entityController) {

            if (charMovement)
                charMovement.MovementSpeed = 0f; // stop movement

            entityController.DisableCoreScripts(); // disable core scripts
            entityController.SetCharacterEnabled(false); // disable character
            entityController.SetInvulnerable(true); // set entity invulnerable

        }

        unfreezeTimeCoroutine = StartCoroutine(UnfreezeTime(duration));

    }

    private IEnumerator UnfreezeTime(float duration) {

        yield return new WaitForSeconds(duration);
        RemoveEffect();

    }

    public void RemoveEffect() {

        if (unfreezeTimeCoroutine != null) StopCoroutine(unfreezeTimeCoroutine); // stop time unfreeze coroutine
        unfreezeTimeCoroutine = null;

        if (rb)
            rb.constraints = RigidbodyConstraints2D.None; // unfreeze rigidbody

        if (entityController) {

            if (charMovement)
                charMovement.MovementSpeed = startSpeed; // reset movement speed

            entityController.SetInvulnerable(false); // set entity vulnerable
            entityController.SetCharacterEnabled(true); // enable character
            entityController.EnableCoreScripts(); // enable core scripts

        }

        timeOverlay.HideOverlay(); // hide slow overlay
        isTimeFrozen = false;

    }

    public bool IsTimeFrozen() => isTimeFrozen;

}
