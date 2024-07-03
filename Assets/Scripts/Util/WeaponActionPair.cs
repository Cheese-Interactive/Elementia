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
    [SerializeField] private PrimaryAction primaryAction;
    [SerializeField] private SecondaryAction secondaryAction;
    [SerializeField] private float switchCooldown;

    public Weapon GetWeapon() => weapon;

    public WeaponData GetWeaponData() => weaponData;

    public PrimaryAction GetPrimaryAction() => primaryAction;

    public SecondaryAction GetSecondaryAction() => secondaryAction;

    public float GetSwitchCooldown() => switchCooldown;

}
