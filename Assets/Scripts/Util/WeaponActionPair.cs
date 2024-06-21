using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponActionPair {

    [Header("Data")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private SecondaryAction secondaryAction;

    public Weapon GetWeapon() => weapon;

    public SecondaryAction GetSecondaryAction() => secondaryAction;

}
