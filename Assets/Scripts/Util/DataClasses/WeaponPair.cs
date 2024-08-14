using MoreMountains.CorgiEngine;
using System;
using UnityEngine;

[Serializable]
public class WeaponPair {

    [Header("Data")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private PrimaryAction primaryAction;
    [SerializeField] private SecondaryAction secondaryAction;

    public Weapon GetWeapon() => weapon;

    public WeaponData GetWeaponData() => weaponData;

    public PrimaryAction GetPrimaryAction() => primaryAction;

    public SecondaryAction GetSecondaryAction() => secondaryAction;

}
