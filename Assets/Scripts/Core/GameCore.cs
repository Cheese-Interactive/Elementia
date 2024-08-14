using MoreMountains.InventoryEngine;
using UnityEngine;

public class GameCore : MonoBehaviour {

    [Header("References")]
    [SerializeField] private InventoryDisplay collectibleInventoryDisplay;
    [SerializeField] private Inventory keyInventory;
    [SerializeField] private InventoryDisplay keyInventoryDisplay;

    public InventoryDisplay GetCollectibleInventoryDisplay() => collectibleInventoryDisplay;

    public Inventory GetKeyInventory() => keyInventory;

    public InventoryDisplay GetKeyInventoryDisplay() => keyInventoryDisplay;

}
