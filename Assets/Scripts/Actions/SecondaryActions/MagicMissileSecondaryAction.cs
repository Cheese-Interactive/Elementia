using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class MagicMissileSecondaryAction : SecondaryAction {

    [Header("References")]
    private GameManager gameManager;
    private Animator anim;
    private Weapon prevWeapon;
    private bool isResetting;

    [Header("Settings")]
    [SerializeField] private float resetDuration;
    private Coroutine resetCoroutine;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();

    }

    public override void OnTriggerHold(bool startHold) {

        if (!isReady || isResetting == startHold) return; // make sure action is ready and is not already in the desired state

        if (!canUseInAir && !playerController.IsGrounded()) { // make sure player is grounded if required

            if (isResetting) // if player is resetting, cancel it
                StopResetting();

            return;

        }

        if (startHold) // mouse button pressed
            StartResetting();
        else // mouse button released
            StopResetting();

    }

    private void StartResetting() {

        isResetting = true;

        prevWeapon = playerController.GetCurrentWeapon(); // store previous weapon
        charWeaponHandler.ChangeWeapon(null, null); // unequip weapon
        playerController.SetCharacterEnabled(false); // disable player character (to prevent corgi built in animations from running)
        playerController.DisableCoreScripts(); // disable player core scripts

        anim.SetBool("isResetting", true); // start animation

        resetCoroutine = StartCoroutine(HandleReset());

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(resetDuration); // create new meter for reset duration

    }

    private void StopResetting() {

        if (resetCoroutine != null) StopCoroutine(resetCoroutine); // stop reset coroutine if it is running
        resetCoroutine = null;

        anim.SetBool("isResetting", false); // stop animation

        playerController.EnableCoreScripts(); // enable player core scripts
        playerController.SetCharacterEnabled(true); // enable player character (to allow corgi built in animations to run)
        charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID); // re-equip previous weapon

        isResetting = false;

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", cooldown);

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(cooldown); // create new meter for cooldown

    }

    private IEnumerator HandleReset() {

        yield return new WaitForSeconds(resetDuration); // wait for reset duration

        gameManager.ResetAllResettables(); // reset all resettables
        StopResetting(); // stop resetting

        resetCoroutine = null;

    }

    public override bool IsRegularAction() => false;

    public override bool IsUsing() => isResetting;

}
