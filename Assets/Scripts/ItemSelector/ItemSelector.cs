using UnityEngine;
using UnityEngine.UI;

public class ItemSelector : MonoBehaviour {

    [Header("Slots")]
    [SerializeField] private Image primaryWeaponFill;
    [SerializeField] private Image secondaryWeaponFill;
    [SerializeField] private ItemSlot[] slots;
    private int currSlotIndex;
    private int placementIndex; // index of the slot to place the weapon

    private void Start() {

        slots[0].SetSelected(true);
        primaryWeaponFill.sprite = slots[0].GetPrimaryIcon();
        secondaryWeaponFill.sprite = slots[0].GetSecondaryIcon();

    }

    public void AddWeapon(WeaponData weaponData) {

        if (placementIndex >= slots.Length) return; // if there are no more slots to place the weapon, return

        slots[placementIndex++].Initialize(weaponData); // add weapon to the current placement slot, then increment the placement index

    }

    public void SelectSlot(int slotIndex) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot
        currSlotIndex = slotIndex;
        slots[currSlotIndex].SetSelected(true); // select the new slot
        primaryWeaponFill.sprite = slots[currSlotIndex].GetPrimaryIcon();
        secondaryWeaponFill.sprite = slots[currSlotIndex].GetSecondaryIcon();

    }

    public void CycleSlot(int cycleAmount) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot

        currSlotIndex = (currSlotIndex + cycleAmount) % slots.Length; // cycle the slot index forward
        currSlotIndex = currSlotIndex < 0 ? slots.Length - 1 : currSlotIndex; // if the index is negative, set it to the last index

        slots[currSlotIndex].SetSelected(true); // select the new slot
        primaryWeaponFill.sprite = slots[currSlotIndex].GetPrimaryIcon();
        secondaryWeaponFill.sprite = slots[currSlotIndex].GetSecondaryIcon();

    }

    public int GetCurrWeapon() => currSlotIndex;

}
