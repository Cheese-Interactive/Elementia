using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Sprite unselectedHotbarSlot;
    [SerializeField] private Sprite blankSelectedHotbarSlot;
    [SerializeField] private Sprite blankSpellSlot;
    private SpellData spellData;
    private Image icon;

    private void Awake() {

        icon = GetComponent<Image>();
        icon.sprite = unselectedHotbarSlot;

    }

    public void SetSelected(bool isSelected) {

        if (isSelected) icon.sprite = spellData ? spellData.GetHotbarSelectedIcon() : blankSelectedHotbarSlot; // give hotbar selected spell icon if it exists, otherwise give blank selected hotbar icon
        else icon.sprite = unselectedHotbarSlot;

    }

    public void SetSpellData(SpellData spellData) => this.spellData = spellData;

    public Sprite GetSpellIcon() => spellData ? spellData.GetSpellIcon() : blankSpellSlot; // give spell icon if it exists, otherwise give blank spell icon

}
