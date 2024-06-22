using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Sprite selectedSlot;
    private Image slot;
    private Sprite unselectedSlot;

    private void Awake() {

        slot = GetComponent<Image>();

        unselectedSlot = slot.sprite;

    }

    public void SetWeapon(WeaponData weaponData) => itemIcon.sprite = weaponData.GetIcon();

    public void SetSelected(bool isSelected) {

        if (isSelected) slot.sprite = selectedSlot;
        else slot.sprite = unselectedSlot;

    }
}
