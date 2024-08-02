using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : Door {

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

        base.Interact();
        gameManager.LoadScene(sceneName);

    }
}
