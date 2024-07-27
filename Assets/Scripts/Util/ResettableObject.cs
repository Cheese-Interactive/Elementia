using UnityEngine;

public class ResettableObject : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject objectOutlinePrefab;
    private Rigidbody2D rb;
    private Vector2 startPos;
    private Quaternion startRot;
    private GameObject outlineObject;

    private void Start() {

        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        startRot = transform.rotation;

        outlineObject = Instantiate(objectOutlinePrefab, transform.position, transform.rotation);
        outlineObject.transform.localScale = transform.localScale;

    }

    private void OnDestroy() => Destroy(outlineObject);

    public void ResetObject() {

        transform.position = startPos;
        transform.rotation = startRot;

        // reset velocity if object has a rigidbody
        if (rb)
            rb.velocity = Vector2.zero;

    }
}
