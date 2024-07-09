using MoreMountains.CorgiEngine;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Data")]
public class WeaponData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private PrimaryAction primaryAction;
    [SerializeField] private SecondaryAction secondaryAction;
    [SerializeField] private Color weaponColor;
    [SerializeField] private Sprite primaryIcon;
    [SerializeField] private Sprite secondaryIcon;
    [SerializeField] private float switchCooldown;

    public Weapon GetWeapon() => weapon;

    public PrimaryAction GetPrimaryAction() => primaryAction;

    public SecondaryAction GetSecondaryAction() => secondaryAction;

    public Color GetWeaponColor() => weaponColor;

    public Sprite GetPrimaryIcon() => primaryIcon;

    public Sprite GetSecondaryIcon() => secondaryIcon;

    public float GetSwitchCooldown() => switchCooldown;

}
