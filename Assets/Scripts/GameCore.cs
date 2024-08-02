using MoreMountains.InventoryEngine;
using UnityEngine;

public class GameCore : MonoBehaviour {

    [Header("References")]
    [SerializeField] private InventoryDisplay collectibleInventoryDisplay;
    [SerializeField] private InventoryDisplay keyInventoryDisplay;

    public InventoryDisplay GetCollectibleInventoryDisplay() => collectibleInventoryDisplay;

    public InventoryDisplay GetKeyInventoryDisplay() => keyInventoryDisplay;

}
