using MoreMountains.Tools;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelector : MMPersistentBase {

    [Header("References")]
    private PlayerController playerController;
    private WeaponDatabase weaponDatabase;

    [Header("Slots")]
    [SerializeField] private Image primaryWeaponFill;
    [SerializeField] private Image secondaryWeaponFill;
    [SerializeField] private WeaponSlot[] slots;
    private int currSlotIndex;
    private int placementIndex; // index of the slot to place the weapon

    [Header("Data")]
    private WeaponData[] slotData;
    private bool isDataLoaded;

    [Serializable]
    protected struct Data {

        public SerializableWeaponData[] slotData;

        public Data(SerializableWeaponData[] slotData) {

            this.slotData = slotData;

        }
    }

    // runs before OnLoad()
    private void Awake() {

        slotData = new WeaponData[slots.Length]; // initialize the slot data array
        playerController = FindObjectOfType<PlayerController>(); // initialize player controller here because it is used in OnLoad()
        weaponDatabase = FindObjectOfType<WeaponDatabase>(); // initialize weapon database here because it is used in OnLoad()

    }

    private void Start() {

        // if there is no data loaded, load default weapons
        if (!isDataLoaded)
            LoadDefaultWeapons();

        slots[currSlotIndex].SetSelected(true); // select the first slot
        UpdateCurrentWeapon(); // update weapons

    }

    private void LoadDefaultWeapons() {

        WeaponData[] defaultWeapons = playerController.GetDefaultWeapons(); // get default weapons

        // add default weapons to slots
        foreach (WeaponData weaponData in defaultWeapons)
            AddWeapon(weaponData);

        placementIndex = defaultWeapons.Length < slots.Length ? defaultWeapons.Length : -1; // set placement index to the first empty slot or -1 if there are no empty slots

    }

    public void SetWeapon(WeaponData weaponData, int slotIndex) {

        // check if there are no more slots to place the weapon
        if (placementIndex < 0) {

            Debug.LogWarning("No more slots to place weapon: " + weaponData.name); // log a warning if there are no more slots to place the weapon
            return;

        }

        playerController.AddWeapon(weaponData);
        slots[slotIndex].SetWeapon(weaponData); // set weapon to the specified slot
        slotData[slotIndex] = weaponData; // update specified slot weapon data

        // set placement index to first empty slot
        for (int i = 0; i < GetSlotCount(); i++) {

            if (slotData[i] == null) {

                placementIndex = i; // update placement index to current slot index

                // update current weapon if it is the weapon being modified
                if (GetCurrentWeapon() == weaponData)
                    UpdateCurrentWeapon();

                return; // return if there is an empty slot

            }
        }

        placementIndex = -1; // if there are no more slots, set placement index to -1

        // update current weapon if it is the weapon being modified
        if (GetCurrentWeapon() == weaponData)
            UpdateCurrentWeapon();

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

        // update current weapon if it is the equipped weapon
        if (GetCurrSlotIndex() == slotIndex)
            UpdateCurrentWeapon();

    }

    public void UpdateCurrentWeapon() {

        playerController.UpdateCurrentWeapon(); // update the current weapon
        primaryWeaponFill.sprite = slots[currSlotIndex].GetPrimaryIcon();
        secondaryWeaponFill.sprite = slots[currSlotIndex].GetSecondaryIcon();

    }

    public void SelectSlot(int slotIndex) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot
        currSlotIndex = slotIndex;
        slots[currSlotIndex].SetSelected(true); // select the new slot

        UpdateCurrentWeapon(); // update weapons

    }

    public void CycleSlot(int cycleAmount) {

        slots[currSlotIndex].SetSelected(false); // deselect the current slot

        currSlotIndex = (currSlotIndex + cycleAmount) % slots.Length; // cycle the slot index forward
        currSlotIndex = currSlotIndex < 0 ? slots.Length - 1 : currSlotIndex; // if the index is negative, set it to the last index

        slots[currSlotIndex].SetSelected(true); // select the new slot

        UpdateCurrentWeapon(); // update weapons

    }

    public override string OnSave() {

        SerializableWeaponData[] data = new SerializableWeaponData[slots.Length];

        // get serializable data for each slot
        for (int i = 0; i < data.Length; i++)
            data[i] = slotData[i] ? slotData[i].GetSerializableData() : null; // get serializable data for each slot

        Data saveData = new Data(data);
        return JsonUtility.ToJson(saveData); // save data

    }

    // runs in between Awake() and Start() IF THERE IS DATA TO LOAD
    public override void OnLoad(string data) {

        // clear all slots
        for (int i = 0; i < GetSlotCount(); i++)
            RemoveWeapon(i);

        Data saveData = JsonUtility.FromJson<Data>(data);

        placementIndex = 0; // reset placement index to 0 because all slots are blank prior to loading weapons (must be done before adding weapons)

        // load weapons from save data & add loaded weapons to slots
        for (int i = 0; i < saveData.slotData.Length; i++) {

            if (!saveData.slotData[i].IsNull()) { // if there is a weapon to load

                slotData[i] = saveData.slotData[i].GetWeaponData(weaponDatabase); // get weapon data from the loaded data and add it to the slot
                SetWeapon(slotData[i], i); // set the weapon to the slot

            }
        }

        // set placement index to first empty slot
        for (int i = 0; i < GetSlotCount(); i++) {

            if (slotData[i] == null) {

                placementIndex = i; // update placement index to current slot index
                isDataLoaded = true; // set data loaded to true
                return; // return if there is an empty slot

            }
        }

        isDataLoaded = true; // set data loaded to true

    }

    public WeaponData GetCurrentWeapon() => slotData[currSlotIndex];

    public WeaponData GetWeaponAt(int slotIndex) => slotData[slotIndex];

    public int GetCurrSlotIndex() => currSlotIndex;

    public int GetSlotCount() => slots.Length;

}
