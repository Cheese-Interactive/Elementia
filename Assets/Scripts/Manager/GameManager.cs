using MoreMountains.InventoryEngine;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private Inventory collectibleInventory;
    [SerializeField] private InventoryDisplay collectibleInventoryDisplay;
    [SerializeField] private int collectibleRows;
    [SerializeField] private int collectibleColumns;
    [SerializeField] private Collectible[] requiredCollectibles;
    private bool isLevelComplete;

    private void Start() {

        // check if there are enough slots to hold the required collectibles
        if (collectibleRows * collectibleColumns < requiredCollectibles.Length)
            Debug.LogError("There are not enough slots to hold the amount of required collectibles.");

        // setup the inventory display
        collectibleInventoryDisplay.NumberOfRows = collectibleRows;
        collectibleInventoryDisplay.NumberOfColumns = collectibleColumns;
        collectibleInventoryDisplay.SetupInventoryDisplay();

    }

    public void CheckVictory() {

        if (isLevelComplete) // if the level is already complete, return
            return;

        // check if all required collectibles have been collected
        foreach (Collectible collectible in requiredCollectibles)
            if (!collectible.IsCollected())
                return;

        // if all required collectibles have been collected, set the level as complete
        print("Level Complete!");
        isLevelComplete = true;

    }
}
