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

    public void SetWeapon(SpellData weaponData) => itemIcon.sprite = weaponData.GetSpellIcon();

    public void SetSelected(bool isSelected) {

        if (isSelected) slot.sprite = selectedSlot;
        else slot.sprite = unselectedSlot;

    }

    public Sprite getItemIconSprite() => itemIcon.sprite;
}
