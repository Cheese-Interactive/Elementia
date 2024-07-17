using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] private Inventory collectibleInventory;
    [SerializeField] private InventoryDisplay collectibleInventoryDisplay;
    [SerializeField] private int collectibleRows;
    [SerializeField] private int collectibleColumns;
    private List<BaseCollectible> requiredCollectibles;
    private bool isLevelComplete;
    private Coroutine resetCoroutine;

    private void Start() {

        // get all required collectibles
        requiredCollectibles = new List<BaseCollectible>();

        foreach (BaseCollectible collectible in FindObjectsOfType<BaseCollectible>())
            if (collectible.IsRequired())
                requiredCollectibles.Add(collectible);

        // check if there are enough slots to hold the required collectibles
        if (collectibleRows * collectibleColumns < requiredCollectibles.Count)
            Debug.LogError("There are not enough slots to hold the amount of required collectibles.");

        // setup the inventory display
        collectibleInventoryDisplay.NumberOfRows = collectibleRows;
        collectibleInventoryDisplay.NumberOfColumns = collectibleColumns;
        collectibleInventoryDisplay.SetupInventoryDisplay();

    }

    public void ResetAllResettables() {

        // reset all resettables (don't store in list because resettables can be added/removed from the game at any point)
        foreach (ResettableObject resettable in FindObjectsOfType<ResettableObject>())
            resettable.ResetObject();

    }

    public void ResetAllResettables(Vector3 playerPosition, float resetWaitDuration) {

        if (resetCoroutine != null) StopCoroutine(resetCoroutine); // stop reset coroutine if it is already running
        resetCoroutine = StartCoroutine(HandleReset(playerPosition, resetWaitDuration));

    }

    private IEnumerator HandleReset(Vector3 playerPosition, float resetWaitDuration) {

        List<ResettableObject> orderedResettables = FindObjectsOfType<ResettableObject>().ToList();
        orderedResettables.Sort((a, b) => Vector2.Distance(playerPosition, a.transform.position).CompareTo(Vector2.Distance(playerPosition, b.transform.position))); // sort in ascending order of distance from player

        // reset all resettables (don't store in list because resettables can be added/removed from the game at any point)
        foreach (ResettableObject resettable in FindObjectsOfType<ResettableObject>()) {

            resettable.ResetObject();
            yield return new WaitForSeconds(resetWaitDuration);

        }

        resetCoroutine = null;

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
