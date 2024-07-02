
using UnityEngine;
using UnityEngine.UI;

public class ItemSelector : MonoBehaviour {

    [Header("Slots")]
    [SerializeField] private Image selectedPrimarySpellSlot;
    [SerializeField] private Image selectedSecondarySpellSlot;
    [SerializeField] private ItemSlot[] slots;
    private int currSlotIndex;

    private void Start() {

        slots[0].SetSelected(true);
        selectedPrimarySpellSlot.sprite = slots[0].GetPrimarySpellIcon();
        selectedSecondarySpellSlot.sprite = slots[0].GetSecondarySpellIcon();

    }

    public void SetSpellData(SpellData spellData, int slotIndex) => slots[slotIndex].Initialize(spellData);

    public void SelectSlot(int slotIndex) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot
        currSlotIndex = slotIndex;
        slots[currSlotIndex].SetSelected(true); // select the new slot
        selectedPrimarySpellSlot.sprite = slots[currSlotIndex].GetPrimarySpellIcon();
        selectedSecondarySpellSlot.sprite = slots[currSlotIndex].GetSecondarySpellIcon();

    }

    public void CycleSlot(int cycleAmount) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot

        currSlotIndex = (currSlotIndex + cycleAmount) % slots.Length; // cycle the slot index forward
        currSlotIndex = currSlotIndex < 0 ? slots.Length - 1 : currSlotIndex; // if the index is negative, set it to the last index

        slots[currSlotIndex].SetSelected(true); // select the new slot
        selectedPrimarySpellSlot.sprite = slots[currSlotIndex].GetPrimarySpellIcon();
        selectedSecondarySpellSlot.sprite = slots[currSlotIndex].GetSecondarySpellIcon();

    }

    public int GetCurrWeapon() => currSlotIndex;

}
