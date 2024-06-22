using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Data")]
public class WeaponData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private Sprite icon;

    public Sprite GetIcon() => icon;

}
