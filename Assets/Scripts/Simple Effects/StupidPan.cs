using System.Collections;
using UnityEngine;

public class StupidPan : MonoBehaviour {

    [Header("Customization")]
    [SerializeField] Vector3 dest;
    [SerializeField] float duration;

    private void Start() {
        StartCoroutine(LerpTo(dest));
    }

    private IEnumerator LerpTo(Vector3 target) {

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

        transform.position = new Vector3(target.x, target.y);

    }
}
