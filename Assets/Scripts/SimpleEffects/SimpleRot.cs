using UnityEngine;

public class SimpleRot : MonoBehaviour {

    [Header("References")]
    private GameManager gameManager;

    [Header("Settings")]
    [SerializeField] private float rotAmount;

    private void Start() => gameManager = FindObjectOfType<GameManager>();

    private void Update() {

        // if the game is paused, don't rotate
        if (gameManager.IsPaused())
            return;

        transform.Rotate(0, 0, rotAmount);

    }
}
