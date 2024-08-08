using DG.Tweening;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("References")]
    private PlayerController playerController;
    private DataManager dataManager;
    private CooldownManager cooldownManager;
    private GameCore gameCore;
    private List<PhysicsObject> objectsToReset;
    private Coroutine sceneLoadCoroutine;

    [Header("UI References")]
    [SerializeField] private CanvasGroup loadingScreen;

    [Header("Settings")]
    [SerializeField] private float loadingScreenFadeDuration;
    private List<Collectible> requiredCollectibles;
    private List<KeyCollectible> keyCollectibles;
    private bool isLevelComplete;
    private bool isCooldownsEnabled;

    [Header("Collectible Inventory")]
    [SerializeField] private int collectibleRows;
    [SerializeField] private int collectibleColumns;
    private Inventory keyInventory;
    private InventoryDisplay collectibleInventoryDisplay;

    [Header("Key Inventory")]
    [SerializeField] private int keyRows;
    [SerializeField] private int keyColumns;
    private InventoryDisplay keyInventoryDisplay;

    [Header("Pausing")]
    private bool isPaused;

    private void Start() {

        playerController = FindObjectOfType<PlayerController>();
        dataManager = FindObjectOfType<DataManager>();
        gameCore = FindObjectOfType<GameCore>();
        cooldownManager = FindObjectOfType<CooldownManager>();

        // get the collectible inventory display & key inventory display
        collectibleInventoryDisplay = gameCore.GetCollectibleInventoryDisplay();
        keyInventory = gameCore.GetKeyInventory();
        keyInventoryDisplay = gameCore.GetKeyInventoryDisplay();

        // get all required & key collectibles
        requiredCollectibles = new List<Collectible>();
        keyCollectibles = new List<KeyCollectible>();

        foreach (Collectible collectible in FindObjectsOfType<Collectible>()) {

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

        HideLoadingScreen(); // hide the loading screen when game starts

    }

    private void OnDestroy() => DOTween.KillAll(); // kill all tweens

    public void ResetAllResettables(float resetDuration) {

        objectsToReset = new List<PhysicsObject>();

        // reset all resettables (don't store in list because resettables can be added/removed from the game at any point)
        foreach (PhysicsObject physicsObject in FindObjectsOfType<PhysicsObject>()) {

            if (physicsObject.IsResettable()) {

                physicsObject.StartReset(resetDuration);
                objectsToReset.Add(physicsObject);

            }
        }
    }

    public void CancelResets() {

        // cancel all resets
        foreach (PhysicsObject physicsObject in objectsToReset)
            physicsObject.CancelReset();

        objectsToReset.Clear();

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

    public void SetCooldownsEnabled(bool isCooldownsEnabled) {

        // clear all cooldown data if cooldowns are disabled
        if (!isCooldownsEnabled)
            cooldownManager.ClearCooldownData();

        this.isCooldownsEnabled = isCooldownsEnabled;

    }

    public void RemoveKey() => keyInventory.RemoveItem(keyInventory.NumberOfFilledSlots - 1, 1, false); // remove key if possible without showing warnings

    public bool HasKey() => keyInventory.NumberOfFilledSlots > 0;

    public void LoadScene(string sceneName) {

        if (sceneLoadCoroutine != null) StopCoroutine(sceneLoadCoroutine); // stop the previous scene load coroutine if it exists
        sceneLoadCoroutine = StartCoroutine(HandleSceneLoad(sceneName));

    }

    private IEnumerator HandleSceneLoad(string sceneName) {

        dataManager.SaveData(); // IMPORTANT: save data before loading a new scene
        ShowLoadingScreen(); // show the loading screen before loading the scene
        yield return new WaitForSeconds(loadingScreenFadeDuration); // wait for the loading screen to fade in
        MMSceneLoadingManager.LoadScene(sceneName);

    }

    public void ShowLoadingScreen() {

        // enable the loading screen and fade it in
        loadingScreen.alpha = 0f;
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.DOFade(1f, loadingScreenFadeDuration);

    }

    private void HideLoadingScreen() {

        // fade out the loading screen and then disable it
        loadingScreen.alpha = 1f;
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.DOFade(0f, loadingScreenFadeDuration).OnComplete(() => loadingScreen.gameObject.SetActive(false));

    }

    public void TogglePause() {

        if (isPaused)
            UnpauseGame();
        else
            PauseGame();

    }

    public void PauseGame() {

        if (isPaused) return; // if the game is already paused, return

        playerController.SetWeaponAimEnabled(false); // disable weapon aiming (to hide reticle)
        playerController.DisableCoreScripts(); // disable core scripts
        Time.timeScale = 0f;
        isPaused = true;

    }

    public void UnpauseGame() {

        if (!isPaused) return; // if the game is not paused, return

        playerController.EnableCoreScripts(); // enable core scripts
        playerController.SetWeaponAimEnabled(true); // enable weapon aiming (to show reticle)
        Time.timeScale = 1f;
        isPaused = false;

    }

    public bool IsCooldownsEnabled() => isCooldownsEnabled;

    public bool IsPaused() => isPaused;

}
