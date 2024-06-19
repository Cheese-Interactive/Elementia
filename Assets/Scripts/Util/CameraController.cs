using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour {
    //explanation:
    //camera is mostly stationary
    //collider triggers define different "zones" 
    //when player enters different zones the camera shifts to the state associated with that zone
    //meaning it changes position and fov (or really anything else)
    //this helps keep the open feel of the map while also splitting it into distinct sections

    new private Camera camera;
    private float zPos;
    [Header("Zones")]
    [SerializeField] private CameraZone[] zones;
    [Header("Customization")]
    [SerializeField] private float shiftTime;

    void Start() {
        camera = GetComponent<Camera>();
        zPos = transform.position.z;
    }

    void Update() {
        //transform.position = new Vector3(transform.position.x, transform.position.y, zPos);
    }

    public void SetCam(Vector3 pos, float size) {
        transform.position = pos;
        camera.orthographicSize = size;
    }

    public void SetCam(CameraZone state) {
        SetCam(state.getPos(), state.getSize());
    }

    public IEnumerator ChangeCamState(Vector3 pos, float size) {
        float elapsed = 0;
        float t = 0; //smoothing formula
        float startSize = camera.orthographicSize;
        Vector3 startPos = transform.position;
        while (elapsed < shiftTime) {
            t = elapsed / shiftTime;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            camera.orthographicSize = Mathf.Lerp(startSize, size, t);
            transform.position = Vector3.Lerp(startPos, pos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetCam(pos, size);
        yield return null;
    }

    public void ChangeCamState(CameraZone state) {
        StartCoroutine(ChangeCamState(state.getPos(), state.getSize()));
    }
}
