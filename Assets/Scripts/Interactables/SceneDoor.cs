using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : Door {

    [Header("Scene")]
    public string sceneName;

    private void Start() {

        if (string.IsNullOrEmpty(sceneName) || SceneManager.GetSceneByName(sceneName) == null) // make sure scene name is valid
            Debug.LogError("Scene name is not valid " + name);

    }

    public override void Interact() {

        base.Interact();
        SceneManager.LoadScene(sceneName); // load target scene

    }
}
