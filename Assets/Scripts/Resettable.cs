using UnityEngine;

public class Resettable : MonoBehaviour {
    //temp

    private Vector2 pos;
    private Rigidbody2D rb;
    void Start() {
        pos = transform.position;
        if (GetComponent<Rigidbody2D>())
            rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            transform.position = pos;
            if (rb) {
                rb.velocity = Vector2.zero;
                rb.SetRotation(0);
            }

        }

    }
}
