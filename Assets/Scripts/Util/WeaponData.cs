using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Data")]
public class WeaponData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private Sprite primaryIcon;
    [SerializeField] private Sprite secondaryIcon;
    [SerializeField] private Color weaponColor;

    public Sprite GetPrimaryIcon() => primaryIcon;

    public Sprite GetSecondaryIcon() => secondaryIcon;

    public Color GetWeaponColor() => weaponColor;

}
