using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : Interactable {

    [Header("References")]
    private GameManager gameManager;

    [Header("Scene")]
    [SerializeField][Tooltip("True if there is no target scene/door should have no functionality")] private bool visualDoor;
    [SerializeField] private string sceneName;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();

        if (!visualDoor && (string.IsNullOrEmpty(sceneName) || SceneManager.GetSceneByName(sceneName) == null)) // make sure door isn't visual and scene name is valid
            Debug.LogError("Scene name \"" + sceneName + "\" is not valid.");

    }

    public override void Interact() {

        if (visualDoor) return; // return if door is visual because it shouldn't be interactable

        if (isInteracted || gameManager.HasKey()) { // make sure door is already open or key can be removed

            isInteracted = true;
            gameManager.RemoveKey(); // remove key
            gameManager.LoadScene(sceneName); // load scene

        }
    }

    public override bool IsInteractable() => !isInteracted && gameManager.HasKey(); // door is interactable if it hasn't been interacted with and player has key

}
