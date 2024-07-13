using MoreMountains.InventoryEngine;
using UnityEngine;

public class WeaponCollectible : BaseCollectible {

    [Header("Settings")]
    [SerializeField] private WeaponData weaponData;

    protected override void OnCollect(ItemPicker collectible) {

        base.OnCollect(collectible);
        weaponSelector.AddWeapon(weaponData); // give player the weapon

    }
}
