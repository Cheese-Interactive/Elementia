using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : Door {

    [Header("Scene")]
    public string sceneName;

    protected new void Start() {

        base.Start();

        if (string.IsNullOrEmpty(sceneName) || SceneManager.GetSceneByName(sceneName) == null) // make sure scene name is valid
            Debug.LogError("Scene name is not valid " + name);

    }

    public override bool TryInteract() {

        if (!base.TryInteract()) return false; // return if door cannot be opened

        gameManager.LoadScene(sceneName);
        return true;

    }
}
