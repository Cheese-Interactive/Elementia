using MoreMountains.Feedbacks;
using System.Collections;
using System.Linq;
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

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player particlesFeedback;

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

        //play particles and start reset
        getFeedbackParticles(particlesFeedback).Stop(); //failsafe
        ResetParticlesTransform(getFeedbackParticles(particlesFeedback), gameObject); //visual effect for particles, makes them follow the object
        particlesFeedback.PlayFeedbacks(); //play particles


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

            //visual effect for particles, makes them no longer follow the object
            ResetParticlesTransform(getFeedbackParticles(particlesFeedback), FindObjectOfType<Grid>().gameObject); //using the grid is hacky... but hey what can you doooooooooooo... im a touchy feely foooohhhhh... i would do anything to not give a $&@! about youuuuuu... life is pretty cruellll..... for a touchy feely fooohhhh.... i would do anything to nto give a shit but i doooo
            //particleFeedbackShape.position = transform.position;

            // reset object position and rotation
            transform.position = startPos;
            transform.rotation = startRot;

            // reset velocity if object has a rigidbody
            if (rb)
                rb.velocity = Vector2.zero;

        }

        // stop particles
        getFeedbackParticles(particlesFeedback).Stop();

        isResetting = false;

    }

    private void OnDrawGizmos() {

        Gizmos.color = lowestCornerVisualizerColor;
        Gizmos.DrawWireSphere(Utilities.GetLowestCorner(spriteRenderer, cornerLeniency), lowestCornerVisualizerRadius);

    }

    public bool IsResettable() => isResettable;

    private ParticleSystem getFeedbackParticles(MMF_Player feedback) => feedback.FeedbacksList.OfType<MMF_Particles>().FirstOrDefault().BoundParticleSystem;

    private void ResetParticlesTransform(ParticleSystem particles, GameObject transformParent) {
        ParticleSystem.MainModule particlesMain = particles.main;
        //particlesMain.simulationSpace = simSpace;
        particles.gameObject.transform.parent = transformParent.transform;
        particles.gameObject.transform.position = gameObject.transform.position;
    }
}
