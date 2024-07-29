using System.Collections;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject objectOutlinePrefab;
    [SerializeField] private ResetBeacon resetBeaconPrefab;
    private Rigidbody2D rb;
    private ResetBeacon currResetBeacon;
    private Coroutine resetCoroutine;
    private GameObject outlineObject;
    private Vector2 startPos;
    private Quaternion startRot;
    private bool isResetting;

    [Header("Settings")]
    [SerializeField][Range(0f, 100f)] private float beaconFadeInDurationPercentage;
    [SerializeField][Range(0f, 100f)] private float beaconFadeOutDurationPercentage;
    [SerializeField] private float beaconStartWidth;
    [SerializeField] private float beaconEndWidth;
    [SerializeField] private float beaconHeight;
    [SerializeField][Tooltip("Should be high enough to cover cases where a corner is slightly higher than another and doesn't skew the beacon position")] private float cornerLeniency;
    [SerializeField] private bool isResettable;
    private float fadeInDuration;
    private float fadeOutDuration;
    private float resetDuration;

    [Header("Debug")]
    [SerializeField] private Color lowestCornerVisualizerColor;
    [SerializeField] private float lowestCornerVisualizerRadius;

    protected void Start() {

        rb = GetComponent<Rigidbody2D>();

        startPos = transform.position;
        startRot = transform.rotation;

        outlineObject = Instantiate(objectOutlinePrefab, transform.position, transform.rotation);
        outlineObject.transform.localScale = transform.localScale;

    }

    private void OnDestroy() => Destroy(outlineObject);

    public void StartReset(float resetDuration) {

        if (!isResettable || isResetting) return; // make sure object is resettable and is not already resetting

        isResetting = true;
        this.resetDuration = resetDuration;

        // make sure fade in and fade out durations do not exceed field duration
        if (beaconFadeInDurationPercentage + beaconFadeOutDurationPercentage > 100)
            Debug.LogError("Reset fade in and fade out durations exceed reset duration.");

        fadeInDuration = (beaconFadeInDurationPercentage / 100f) * resetDuration;
        fadeOutDuration = (beaconFadeOutDurationPercentage / 100f) * resetDuration;

        // instantiate reset beacon and start reset
        currResetBeacon = Instantiate(resetBeaconPrefab, transform.position, Quaternion.identity);
        currResetBeacon.StartReset(transform, spriteRenderer, fadeInDuration, beaconStartWidth, beaconEndWidth, beaconHeight, cornerLeniency);

        resetCoroutine = StartCoroutine(HandleReset());

    }

    private IEnumerator HandleReset() {

        yield return new WaitForSeconds(resetDuration); // wait for reset duration
        StopReset(true); // stop resetting as it has completed
        resetCoroutine = null;

    }

    public void CancelReset() {

        if (!isResetting) return;
        StopReset(false);

    }

    private void StopReset(bool resetCompleted) {

        if (resetCoroutine != null) StopCoroutine(resetCoroutine); // stop reset coroutine if it is running
        resetCoroutine = null;

        if (resetCompleted) {

            // reset object position and rotation
            transform.position = startPos;
            transform.rotation = startRot;

            // reset velocity if object has a rigidbody
            if (rb)
                rb.velocity = Vector2.zero;

        }

        // stop reset beacon and reset current reset beacon value
        currResetBeacon.StopReset(fadeOutDuration);
        currResetBeacon = null;

        isResetting = false;

    }

    private void OnDrawGizmos() {

        Gizmos.color = lowestCornerVisualizerColor;
        Gizmos.DrawWireSphere(Utilities.GetLowestCorner(spriteRenderer, cornerLeniency), lowestCornerVisualizerRadius);

    }

    public bool IsResettable() => isResettable;

}
