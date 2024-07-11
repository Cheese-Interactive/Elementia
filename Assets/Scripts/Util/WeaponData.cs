using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Data")]
[Serializable]
public class WeaponData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private Color weaponColor;
    [SerializeField] private Sprite primaryIcon;
    [SerializeField] private Sprite secondaryIcon;

    public Color GetWeaponColor() => weaponColor;

    public Sprite GetPrimaryIcon() => primaryIcon;

    public Sprite GetSecondaryIcon() => secondaryIcon;

}
