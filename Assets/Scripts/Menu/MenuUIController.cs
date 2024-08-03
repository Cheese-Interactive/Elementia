using MoreMountains.Tools;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour {

    [Header("References")]
    private Animator anim;

    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button resetDataButton;
    [SerializeField] private GameObject confirmationButtons;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string gameSceneName;

    [Header("Coroutines")]
    private Coroutine confirmationButtonsCoroutine;

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

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // wait for menu animations to finish

        // enable buttons
        playButton.interactable = true;
        resetDataButton.interactable = true;
        quitButton.interactable = true;

    }

    private void Play() => MMSceneLoadingManager.LoadScene(gameSceneName); // no game manager exists in this scene, no need to use that method

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

}
