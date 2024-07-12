using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelector : MMPersistentBase {

    [Header("References")]
    private PlayerController playerController;

    [Header("Slots")]
    [SerializeField] private Image primaryWeaponFill;
    [SerializeField] private Image secondaryWeaponFill;
    [SerializeField] private WeaponSlot[] slots;
    private int currSlotIndex;
    private int placementIndex; // index of the slot to place the weapon

    [Header("Data")]
    private WeaponData[] slotData;

    [Serializable]
    protected struct Data {

        public WeaponData[] slotData;

        public Data(int size) { // use array instead of dictionary for serialization (and array over list for performance)

            slotData = new WeaponData[size];

        }
    }

    // runs before OnLoad()
    private void Awake() {

        slotData = new WeaponData[slots.Length]; // initialize the slot data array
        playerController = FindObjectOfType<PlayerController>(); // initialize player controller here because it is used in OnLoad()

        WeaponData[] defaultWeapons = playerController.GetDefaultWeapons(); // get default weapons

        // add default weapons to slots
        foreach (WeaponData weaponData in defaultWeapons)
            AddWeapon(weaponData);

        placementIndex = defaultWeapons.Length; // set placement index to the first empty slot

    }

    private void Start() => UpdateWeapons(); // update weapons

    public void SetWeapon(WeaponData weaponData, int slotIndex) {

        if (placementIndex >= slots.Length) return; // if there are no more slots to place the weapon, return

        playerController.AddWeapon(weaponData);
        slots[slotIndex].SetWeapon(weaponData); // set weapon to the specified slot
        slotData[slotIndex] = weaponData; // update specified slot weapon data

        // set placement index to first empty slot
        for (int i = 0; i < GetSlotCount(); i++) {

            if (slotData[i] == null) {

                placementIndex = i; // update placement index to current slot index
                return; // return if there is an empty slot

            }
        }

        placementIndex = -1; // if there are no more slots, set placement index to -1
        UpdateWeapons(); // update weapons

    }

    // util method to add weapons (uses the SetWeapon method)
    public void AddWeapon(WeaponData weaponData) => SetWeapon(weaponData, placementIndex);

    public void RemoveWeapon(int slotIndex) {

        if (slotIndex < 0 || slotData[slotIndex] == null) return; // if there is no weapon to remove, return

        playerController.RemoveWeapon(slotIndex);
        slots[slotIndex].RemoveWeapon(); // remove weapon from the current placement slot
        slotData[slotIndex] = null; // remove current placement slot weapon data

        // set placement index to first empty slot
        for (int i = 0; i < GetSlotCount(); i++) {

            if (slotData[i] == null) {

                placementIndex = i; // update placement index to current slot index (guaranteed to find one because a slot was just emptied)
                break;

            }
        }

        UpdateWeapons(); // update weapons

    }

    public void UpdateWeapons() {

        playerController.UpdateCurrentWeapon(); // update the current weapon
        primaryWeaponFill.sprite = slots[currSlotIndex].GetPrimaryIcon();
        secondaryWeaponFill.sprite = slots[currSlotIndex].GetSecondaryIcon();

    }

    public void SelectSlot(int slotIndex) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot
        currSlotIndex = slotIndex;
        slots[currSlotIndex].SetSelected(true); // select the new slot

        UpdateWeapons(); // update weapons

    }

    public void CycleSlot(int cycleAmount) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot

        currSlotIndex = (currSlotIndex + cycleAmount) % slots.Length; // cycle the slot index forward
        currSlotIndex = currSlotIndex < 0 ? slots.Length - 1 : currSlotIndex; // if the index is negative, set it to the last index

        slots[currSlotIndex].SetSelected(true); // select the new slot

        UpdateWeapons(); // update weapons

    }

    public override string OnSave() {

        Data saveData = new() { slotData = slotData };
        return JsonUtility.ToJson(saveData); // save data

    }

    // runs in between Awake() and Start() IF THERE IS DATA TO LOAD
    public override void OnLoad(string data) {

        // clear all slots
        for (int i = 0; i < GetSlotCount(); i++)
            RemoveWeapon(i);

        Data saveData = JsonUtility.FromJson<Data>(data);
        slotData = saveData.slotData; // load data

        placementIndex = 0; // reset placement index to 0 because all slots are blank prior to loading weapons (must be done before adding weapons)

        // add loaded weapons to  slots
        for (int i = 0; i < slotData.Length; i++)
            if (slotData[i] != null)
                SetWeapon(slotData[i], i);

        // set placement index to first empty slot
        for (int i = 0; i < GetSlotCount(); i++) {

            if (slotData[i] == null) {

                placementIndex = i; // update placement index to current slot index
                slots[currSlotIndex].SetSelected(true); // select the first slot
                UpdateWeapons(); // update weapons
                return; // return if there is an empty slot

            }
        }

        placementIndex = -1; // if there are no more slots, set placement index to -1

        slots[currSlotIndex].SetSelected(true); // select the first slot
        UpdateWeapons(); // update weapons

    }

    public WeaponData GetCurrentWeapon() => slotData[currSlotIndex];

    public WeaponData GetWeaponAt(int slotIndex) => slotData[slotIndex];

    public int GetCurrSlotIndex() => currSlotIndex;

    public int GetSlotCount() => slots.Length;

    public bool IsSlotsEmpty() => placementIndex == 0;

}
