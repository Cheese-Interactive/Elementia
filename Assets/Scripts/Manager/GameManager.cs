using MoreMountains.InventoryEngine;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("Settings")]
    private List<BaseCollectible> requiredCollectibles;
    private List<BaseCollectible> keyCollectibles;
    private bool isLevelComplete;
    private Coroutine resetCoroutine;

    [Header("Collectible Inventory")]
    [SerializeField] private Inventory collectibleInventory;
    [SerializeField] private InventoryDisplay collectibleInventoryDisplay;
    [SerializeField] private int collectibleRows;
    [SerializeField] private int collectibleColumns;

    [Header("Key Inventory")]
    [SerializeField] private Inventory keyInventory;
    [SerializeField] private InventoryDisplay keyInventoryDisplay;
    [SerializeField] private int keyRows;
    [SerializeField] private int keyColumns;

    private void Start() {

        // get all required & key collectibles
        requiredCollectibles = new List<BaseCollectible>();
        keyCollectibles = new List<BaseCollectible>();

        foreach (BaseCollectible collectible in FindObjectsOfType<BaseCollectible>())
            if (collectible.IsRequired())
                requiredCollectibles.Add(collectible);
            else if (collectible.IsKey())
                keyCollectibles.Add(collectible);

        // check if there are enough slots to hold the required collectibles
        if (collectibleRows * collectibleColumns < requiredCollectibles.Count)
            Debug.LogError("There are not enough slots to hold the amount of required collectibles.");

        // check if there are enough slots to hold the key collectibles
        if (keyRows * keyColumns < keyCollectibles.Count)
            Debug.LogError("There are not enough slots to hold the amount of key collectibles.");

        // setup the collectible inventory display
        collectibleInventoryDisplay.NumberOfRows = collectibleRows;
        collectibleInventoryDisplay.NumberOfColumns = collectibleColumns;
        collectibleInventoryDisplay.SetupInventoryDisplay();

        // setup the key inventory display
        keyInventoryDisplay.NumberOfRows = keyRows;
        keyInventoryDisplay.NumberOfColumns = keyColumns;
        keyInventoryDisplay.SetupInventoryDisplay();

    }

    public void ResetAllResettables() {

        // reset all resettables (don't store in list because resettables can be added/removed from the game at any point)
        foreach (ResettableObject resettable in FindObjectsOfType<ResettableObject>())
            resettable.ResetObject();

    }

    public void CheckVictory() {

        if (isLevelComplete) // if the level is already complete, return
            return;

        // check if all required collectibles have been collected
        foreach (BaseCollectible collectible in requiredCollectibles)
            if (!collectible.IsCollected())
                return;

        // if all required collectibles have been collected, set the level as complete
        print("Level Complete!");
        isLevelComplete = true;

    }
}
