using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindPrimaryAction : PrimaryAction {

    [Header("References")]
    private CharacterHandleWeapon charWeaponHandler;
    private Meter currMeter;
    private bool hasStartRan;

    private void OnEnable() {

        charWeaponHandler = GetComponent<CharacterHandleWeapon>();
        StartCoroutine(WaitForStart());

    }

    private new void Start() {

        base.Start();
        hasStartRan = true; // done because start is called after onenable

    }

    private IEnumerator WaitForStart() {

        while (!hasStartRan) yield return null; // wait until start has ran

        charWeaponHandler.CurrentWeapon.OnShoot += OnShoot; // subscribe to shoot event

    }

    private void OnShoot() {

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(charWeaponHandler.CurrentWeapon.TimeBetweenUses); // create new meter for cooldown (use the weapon cooldown instead of primary action cooldown)

    }

    public override bool IsRegularAction() => true;

}
