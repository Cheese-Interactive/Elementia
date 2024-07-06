using DG.Tweening;
using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private LineRenderer channelBeacon;
    private LevelManager levelManager;
    private CharacterHandleWeapon charWeaponHandler;
    private Animator anim;
    private Weapon prevWeapon;
    private Meter currMeter;

    [Header("Settings")]
    [SerializeField] private float channelDuration;
    [SerializeField][Range(0f, 100f)] private float channelFadeInDurationPercentage;
    [SerializeField][Range(0f, 100f)] private float channelFadeOutDurationPercentage;
    [SerializeField] private float channelBeaconStartWidth;
    [SerializeField] private float channelBeaconEndWidth;
    [SerializeField] private float channelBeaconHeight;
    private bool isChanneling;
    private Coroutine channelCoroutine;
    private Tweener channelBeaconTweener;

    private new void Start() {

        base.Start();

        levelManager = FindObjectOfType<LevelManager>();
        charWeaponHandler = GetComponent<CharacterHandleWeapon>();
        anim = GetComponent<Animator>();

        channelBeacon.gameObject.SetActive(false); // hide channel beacon by default
        channelBeacon.startWidth = channelBeaconStartWidth;
        channelBeacon.endWidth = channelBeaconEndWidth;

    }

    public override void OnTriggerHold(bool startHold) {

        if (!isReady || isChanneling == startHold) return; // make sure action is ready and is not already in the desired state

        if (!canUseInAir && !playerController.IsGrounded()) { // make sure player is grounded if required

            if (isChanneling) // if flamethrower is equipped, unequip it
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
        playerController.DisableAllMechanics(); // disable player mechanics
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

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(channelDuration); // create new meter for channel duration

    }

    private void StopChanneling() {

        if (channelCoroutine != null) StopCoroutine(channelCoroutine); // stop channeling coroutine if it is running

        if (channelBeaconTweener != null && channelBeaconTweener.IsActive()) channelBeaconTweener.Kill(); // stop channel beacon tweener if it is running

        anim.SetBool("isChanneling", false); // stop animation

        // fade channel beacon out
        float channelFadeDuration = channelDuration * (channelFadeOutDurationPercentage / 100f);
        Color color = channelBeacon.material.GetColor("_Color");
        channelBeaconTweener = DOVirtual.Float(1f, 0f, channelFadeDuration, (float alpha) => channelBeacon.material.SetColor("_Color", new Color(color.r, color.g, color.b, alpha))).SetEase(Ease.OutCubic).OnComplete(() => channelBeacon.gameObject.SetActive(false)); // fade out and hide channel beacon after fade out

        playerController.EnableCoreScripts(); // enable player core scripts
        playerController.EnableAllMechanics(); // enable player mechanics
        playerController.SetCharacterEnabled(true); // enable player character (to allow corgi built in animations to run)
        charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID); // re-equip previous weapon

        isChanneling = false;

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(secondaryCooldown); // create new meter for cooldown

    }

    private IEnumerator HandleChannel() {

        yield return new WaitForSeconds(channelDuration); // wait for channel duration

        levelManager.CurrentCheckPoint.SpawnPlayer(playerController.GetComponent<Character>()); // teleport player to checkpoint
        StopChanneling(); // stop channeling

        channelCoroutine = null;

    }

    public override bool IsRegularAction() => false;

    public bool IsChanneling() => isChanneling;

}
