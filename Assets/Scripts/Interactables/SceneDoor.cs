using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : Interactable {

    [Header("References")]
    private GameManager gameManager;

    [Header("Scene")]
    public string sceneName;

    private void Start() {

        gameManager = FindObjectOfType<GameManager>();

        if (string.IsNullOrEmpty(sceneName) || SceneManager.GetSceneByName(sceneName) == null) // make sure scene name is valid
            Debug.LogError("Scene name is not valid " + name);

    }

    public override void Interact() {

        if (isInteracted || gameManager.HasKey()) { // make sure door is already open or key can be removed

            isInteracted = true;
            gameManager.RemoveKey(); // remove key
            gameManager.LoadScene(sceneName); // load scene

        }
    }

    public override bool IsInteractable() => !isInteracted && gameManager.HasKey(); // door is interactable if it hasn't been interacted with and player has key

}
