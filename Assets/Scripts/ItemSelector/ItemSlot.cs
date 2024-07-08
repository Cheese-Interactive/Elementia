using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image fill;
    [SerializeField] private Sprite blankWeaponIcon;
    private WeaponData weaponData;

    [Header("Settings")]
    [SerializeField] private float selectedScale;
    [SerializeField] private float scaleDuration;
    private Vector3 initialScale;
    private Tweener scaleTweener;

    private void Start() => initialScale = transform.localScale;

    public void Initialize(WeaponData weaponData) {

        this.weaponData = weaponData;

        fill.color = weaponData.GetWeaponColor();

    }

    public void SetSelected(bool isSelected) {

        if (scaleTweener != null && scaleTweener.IsActive()) scaleTweener.Kill(); // kill the previous scale tweener if it's still active

        scaleTweener = transform.DOScale(isSelected ? initialScale * selectedScale : initialScale, scaleDuration);

    }

    public Sprite GetPrimaryIcon() => weaponData && weaponData.GetPrimaryIcon() ? weaponData.GetPrimaryIcon() : blankWeaponIcon;

    public Sprite GetSecondaryIcon() => weaponData && weaponData.GetSecondaryIcon() ? weaponData.GetSecondaryIcon() : blankWeaponIcon;

}
