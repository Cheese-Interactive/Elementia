using DG.Tweening;
using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class TimeSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private LineRenderer channelBeacon;
    private GameManager gameManager;
    private LevelManager levelManager;
    private Animator anim;
    private Weapon prevWeapon;
    private bool isChanneling;

    [Header("Settings")]
    [SerializeField] private float channelDuration;
    [SerializeField][Range(0f, 100f)] private float channelFadeInDurationPercentage;
    [SerializeField][Range(0f, 100f)] private float channelFadeOutDurationPercentage;
    [SerializeField] private float channelBeaconStartWidth;
    [SerializeField] private float channelBeaconEndWidth;
    [SerializeField] private float channelBeaconHeight;
    private Coroutine channelCoroutine;
    private Tweener channelBeaconTweener;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();
        levelManager = FindObjectOfType<LevelManager>();
        anim = GetComponent<Animator>();

        channelBeacon.gameObject.SetActive(false); // hide channel beacon by default
        channelBeacon.startWidth = channelBeaconStartWidth;
        channelBeacon.endWidth = channelBeaconEndWidth;

    }

    public override void OnTriggerHold(bool startHold) {

        if (cooldownTimer > 0f || isChanneling == startHold) return; // make sure action is ready and is not already in the desired state

        if (!canUseInAir && !playerController.IsGrounded()) { // make sure player is grounded if required

            if (isChanneling) // if player is channeling, cancel it
                StopChanneling();

            return;

        }

        if (startHold) // mouse button pressed
            StartChanneling();
        else // mouse button released
            StopChanneling();

    }

    private void StartChanneling() {

        isChanneling = true;

        prevWeapon = playerController.GetCurrentWeapon(); // store previous weapon
        charWeaponHandler.ChangeWeapon(null, null); // unequip weapon
        playerController.SetCharacterEnabled(false); // disable player character (to prevent corgi built in animations from running)
        playerController.DisableCoreScripts(); // disable player core scripts

        // show channel beacon
        channelBeacon.gameObject.SetActive(true);
        Vector3 playerBottomPos = playerController.GetBottomPosition(); // get player bottom position
        channelBeacon.SetPositions(new Vector3[] { playerBottomPos, playerBottomPos + (transform.up * channelBeaconHeight) });

        // fade channel beacon in
        float channelFadeDuration = channelDuration * (channelFadeInDurationPercentage / 100f);
        Color color = channelBeacon.material.GetColor("_Color");
        channelBeaconTweener = DOVirtual.Float(0f, 1f, channelFadeDuration, (float alpha) => channelBeacon.material.SetColor("_Color", new Color(color.r, color.g, color.b, alpha))).SetEase(Ease.InExpo);

        anim.SetBool("isChanneling", true); // start animation

        channelCoroutine = StartCoroutine(HandleChannel());

    }

    private void StopChanneling() {

        if (channelCoroutine != null) StopCoroutine(channelCoroutine); // stop channel coroutine if it is running
        channelCoroutine = null;

        if (channelBeaconTweener != null && channelBeaconTweener.IsActive()) channelBeaconTweener.Kill(); // stop channel beacon tweener if it is running

        anim.SetBool("isChanneling", false); // stop animation

        // fade channel beacon out
        float channelFadeDuration = channelDuration * (channelFadeOutDurationPercentage / 100f);
        Color color = channelBeacon.material.GetColor("_Color");
        channelBeaconTweener = DOVirtual.Float(1f, 0f, channelFadeDuration, (float alpha) => channelBeacon.material.SetColor("_Color", new Color(color.r, color.g, color.b, alpha))).SetEase(Ease.OutCubic).OnComplete(() => channelBeacon.gameObject.SetActive(false)); // fade out and hide channel beacon after fade out

        playerController.EnableCoreScripts(); // enable player core scripts
        playerController.SetCharacterEnabled(true); // enable player character (to allow corgi built in animations to run)
        charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID); // re-equip previous weapon

        isChanneling = false;

        cooldownTimer = cooldown; // restart cooldown timer
        weaponSelector.SetSecondaryCooldownValue(GetNormalizedCooldown(), cooldownTimer); // update secondary cooldown meter

    }

    private IEnumerator HandleChannel() {

        yield return new WaitForSeconds(channelDuration); // wait for channel duration

        levelManager.CurrentCheckPoint.SpawnPlayer(playerController.GetComponent<Character>()); // teleport player to checkpoint
        gameManager.ResetAllResettables(); // reset all resettables
        StopChanneling(); // stop channeling

        channelCoroutine = null;

    }

    public override bool IsRegularAction() => false;

    public override bool IsUsing() => isChanneling;

}
