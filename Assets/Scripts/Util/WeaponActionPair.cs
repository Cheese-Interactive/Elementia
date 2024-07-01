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
    [SerializeField] private SpellData weaponData;
    [SerializeField] private PrimaryAction primaryAction;
    [SerializeField] private SecondaryAction secondaryAction;

    public Weapon GetWeapon() => weapon;

    public SpellData GetWeaponData() => weaponData;

    public PrimaryAction GetPrimaryAction() => primaryAction;

    public SecondaryAction GetSecondaryAction() => secondaryAction;

}
