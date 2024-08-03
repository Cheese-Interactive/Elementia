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

    public override void TryInteract() {

        if (isInteracted) {

            isInteracted = true;
            gameManager.LoadScene(sceneName); // load scene if door is already open
            return;

        }

        if (gameManager.TryRemoveKey()) { // remove key if key is available

            isInteracted = true;
            gameManager.LoadScene(sceneName);
            return;

        }
    }
}
