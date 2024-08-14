using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class MagicMissileSecondaryAction : SecondaryAction {

    [Header("References")]
    private MagicMissilePrimaryAction primaryAction;
    private Animator anim;
    private Weapon prevWeapon;
    private bool isResetting;

    [Header("Settings")]
    [SerializeField] private float resetDuration;
    [SerializeField] private float resetWaitDuration;
    private Coroutine resetCoroutine;

    [Header("Feedback")]
    [SerializeField] private MMF_Player onUseFeedback;

    private void Start() {

        primaryAction = GetComponent<MagicMissilePrimaryAction>();
        anim = GetComponent<Animator>();

    }

    public override void OnTriggerHold(bool startHold) {

        if (cooldownTimer > 0f || isResetting == startHold) return; // make sure action is ready and is not already in the desired state

        if (!canUseInAir && !playerController.IsGrounded()) { // make sure player is grounded if required

            if (isResetting) // if player is resetting, cancel it
                StopReset(false);

            return;

        }

        if (startHold) // mouse button pressed
            StartReset();
        else // mouse button released (cancel action)
            StopReset(false);

    }

    private void StartReset() {

        if (isResetting) return; // make sure player is not already resetting

        isResetting = true;

        prevWeapon = playerController.GetCurrentWeapon(); // store previous weapon

        primaryAction.enabled = false; // disable primary action
        charWeaponHandler.ChangeWeapon(null, null); // unequip weapon
        playerController.SetCharacterEnabled(false); // disable player character (to prevent corgi built in animations from running)
        playerController.DisableCoreScripts(); // disable player core scripts

        anim.SetBool("isResetting", true); // start animation

        onUseFeedback.PlayFeedbacks(); // play use sound
        resetCoroutine = StartCoroutine(HandleReset());

    }

    private IEnumerator HandleReset() {

        gameManager.ResetAllResettables(resetDuration); // start reset all resettables
        yield return new WaitForSeconds(resetDuration); // wait for reset duration
        StopReset(true); // stop resetting as it has completed

        resetCoroutine = null;

    }

    private void StopReset(bool resetCompleted) {

        if (!isResetting) return; // make sure player is resetting

        if (resetCoroutine != null) StopCoroutine(resetCoroutine); // stop reset coroutine if it is running
        resetCoroutine = null;

        anim.SetBool("isResetting", false); // stop animation

        playerController.EnableCoreScripts(); // enable player core scripts
        playerController.SetCharacterEnabled(true); // enable player character (to allow corgi built in animations to run)
        charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID); // re-equip previous weapon
        primaryAction.enabled = true; // enable primary action

        if (!resetCompleted)
            gameManager.CancelResets(); // cancel resets

        isResetting = false;
        StartCooldown(); // start cooldown

    }

    public override void OnDeath() {

        cooldownTimer = 0f; // reset cooldown timer
        StopReset(false); // stop reset if resetting (without completion)

    }

    public override bool IsRegularAction() => false;

    public override bool IsUsing() => isResetting;

}
