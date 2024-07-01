using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour {

    new private Camera camera;
    private float zPos;
    [Header("Zones")]
    [SerializeField] private CameraZone[] zones;
    [SerializeField] private CameraZone startZone;
    [Header("Customization")]
    [SerializeField] private float shiftTime;
    [SerializeField] private bool isPosStatic;

    void Start() {
        camera = GetComponent<Camera>();
        zPos = transform.position.z;
        SetCam(startZone);
        if (!FindObjectOfType<CharacterHorizontalMovement>()!.GetComponent<CameraZoneHelper>())
            print("ERROR: Player needs a CameraZoneHelper!!!!!!");
    }

    void Update() {
    }

    public void SetCam(Vector3 pos, float size) {
        if (!isPosStatic)
            transform.position = new Vector3(pos.x, pos.y, zPos);
        camera.orthographicSize = size;
    }

    public void SetCam(CameraZone state) {
        SetCam(state.getPos(), state.getSize());
    }

    public IEnumerator ChangeCamState(Vector3 pos, float size) {
        pos = new Vector3(pos.x, pos.y, zPos);
        float elapsed = 0;
        float t = 0; //smoothing formula
        float startSize = camera.orthographicSize;
        Vector3 startPos = transform.position;
        float dist = Vector2.Distance(transform.position, pos);

        while (elapsed < shiftTime) {
            t = elapsed / shiftTime;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            camera.orthographicSize = Mathf.Lerp(startSize, size, t);
            if (!isPosStatic)
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
