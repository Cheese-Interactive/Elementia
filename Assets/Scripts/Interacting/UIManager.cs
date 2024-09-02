using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [Header("References")]
    private PlayerController playerController;
    private GameManager gameManager;

    [Header("Interact Indicator")]
    [SerializeField] private CanvasGroup interactIndicator;
    [SerializeField] private Image interactIcon;
    [SerializeField] private float interactFadeDuration;
    [SerializeField] private float interactTargetScale;
    [SerializeField] private float interactScaleDuration;
    private Tweener interactFadeTweener;
    private Tweener interactScaleTweener;

    [Header("Level Complete")]
    [SerializeField] private CanvasGroup levelCompleteScreen;
    [SerializeField] private float levelCompleteFadeDuration;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    private float timeScaleUnpauseDuration;
    private float timeScalePauseDuration;
    private Tweener timeScaleTweener;
    private Tweener levelCompleteFadeTweener;

    [Header("Loading Screen")]
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private float loadingScreenFadeDuration;

    public void Initialize() {

        playerController = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        interactIndicator.gameObject.SetActive(false); // hide interact indicator by default
        levelCompleteScreen.gameObject.SetActive(false); // hide level complete screen by default

        mainMenuButton.onClick.AddListener(() => LoadMainMenu());
        quitButton.onClick.AddListener(() => Application.Quit());

        timeScaleUnpauseDuration = gameManager.GetTimeScaleUnpauseDuration();
        timeScalePauseDuration = gameManager.GetTimeScalePauseDuration();

        HideLoadingScreen(); // hide the loading screen when game starts

    }

    private void OnDestroy() {

        if (interactFadeTweener != null && interactFadeTweener.IsActive()) interactFadeTweener.Kill(); // kill previous tween if it exists
        if (interactScaleTweener != null && interactScaleTweener.IsActive()) interactScaleTweener.Kill(); // kill previous tween if it exists

    }

    #region INTERACT INDICATOR
    public void ShowInteractIndicator() {

        interactIndicator.gameObject.SetActive(true); // show interact indicator

        if (interactFadeTweener != null && interactFadeTweener.IsActive()) interactFadeTweener.Kill(); // kill previous tween if it exists
        interactFadeTweener = interactIndicator.DOFade(1f, interactFadeDuration); // fade interact indicator in

    }

    public void HideInteractIndicator() {

        if (interactScaleTweener != null && interactScaleTweener.IsActive()) return; // if interact indicator is scaling, return because we don't want to hide it yet

        if (interactFadeTweener != null && interactFadeTweener.IsActive()) interactFadeTweener.Kill(); // kill previous tween if it exists
        interactFadeTweener = interactIndicator.DOFade(0f, interactFadeDuration).OnComplete(() => interactIndicator.gameObject.SetActive(false)); // fade interact indicator out and hide it when done

    }

    public void OnInteract() {

        if (interactScaleTweener != null && interactScaleTweener.IsActive()) interactScaleTweener.Kill(); // kill previous tween if it exists
        interactScaleTweener = interactIcon.transform.DOScale(interactTargetScale, interactScaleDuration).SetLoops(2, LoopType.Yoyo).OnComplete(() => HideInteractIndicator()); // scale interact indicator up and down then hide it

    }
    #endregion

    #region LOADING SCREEN
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
    #endregion

    #region LEVEL COMPLETE SCREEN
    public void ShowLevelCompleteScreen() {

        if (timeScaleTweener != null && timeScaleTweener.IsActive()) return; // if time scale is already lerping, return

        timeScaleTweener = DOVirtual.Float(Time.timeScale, 0f, timeScalePauseDuration, value => Time.timeScale = value).SetUpdate(true).SetEase(Ease.OutCirc).OnComplete(() => {

            playerController.SetWeaponAimEnabled(false); // disable weapon aiming (to hide reticle)
            playerController.SetWeaponHandlerEnabled(false); // disable weapon handler
            playerController.DisableCoreScripts(); // disable core scripts
            playerController.SetCharacterEnabled(false); // disable character
            playerController.SetInvulnerable(true); // set player invulnerable

            Cursor.lockState = CursorLockMode.None; // unlock cursor
            Cursor.visible = true; // show cursor

            levelCompleteScreen.alpha = 0f; // set level complete screen alpha to 0
            levelCompleteScreen.gameObject.SetActive(true); // show level complete screen

            RebuildLayout(levelCompleteScreen.GetComponent<RectTransform>()); // rebuild the layout of the level complete screen
            RebuildLayout(mainMenuButton.GetComponent<RectTransform>()); // rebuild the layout of the main menu button
            RebuildLayout(quitButton.GetComponent<RectTransform>()); // rebuild the layout of the quit button

            if (levelCompleteFadeTweener != null && levelCompleteFadeTweener.IsActive()) levelCompleteFadeTweener.Kill(); // kill previous tween if it exists
            levelCompleteFadeTweener = levelCompleteScreen.DOFade(1f, levelCompleteFadeDuration).SetUpdate(true); // fade level complete screen in with update mode set to true (to update while timescale is 0)

        }); // lerp time scale to 0 then show level complete screen
    }

    private void LoadMainMenu() {

        if (timeScaleTweener != null && timeScaleTweener.IsActive()) return; // if time scale is already lerping, return

        if (levelCompleteFadeTweener != null && levelCompleteFadeTweener.IsActive()) levelCompleteFadeTweener.Kill(); // kill previous tween if it exists

        playerController.SetCharacterEnabled(true); // enable character
        playerController.EnableCoreScripts(); // enable core scripts
        playerController.SetWeaponHandlerEnabled(true); // enable weapon handler
        playerController.SetWeaponAimEnabled(true); // enable weapon aiming (to show reticle)

        levelCompleteFadeTweener = levelCompleteScreen.DOFade(0f, levelCompleteFadeDuration).SetUpdate(true).OnComplete(() => {

            timeScaleTweener = DOVirtual.Float(Time.timeScale, 1f, timeScaleUnpauseDuration, value => Time.timeScale = value).SetUpdate(true).SetEase(Ease.InCirc).OnComplete(() => gameManager.LoadScene(gameManager.GetMainMenuSceneName()));

        }); // fade level complete screen out with update mode set to true (to update while timescale is 0), then lerp time scale to 1 then load the main menu scene

    }
    #endregion

    #region UTILITIES
    public float GetLoadingScreenFadeDuration() => loadingScreenFadeDuration;

    public void RebuildLayout(RectTransform rectTransform) => LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    #endregion

}
