using MoreMountains.InventoryEngine;
using UnityEngine;

public class WeaponCollectible : BaseCollectible {

    [Header("Settings")]
    [SerializeField] private WeaponData weaponData;

    protected override void OnCollect(ItemPicker collectible) {

        base.OnCollect(collectible);
        playerController.AddWeapon(weaponData); // give player the weapon
        playerController.UpdateCurrentWeapon(); // update the current weapon

    }

    public override void OnLoad(string data) {

        Data saveData = JsonUtility.FromJson<Data>(data);
        isCollected = saveData.isCollected; // load data

        if (isCollected) {

            itemPicker.gameObject.SetActive(false); // disable the collectible
            playerController.AddWeapon(weaponData); // give player the weapon

        }
    }
}
