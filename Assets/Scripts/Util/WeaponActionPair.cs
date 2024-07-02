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
    [SerializeField] private SpellData spellData;
    [SerializeField] private PrimaryAction primaryAction;
    [SerializeField] private SecondaryAction secondaryAction;
    [SerializeField] private float switchCooldown;

    public Weapon GetWeapon() => weapon;

    public SpellData GetSpellData() => spellData;

    public PrimaryAction GetPrimaryAction() => primaryAction;

    public SecondaryAction GetSecondaryAction() => secondaryAction;

    public float GetSwitchCooldown() => switchCooldown;

}
