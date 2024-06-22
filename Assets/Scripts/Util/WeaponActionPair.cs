using MoreMountains.CorgiEngine;
using MoreMountains.InventoryEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponActionPair {

    [Header("Data")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private SecondaryAction secondaryAction;

    public Weapon GetWeapon() => weapon;

    public WeaponData GetWeaponData() => weaponData;

    public SecondaryAction GetSecondaryAction() => secondaryAction;

}
