using UnityEngine;

public class CameraZoneHelper : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<CameraZone>())
            FindObjectOfType<CameraController>().ChangeCamState(other.gameObject.GetComponent<CameraZone>());
    }
}
