using UnityEngine;

public class CameraZone : MonoBehaviour {
    [SerializeField][Range(1, 15)] private float camSize;
    [SerializeField] private Vector3 camPos;

    void Start() {
        if (!GetComponent<Collider2D>())
            print("ERROR: CAMERA ZONE " + gameObject.name + " IS MISSING A TRIGGER!");
    }

    public float getSize() { return camSize; }
    public Vector3 getPos() { return camPos; }

    private void OnDrawGizmosSelected() {
        //Camera preview
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(camPos, new Vector3(camSize * 4f, camSize * 2f));
    }
}
