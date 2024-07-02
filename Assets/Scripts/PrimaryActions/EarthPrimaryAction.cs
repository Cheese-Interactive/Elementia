using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthPrimaryAction : PrimaryAction {

    [Header("References")]
    [SerializeField] private GameObject[] rockPrefabs;
    private CharacterHandleWeapon charWeaponHandler;

    [Header("Summon")]
    [SerializeField] private float maxThrowDuration;
    private bool isRockSummoned;
    private bool isRockThrowReady;

    private new void Start() {

        base.Start();

        charWeaponHandler = GetComponent<CharacterHandleWeapon>();
        charWeaponHandler.AbilityPermitted = false; // disable weapon until rock is summoned and ready to be thrown

    }

    private void OnDisable() => charWeaponHandler.AbilityPermitted = true; // disables when weapon is switched, re-enable weapon handler

    public override void OnTriggerRegular() {

        if (!isReady) return; // make sure player is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        if (isRockThrowReady) { // weapon handles rock throw

            playerController.OnRockThrow();
            isRockSummoned = false;
            isRockThrowReady = false;

            // begin cooldown (placed here to start cooldown if rock is thrown)
            isReady = false;
            Invoke("ReadyAction", primaryCooldown);

            return;

        }

        if (!isRockSummoned) {

            isRockSummoned = playerController.SummonRock(this, rockPrefabs[Random.Range(0, rockPrefabs.Length)], maxThrowDuration);

        } else {

            playerController.DropRock();
            isRockSummoned = false;

            // begin cooldown (placed here to start cooldown if rock is dropped)
            isReady = false;
            Invoke("ReadyAction", primaryCooldown);

        }
    }

    public void ActivateWeapon() => isRockThrowReady = true;

    public override bool IsRegularAction() => true;

}
