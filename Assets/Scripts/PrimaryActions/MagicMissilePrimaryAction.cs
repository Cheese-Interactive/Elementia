using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissilePrimaryAction : PrimaryAction {

    [Header("References")]
    private CharacterHandleWeapon charWeaponHandler;
    private Weapon weapon;

    [Header("Settings")]
    [SerializeField] private float groundedLaunchForce;
    [SerializeField] private float airLaunchForce;

    private void OnEnable() => StartCoroutine(SetRecoil()); // to set recoil after weapon is set

    private IEnumerator SetRecoil() {

        while (!GetComponent<CharacterHandleWeapon>().CurrentWeapon) yield return null; // wait until weapon is set

        charWeaponHandler = GetComponent<CharacterHandleWeapon>();
        weapon = charWeaponHandler.CurrentWeapon.GetComponent<Weapon>();

        weapon.ApplyRecoilOnUse = true; // activate recoil
        weapon.RecoilOnUseProperties.RecoilForceGrounded = groundedLaunchForce; // set grounded recoil force
        weapon.RecoilOnUseProperties.RecoilForceAirborne = airLaunchForce; // set air recoil force

    }

    public override bool IsRegularAction() => true;

}
