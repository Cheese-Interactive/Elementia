using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDatabase : MonoBehaviour {

    [Header("Data")]
    [SerializeField] private List<WeaponPair> weapons;
    private Dictionary<WeaponData, WeaponPair> database;

    // called here to initialize the database when it is needed (awake runs too late)
    public void Initialize() {

        database = new Dictionary<WeaponData, WeaponPair>();

        foreach (WeaponPair weapon in weapons)
            database.Add(weapon.GetWeaponData(), weapon);

    }

    public WeaponPair GetWeaponPair(WeaponData weaponData) => database[weaponData];

}
