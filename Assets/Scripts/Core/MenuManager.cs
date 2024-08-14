using DG.Tweening;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    [Header("References")]
    private Animator anim;
    private Coroutine confirmationButtonsCoroutine;
    private Coroutine sceneLoadCoroutine;

    [Header("UI References")]
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private Button playButton;
    [SerializeField] private Button resetDataButton;
    [SerializeField] private GameObject confirmationButtons;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string gameSceneName;
    [SerializeField] private float loadingScreenFadeDuration;

    private IEnumerator Start() {

        anim = GetComponent<Animator>();

        // setup button listeners
        playButton.onClick.AddListener(Play);
        resetDataButton.onClick.AddListener(ResetData);
        confirmButton.onClick.AddListener(ConfirmResetData);
        cancelButton.onClick.AddListener(CancelResetData);
        quitButton.onClick.AddListener(Quit);

        // disable buttons until menu animations are finished
        playButton.interactable = false;
        resetDataButton.interactable = false;
        confirmButton.interactable = false;
        cancelButton.interactable = false;
        quitButton.interactable = false;

        // disable confirmation buttons
        confirmationButtons.SetActive(false);

        HideLoadingScreen(); // hide the loading screen when game starts

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // wait for menu animations to finish

        // enable buttons
        playButton.interactable = true;
        resetDataButton.interactable = true;
        quitButton.interactable = true;

    }

    private void OnDestroy() => DOTween.KillAll(); // kill all tweens

    private void Play() => LoadScene(gameSceneName);

    private void ResetData() {

        // slide confirmation buttons in
        if (confirmationButtonsCoroutine != null) StopCoroutine(confirmationButtonsCoroutine); // stop any existing coroutines
        confirmationButtonsCoroutine = StartCoroutine(HandleConfirmationButtonsSlide(true));

    }

    private IEnumerator HandleConfirmationButtonsSlide(bool slideIn) {

        // enable confirmation buttons
        confirmationButtons.SetActive(true);

        if (slideIn) { // if sliding in

            // disable all other buttons before animation starts
            playButton.interactable = false;
            resetDataButton.interactable = false;
            quitButton.interactable = false;

        } else { // if sliding out

            // disable confirmation buttons before animation starts
            confirmButton.interactable = false;
            cancelButton.interactable = false;

        }

        if (slideIn)
            anim.SetTrigger("confirmationButtonsSlideIn");
        else
            anim.SetTrigger("confirmationButtonsSlideOut");

        yield return null; // wait for animation to start
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // wait for confirmation buttons to slide

        if (slideIn) { // if sliding in

            // enable confirmation buttons after animation finishes
            confirmButton.interactable = true;
            cancelButton.interactable = true;

        } else { // if sliding out

            // enable all other buttons after animation finishes
            playButton.interactable = true;
            resetDataButton.interactable = true;
            quitButton.interactable = true;

            // disable confirmation buttons
            confirmationButtons.SetActive(false);

        }
    }

    private void ConfirmResetData() {

        MMSaveLoadManager.DeleteAllSaveFiles();

        // slide confirmation buttons out
        if (confirmationButtonsCoroutine != null) StopCoroutine(confirmationButtonsCoroutine); // stop any existing coroutines
        confirmationButtonsCoroutine = StartCoroutine(HandleConfirmationButtonsSlide(false));

    }

    private void CancelResetData() {

        // slide confirmation buttons out
        if (confirmationButtonsCoroutine != null) StopCoroutine(confirmationButtonsCoroutine); // stop any existing coroutines
        confirmationButtonsCoroutine = StartCoroutine(HandleConfirmationButtonsSlide(false));

    }

    private void Quit() => Application.Quit();

    public void LoadScene(string sceneName) {

        if (sceneLoadCoroutine != null) StopCoroutine(sceneLoadCoroutine); // stop the previous scene load coroutine if it exists
        sceneLoadCoroutine = StartCoroutine(HandleSceneLoad(sceneName));

    }

    private IEnumerator HandleSceneLoad(string sceneName) {

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
}
