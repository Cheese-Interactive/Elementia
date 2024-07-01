
using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour {

    [Header("Slots")]
    [SerializeField] private HotbarSlot[] slots;
    private int currSlotIndex;

    private void Start() => slots[0].SetSelected(true);

    public void SetWeapon(WeaponData weaponData, int slotIndex) => slots[slotIndex].SetWeapon(weaponData);

    [Header("Bique's fake hotbar")]
    //welcome to the worlds laziest code
    [SerializeField] private ItemSelectionIndicator[] indicators;
    [SerializeField] private Image selectedItemSprite;

    public void SelectSlot(int slotIndex) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot
        indicators[currSlotIndex].SetSelected(false);
        currSlotIndex = slotIndex;
        slots[currSlotIndex].SetSelected(true); // select the new slot
        indicators[currSlotIndex].SetSelected(true);
        selectedItemSprite.sprite = slots[currSlotIndex].getItemIconSprite();

    }

    public void CycleSlot(int cycleAmount) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot
        indicators[currSlotIndex].SetSelected(false);

        currSlotIndex = (currSlotIndex + cycleAmount) % this.slots.Length; // cycle the slot index forward
        currSlotIndex = currSlotIndex < 0 ? slots.Length - 1 : currSlotIndex; // if the index is negative, set it to the last index

        slots[currSlotIndex].SetSelected(true); // select the new slot
        indicators[currSlotIndex].SetSelected(true);
        selectedItemSprite.sprite = slots[currSlotIndex].getItemIconSprite();

    }

    public int GetCurrWeapon() => currSlotIndex;

}
