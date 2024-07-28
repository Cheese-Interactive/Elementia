using System;
using System.Collections;
using UnityEngine;

public class SimpleFloatEffect : MonoBehaviour {

    [Header("Customization")]
    [SerializeField] float height;
    [SerializeField] float duration;

    float startY;
    float endY;

    bool rising = true;
    bool readyToGo = true;

    // Start is called before the first frame update
    void Start() {
        if (duration <= 0)
            throw new ArgumentException("duration must be greater than or equal to 0 lil bro");
        startY = transform.position.y;
        endY = startY + height;
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
        float elapsed = 0;
        Vector3 startPosition = transform.position;
        float t = 0;

        while (elapsed < duration) {
            t = elapsed / duration;
            t = t * t * (3 - 2 * t);
            transform.position = Vector3.Lerp(startPosition, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = target;
        readyToGo = true;
        rising = !rising;
    }

}
