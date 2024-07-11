using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image fill;
    [SerializeField] private Sprite blankWeaponIcon;
    private WeaponData weaponData;

    [Header("Settings")]
    [SerializeField] private float selectedScale;
    [SerializeField] private float scaleDuration;
    private Vector3 startScale;
    private Color startColor;
    private Tweener scaleTweener;

    public void Awake() {

        startScale = transform.localScale;
        startColor = fill.color;

    }

    public void SetWeapon(WeaponData weaponData) {

        this.weaponData = weaponData;
        fill.color = weaponData.GetWeaponColor();

    }

    public void RemoveWeapon() {

        weaponData = null;
        fill.color = startColor;

    }

    public void SetSelected(bool isSelected) {

        if (scaleTweener != null && scaleTweener.IsActive()) scaleTweener.Kill(); // kill the previous scale tweener if it's still active

        scaleTweener = transform.DOScale(isSelected ? startScale * selectedScale : startScale, scaleDuration);

    }

    public Sprite GetPrimaryIcon() => weaponData && weaponData.GetPrimaryIcon() ? weaponData.GetPrimaryIcon() : blankWeaponIcon;

    public Sprite GetSecondaryIcon() => weaponData && weaponData.GetSecondaryIcon() ? weaponData.GetSecondaryIcon() : blankWeaponIcon;

}
