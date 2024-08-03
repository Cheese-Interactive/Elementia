using System.Collections;
using UnityEngine;

public class SimpleFloat : MonoBehaviour {

    [Header("Settings")]
    [SerializeField] float height;
    [SerializeField] float duration;
    private float startY;
    private float endY;
    private bool rising;
    private bool readyToGo;

    // Start is called before the first frame update
    void Start() {

        if (duration < 0)
            Debug.LogError("Duration must be greater than or equal to 0.");

        startY = transform.position.y;
        endY = startY + height;

        rising = true;
        readyToGo = true;

    }

    // Update is called once per frame
    void Update() {

        if (rising && readyToGo)
            StartCoroutine(LerpTo(new Vector3(transform.position.x, endY)));
        else if (!rising && readyToGo)
            StartCoroutine(LerpTo(new Vector3(transform.position.x, startY)));

    }


    private IEnumerator LerpTo(Vector3 target) {

        readyToGo = false;
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        float time;

        while (elapsed < duration) {

            time = elapsed / duration;
            time = time * time * (3 - 2 * time);
            transform.position = Vector3.Lerp(startPosition, target, time);
            elapsed += Time.deltaTime;
            yield return null;

        }

        transform.position = target;
        readyToGo = true;
        rising = !rising;

    }
}
