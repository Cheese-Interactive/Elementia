using MoreMountains.CorgiEngine;
using UnityEngine;

public class CamFovZone : MonoBehaviour {

    private GameObject player;
    private CameraController cam;

    [Header("Customization")]
    [SerializeField] private float minFov;
    [SerializeField] private float maxFov;

    void Start() {
        player = FindObjectOfType<PlayerController>().gameObject;
        cam = FindObjectOfType<CameraController>();
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.Equals(player)) {
            cam.MinimumZoom = minFov;
            cam.MaximumZoom = maxFov;

        }
    }

}
