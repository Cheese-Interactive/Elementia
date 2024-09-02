using System.Collections;
using UnityEngine;

public class EndZone : MonoBehaviour {

    [Header("References")]
    private GameManager gameManager;

    private void Start() => gameManager = FindObjectOfType<GameManager>();

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.CompareTag("Player") && gameManager.IsLevelComplete())
            StartTimer();

    }

    private void StartTimer() => StartCoroutine(EndLevel());

    private IEnumerator EndLevel() {

        yield return new WaitForSeconds(gameManager.GetLevelCompleteDelay());
        gameManager.ShowLevelCompleteScreen();

    }
}
