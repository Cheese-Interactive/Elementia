using MoreMountains.CorgiEngine;
using UnityEngine;

public class CameraFOVZone : MonoBehaviour {

    [Header("References")]
    private GameObject player;
    private CameraController cam;

    [Header("Customization")]
    [SerializeField] private float minFov;
    [SerializeField] private float maxFov;

    private void Start() {

        player = FindObjectOfType<PlayerController>().gameObject;
        cam = FindObjectOfType<CameraController>();

    }

    private void OnTriggerEnter2D(Collider2D col) {

        if (col.gameObject.Equals(player)) {

            cam.MinimumZoom = minFov;
            cam.MaximumZoom = maxFov;

        }
    }
}
