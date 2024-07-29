using DG.Tweening;
using UnityEngine;

public class ResetBeacon : MonoBehaviour {

    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private Transform targetObject;
    private LineRenderer lineRenderer;
    private Tweener beaconTweener;

    [Header("Settings")]
    private float beaconHeight;
    private float cornerLeniency;

    private void Update() {

        transform.position = targetObject.position; // follow target object
        Vector2 lowestCorner = Utilities.GetLowestCorner(spriteRenderer, cornerLeniency);
        lineRenderer.SetPositions(new Vector3[] { lowestCorner, lowestCorner + (Vector2) (transform.up * beaconHeight) }); // update reset beacon positions

    }

    public void StartReset(Transform targetObject, SpriteRenderer spriteRenderer, float fadeInDuration, float beaconStartWidth, float beaconEndWidth, float beaconHeight, float cornerLeniency) {

        lineRenderer = GetComponent<LineRenderer>();

        this.targetObject = targetObject;
        this.spriteRenderer = spriteRenderer;
        this.beaconHeight = beaconHeight;
        this.cornerLeniency = cornerLeniency;

        // set reset beacon settings
        lineRenderer.startWidth = beaconStartWidth;
        lineRenderer.endWidth = beaconEndWidth;
        Vector2 lowestCorner = Utilities.GetLowestCorner(spriteRenderer, cornerLeniency);
        lineRenderer.SetPositions(new Vector3[] { lowestCorner, lowestCorner + (Vector2) (transform.up * beaconHeight) });

        // fade reset beacon in
        Color color = lineRenderer.material.GetColor("_Color");
        beaconTweener = DOVirtual.Float(0f, 1f, fadeInDuration, (float alpha) => lineRenderer.material.SetColor("_Color", new Color(color.r, color.g, color.b, alpha))).SetEase(Ease.InExpo);

    }

    // usable for canceling reset as well because it just fades out the beacon, which happens either way
    public void StopReset(float fadeOutDuration) {

        if (beaconTweener != null && beaconTweener.IsActive()) beaconTweener.Kill(); // stop reset beacon tweener if it is running

        // fade reset beacon out
        Color color = lineRenderer.material.GetColor("_Color");
        beaconTweener = DOVirtual.Float(1f, 0f, fadeOutDuration, (float alpha) => lineRenderer.material.SetColor("_Color", new Color(color.r, color.g, color.b, alpha))).SetEase(Ease.OutCubic).OnComplete(() => Destroy(gameObject)); // fade out and hide reset beacon after fade out

    }
}
