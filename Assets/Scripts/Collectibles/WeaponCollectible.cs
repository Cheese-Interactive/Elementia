using MoreMountains.InventoryEngine;
using UnityEngine;

public class WeaponCollectible : BaseCollectible {

    [Header("Settings")]
    [SerializeField] private WeaponPair weaponPair;

    protected override void OnCollect(ItemPicker collectible) {

        base.OnCollect(collectible);
        playerController.AddWeapon(weaponPair); // give player the weapon
        playerController.UpdateCurrentWeapon(); // update the current weapon

    }

    public override void OnLoad(string data) {

        Data saveData = JsonUtility.FromJson<Data>(data);
        isCollected = saveData.isCollected; // load data

        // if the collectible has been collected, disable the collectible
        if (isCollected) {

            itemPicker.gameObject.SetActive(false);
            playerController.AddWeapon(weaponPair); // give player the weapon

        }
    }
}
