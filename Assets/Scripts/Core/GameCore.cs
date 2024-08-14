using MoreMountains.InventoryEngine;
using UnityEngine;
using UnityEngine.UI;

public class GameCore : MonoBehaviour {

    [Header("References")]
    [SerializeField] private InventoryDisplay collectibleInventoryDisplay;
    [SerializeField] private Inventory keyInventory;
    [SerializeField] private InventoryDisplay keyInventoryDisplay;

    private void Start() {

        // force rebuild the layout of the inventory displays
        LayoutRebuilder.ForceRebuildLayoutImmediate(collectibleInventoryDisplay.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(keyInventoryDisplay.GetComponent<RectTransform>());

    }

    public InventoryDisplay GetCollectibleInventoryDisplay() => collectibleInventoryDisplay;

    public Inventory GetKeyInventory() => keyInventory;

    public InventoryDisplay GetKeyInventoryDisplay() => keyInventoryDisplay;

}
