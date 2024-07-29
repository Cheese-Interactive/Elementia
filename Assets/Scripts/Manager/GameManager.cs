using MoreMountains.InventoryEngine;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("References")]
    private CooldownManager cooldownManager;

    [Header("Settings")]
    private List<BaseCollectible> requiredCollectibles;
    private List<KeyCollectible> keyCollectibles;
    private bool isLevelComplete;
    private bool isCooldownsEnabled;

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

        cooldownManager = FindObjectOfType<CooldownManager>();

        // get all required & key collectibles
        requiredCollectibles = new List<BaseCollectible>();
        keyCollectibles = new List<KeyCollectible>();

        foreach (BaseCollectible collectible in FindObjectsOfType<BaseCollectible>()) {

            if (collectible.IsRequired())
                requiredCollectibles.Add(collectible);
            else if (collectible is KeyCollectible)
                keyCollectibles.Add((KeyCollectible) collectible);

        }

        // check if there are enough slots to hold the required collectibles
        if (collectibleRows * collectibleColumns < requiredCollectibles.Count)
            Debug.LogError("There are not enough slots to hold the amount of required collectibles. There are " + requiredCollectibles.Count + " required collectibles in this scene.");

        // check if there are enough slots to hold the key collectibles
        if (keyRows * keyColumns < keyCollectibles.Count)
            Debug.LogError("There are not enough slots to hold the amount of key collectibles. There are " + keyCollectibles.Count + " key collectibles in this scene.");

        // setup the collectible inventory display
        collectibleInventoryDisplay.NumberOfRows = collectibleRows;
        collectibleInventoryDisplay.NumberOfColumns = collectibleColumns;
        collectibleInventoryDisplay.SetupInventoryDisplay();

        // setup the key inventory display
        keyInventoryDisplay.NumberOfRows = keyRows;
        keyInventoryDisplay.NumberOfColumns = keyColumns;
        keyInventoryDisplay.SetupInventoryDisplay();

        isCooldownsEnabled = true; // enable cooldowns by default

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

    public void SetCooldownsEnabled(bool isCooldownsEnabled) {

        // clear all cooldown data if cooldowns are disabled
        if (!isCooldownsEnabled)
            cooldownManager.ClearCooldownData();

        this.isCooldownsEnabled = isCooldownsEnabled;

    }

    public bool IsCooldownsEnabled() => isCooldownsEnabled;

}
