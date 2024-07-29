using System;
using System.Collections;
using UnityEngine;

public class SimpleSway : MonoBehaviour {

    [Header("Customization")]
    [SerializeField] float degreeOffset;
    [SerializeField] float time;

    float startRot;
    bool goingRight = true; //imagine an arrow placed on top of the object pointing up
    bool readyToGo = true;

    // Start is called before the first frame update
    void Start() {
        if (time < 0)
            throw new ArgumentException("time must be greater than or equal to 0 lil bro");
        startRot = transform.rotation.eulerAngles.z;
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, transform.rotation.y, startRot + degreeOffset));
    }

    // Update is called once per frame
    void Update() {
        if (readyToGo) {
            if (goingRight)
                StartCoroutine(EaseLerpTo(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,
                                                    startRot + degreeOffset)));
            else
                StartCoroutine(EaseLerpTo(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,
                                                    startRot - degreeOffset)));
            goingRight = !goingRight;
        }
    }


    private IEnumerator EaseLerpTo(Vector3 targetRot) {
        readyToGo = false;
        float elapsed = 0;
        Vector3 startRotEuler = transform.rotation.eulerAngles;
        Quaternion targetQ = Quaternion.Euler(targetRot);
        Quaternion startQ = Quaternion.Euler(startRotEuler);
        float t = 0; //smoothening formula

        while (elapsed < time) {
            t = elapsed / time;
            t = t * t * (3 - 2 * t);
            transform.rotation = Quaternion.Lerp(startQ, targetQ, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetQ;
        readyToGo = true;
    }

}
