using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Data")]
public class WeaponData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private Color weaponColor;
    [SerializeField] private Sprite primaryIcon;
    [SerializeField] private Sprite secondaryIcon;

    public Color GetWeaponColor() => weaponColor;

    public Sprite GetPrimaryIcon() => primaryIcon;

    public Sprite GetSecondaryIcon() => secondaryIcon;

    public SerializableWeaponData GetSerializableData() => new SerializableWeaponData(this);

    public override bool Equals(object other) {

        if (other == null || other is not WeaponData) return false;

        WeaponData data = (WeaponData) other;

        return weaponColor == data.weaponColor && primaryIcon == data.primaryIcon && secondaryIcon == data.secondaryIcon;

    }

    public override int GetHashCode() => weaponColor.GetHashCode() ^ primaryIcon.GetHashCode() ^ secondaryIcon.GetHashCode();

}

[Serializable]
public class SerializableWeaponData {

    public Color weaponColor;
    public Sprite primaryIcon;
    public Sprite secondaryIcon;

    public SerializableWeaponData(WeaponData weaponData) {

        weaponColor = weaponData.GetWeaponColor();
        primaryIcon = weaponData.GetPrimaryIcon();
        secondaryIcon = weaponData.GetSecondaryIcon();

    }

    public WeaponData GetWeaponData(WeaponDatabase weaponDatabase) => weaponDatabase.GetWeaponData(weaponColor, primaryIcon, secondaryIcon);

}
