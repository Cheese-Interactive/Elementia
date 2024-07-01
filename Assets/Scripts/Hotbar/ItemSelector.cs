
using UnityEngine;
using UnityEngine.UI;

public class ItemSelector : MonoBehaviour {

    [Header("Slots")]
    [SerializeField] private Image selectedSpellSlot;
    [SerializeField] private ItemSlot[] slots;
    private int currSlotIndex;

    private void Start() {

        slots[0].SetSelected(true);
        selectedSpellSlot.sprite = slots[0].GetSpellIcon();

    }

    public void SetSpell(SpellData spellData, int slotIndex) => slots[slotIndex].SetSpellData(spellData);

    public void SelectSlot(int slotIndex) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot
        currSlotIndex = slotIndex;
        slots[currSlotIndex].SetSelected(true); // select the new slot
        selectedSpellSlot.sprite = slots[currSlotIndex].GetSpellIcon();

    }

    public void CycleSlot(int cycleAmount) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot

        currSlotIndex = (currSlotIndex + cycleAmount) % slots.Length; // cycle the slot index forward
        currSlotIndex = currSlotIndex < 0 ? slots.Length - 1 : currSlotIndex; // if the index is negative, set it to the last index

        slots[currSlotIndex].SetSelected(true); // select the new slot
        selectedSpellSlot.sprite = slots[currSlotIndex].GetSpellIcon();

    }

    public int GetCurrWeapon() => currSlotIndex;

}
