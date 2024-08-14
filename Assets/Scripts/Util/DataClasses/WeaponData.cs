using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Data")]
public class WeaponData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private Color weaponColor;
    [SerializeField] private Color cooldownColor;
    [SerializeField] private Sprite primaryIcon;
    [SerializeField] private Sprite secondaryIcon;

    public Color GetWeaponColor() => weaponColor;

    public Color GetCooldownColor() => cooldownColor;

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
    public string primaryIconPath;
    public string secondaryIconPath;

    public SerializableWeaponData(WeaponData weaponData) {

        weaponColor = weaponData.GetWeaponColor();

        if (weaponData.GetPrimaryIcon() != null) {

            string tempPrimaryPath = AssetDatabase.GetAssetPath(weaponData.GetPrimaryIcon());
            primaryIconPath = tempPrimaryPath.Substring(("Assets/Resources/").Length, tempPrimaryPath.LastIndexOf(".") - ("Assets/Resources/").Length); // get the path of the primary icon (disclude "Assets/Resources/" and file extension from the path)

        } else {

            Debug.LogWarning("Primary icon is null for weapon data " + weaponData.name + ", this weapon will not be loaded next time.");

        }

        if (weaponData.GetSecondaryIcon() != null) {

            string tempSecondaryPath = AssetDatabase.GetAssetPath(weaponData.GetSecondaryIcon());
            secondaryIconPath = tempSecondaryPath.Substring(("Assets/Resources/").Length, tempSecondaryPath.LastIndexOf(".") - ("Assets/Resources/").Length); // get the path of the secondary icon (disclude "Assets/Resources/" and file extension from the path)

        } else {

            Debug.LogWarning("Secondary icon is null for weapon data " + weaponData.name + ", this weapon will not be loaded next time.");

        }
    }

    public WeaponData GetWeaponData(WeaponDatabase weaponDatabase) => weaponDatabase.GetWeaponData(weaponColor, Resources.Load<Sprite>(primaryIconPath), Resources.Load<Sprite>(secondaryIconPath));

    public bool IsNull() => weaponColor == null || primaryIconPath == "" || secondaryIconPath == "";

}
