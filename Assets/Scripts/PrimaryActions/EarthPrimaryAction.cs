using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthPrimaryAction : PrimaryAction {

    [Header("References")]
    [SerializeField] private Rock[] rockPrefabs;
    private CharacterHandleWeapon charWeapon;

    [Header("Summon")]
    private bool isRockSummoned;
    private bool isRockThrowReady;

    [Header("Throw")]
    [SerializeField] private float throwSpeed;

    private new void Start() {

        charWeapon = GetComponent<CharacterHandleWeapon>();
        charWeapon.AbilityPermitted = false; // disable weapon until rock is summoned and ready to be thrown

    }

    public override void OnTriggerRegular() {

        if (isRockThrowReady) return; // weapon handles rock throw

        if (!isRockSummoned) {

            isRockSummoned = playerController.SummonRock(this, rockPrefabs[Random.Range(0, rockPrefabs.Length)]);

        } else {

            playerController.DropRock();
            isRockSummoned = false;

        }
    }

    public void ActivateWeapon() {

        isRockThrowReady = true;
        charWeapon.AbilityPermitted = true; // enable weapon

    }

    public override bool IsRegularAction() => true;

}
