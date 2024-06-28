using UnityEngine;

public class CameraZoneHelper : MonoBehaviour {
    CameraZone currentZone = null;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<CameraZone>()) {
            CameraZone z = other.gameObject.GetComponent<CameraZone>();
            if (currentZone != z) {
                currentZone = z;
                FindObjectOfType<CameraController>().ChangeCamState(z);
            }
        }

    }
}
