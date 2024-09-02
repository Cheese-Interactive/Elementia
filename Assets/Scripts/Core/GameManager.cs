using DG.Tweening;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private string mainMenuSceneName;
    private PlayerController playerController;
    private DataManager dataManager;
    private CooldownManager cooldownManager;
    private GameCore gameCore;
    private UIManager uiManager;
    private List<PhysicsObject> objectsToReset;
    private Coroutine sceneLoadCoroutine;
    private bool isLevelComplete;
    private bool isCooldownsEnabled;

    [Header("Collectible Inventory")]
    [SerializeField] private int collectibleRows;
    [SerializeField] private int collectibleColumns;
    private List<Collectible> requiredCollectibles;
    private List<KeyCollectible> keyCollectibles;
    private Inventory keyInventory;
    private InventoryDisplay collectibleInventoryDisplay;
    private RectTransform collectibleInventoryTransform;

    [Header("Key Inventory")]
    [SerializeField] private int keyRows;
    [SerializeField] private int keyColumns;
    private InventoryDisplay keyInventoryDisplay;
    private RectTransform keyInventoryTransform;

    [Header("End Zone")]
    [SerializeField] private float levelCompleteDelay;

    [Header("Pausing")]
    [SerializeField] private float timeScaleUnpauseDuration; // lerping to original time scale | unpausing
    [SerializeField] private float timeScalePauseDuration; // lerping to 0 | pausing
    private Tweener timeScaleTweener;
    private bool isPaused;

    private void Start() {

        playerController = FindObjectOfType<PlayerController>();
        dataManager = FindObjectOfType<DataManager>();
        gameCore = FindObjectOfType<GameCore>();
        cooldownManager = FindObjectOfType<CooldownManager>();
        uiManager = FindObjectOfType<UIManager>();

        // get the collectible inventory display and transform
        collectibleInventoryDisplay = gameCore.GetCollectibleInventoryDisplay();
        collectibleInventoryTransform = collectibleInventoryDisplay.GetComponent<RectTransform>();

        // get the key inventory, display, and transform
        keyInventory = gameCore.GetKeyInventory();
        keyInventoryDisplay = gameCore.GetKeyInventoryDisplay();
        keyInventoryTransform = keyInventoryDisplay.GetComponent<RectTransform>();

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

        RefreshInventoryLayouts(); // refresh the inventory layouts

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

    public void CheckLevelComplete() {

        if (isLevelComplete) return; // if the level is already complete, return

        // check if all required collectibles have been collected
        foreach (Collectible collectible in requiredCollectibles)
            if (!collectible.IsCollected())
                return;

        Debug.Log("Level Complete!");

        // if all required collectibles have been collected, set the level as complete
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
        uiManager.ShowLoadingScreen(); // show the loading screen before loading the scene
        yield return new WaitForSeconds(uiManager.GetLoadingScreenFadeDuration()); // wait for the loading screen to fade in
        MMSceneLoadingManager.LoadScene(sceneName);

    }

    public void ShowLevelCompleteScreen() {

        if (isPaused) TogglePause(); // unpause the game if it is paused to close the pause menu
        uiManager.ShowLevelCompleteScreen(); // show the level complete screen

    }

    #region PAUSING
    public void TogglePause() {

        if (isLevelComplete) return; // if the level is complete, return (don't allow pausing when the level is complete)

        if (isPaused) UnpauseGame();
        else PauseGame();

    }

    // TODO: deal with if pause menu is open and game is completed
    public void PauseGame() {

        if (isPaused) return; // if the game is already paused, return

        if (timeScaleTweener != null && timeScaleTweener.IsActive()) timeScaleTweener.Kill(); // kill previous tween if it exists

        timeScaleTweener = DOVirtual.Float(Time.timeScale, 0f, timeScalePauseDuration, value => Time.timeScale = value).SetUpdate(true).OnComplete(() => {

            playerController.SetWeaponAimEnabled(false); // disable weapon aiming (to hide reticle)
            playerController.DisableCoreScripts(); // disable core scripts
            isPaused = true;

        });
    }

    public void UnpauseGame() {

        if (!isPaused) return; // if the game is not paused, return

        if (timeScaleTweener != null && timeScaleTweener.IsActive()) timeScaleTweener.Kill(); // kill previous tween if it exists

        timeScaleTweener = DOVirtual.Float(Time.timeScale, 1f, timeScaleUnpauseDuration, value => Time.timeScale = value).SetUpdate(true).OnComplete(() => {

            playerController.EnableCoreScripts(); // enable core scripts
            playerController.SetWeaponAimEnabled(true); // enable weapon aiming (to show reticle)
            isPaused = false;

        });
    }
    #endregion

    #region UTILITIES
    public string GetMainMenuSceneName() => mainMenuSceneName;

    public bool IsCooldownsEnabled() => isCooldownsEnabled;

    public bool IsPaused() => isPaused;

    public float GetLevelCompleteDelay() => levelCompleteDelay;

    public float GetTimeScaleUnpauseDuration() => timeScaleUnpauseDuration;

    public float GetTimeScalePauseDuration() => timeScalePauseDuration;

    public bool IsLevelComplete() => isLevelComplete;

    public void RefreshInventoryLayouts() {

        uiManager.RebuildLayout(collectibleInventoryTransform);
        uiManager.RebuildLayout(keyInventoryTransform);

    }
    #endregion

}
