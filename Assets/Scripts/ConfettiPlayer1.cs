using System.Collections;
using UnityEngine;

public class ConfettiPlayer : MonoBehaviour {
    [Header("Confetti Prefabs")]
    [SerializeField] private MainMenuConfettiBlast[] bursts;

    [Header("Customization")]
    [SerializeField] private float startDelay;
    [SerializeField] private float minWait;
    [SerializeField] private float maxWait;
    [SerializeField] private float xBound;
    [SerializeField] private float yBound;

    private bool startDelayPlayed = false;

    float currentWait;

    int nextBurstIdx;

    private void Start() {
        StartCoroutine(StartWait()); //start delay
    }

    void Update() {
        if (startDelayPlayed) {
            if (currentWait > 0)
                currentWait -= Time.deltaTime;
            else {
                currentWait = Random.Range(minWait, maxWait); //randomize the time for the next one

                Vector2 targetPos = GetRandomPos(); //get randomized pos to spawn confetti
                Instantiate(bursts[nextBurstIdx].gameObject, targetPos, Quaternion.identity); //create (play) the confetti

                nextBurstIdx++; //get next confetti to play (instead of randomizing it)
                if (nextBurstIdx >= bursts.Length)
                    nextBurstIdx = 0;
            }
        }
    }

    private IEnumerator StartWait() { //delay before the first burst
        yield return new WaitForSeconds(startDelay);
        startDelayPlayed = true;
    }

    private Vector3 GetRandomPos() {
        float x = Random.Range(-xBound, xBound);
        float y = Random.Range(-yBound, yBound);
        return new Vector3(x, y);
    }

    void OnDrawGizmos() {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(xBound * 2, yBound * 2, 0));
    }
}
