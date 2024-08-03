using UnityEngine;

public class Door : Interactable {

    [Header("References")]
    protected GameManager gameManager;

    protected void Start() => gameManager = FindObjectOfType<GameManager>();

    public override bool TryInteract() {

        bool keyRemoved = gameManager.TryRemoveKey();

        // TODO: add door opening animation here
        if (keyRemoved)
            Debug.Log("Key removed.");

        return keyRemoved;

    }
}
