using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotbar : MonoBehaviour {

    [Header("Slots")]
    [SerializeField] private HotbarSlot[] slots;
    private int currSlotIndex;

    private void Start() => slots[0].SetSelected(true);

    public void SetWeapon(WeaponData weaponData, int slotIndex) => slots[slotIndex].SetWeapon(weaponData);

    public void SelectSlot(int slotIndex) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot
        currSlotIndex = slotIndex;
        slots[currSlotIndex].SetSelected(true); // select the new slot

    }

    public void CycleSlot(int cycleAmount) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot

        currSlotIndex = (currSlotIndex + cycleAmount) % this.slots.Length; // cycle the slot index forward
        currSlotIndex = currSlotIndex < 0 ? slots.Length - 1 : currSlotIndex; // if the index is negative, set it to the last index

        slots[currSlotIndex].SetSelected(true); // select the new slot

    }

    public int GetCurrWeapon() => currSlotIndex;

}
