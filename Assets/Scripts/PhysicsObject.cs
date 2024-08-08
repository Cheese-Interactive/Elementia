using MoreMountains.Feedbacks;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject objectOutlinePrefab;
    private Rigidbody2D rb;
    private Coroutine resetCoroutine;
    private GameObject outlineObject;
    private Vector2 startPos;
    private Quaternion startRot;
    private bool isResetting;

    [Header("Settings")]
    [SerializeField] private float cornerLeniency;
    [SerializeField] private bool isResettable;
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

        // play particles and start reset
        GetFeedbackParticles(particlesFeedback).Stop(); // stop particles if they are already playing
        ResetParticlesTransform(GetFeedbackParticles(particlesFeedback), gameObject); // visual effect for particles, makes them follow the object
        particlesFeedback.PlayFeedbacks(); // play feedbacks


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

            ResetParticlesTransform(GetFeedbackParticles(particlesFeedback), FindObjectOfType<Grid>().gameObject); // visual effect for particles, makes them no longer follow the object
            //particleFeedbackShape.position = transform.position;

            // reset object position and rotation
            transform.position = startPos;
            transform.rotation = startRot;

            // reset velocity if object has a rigidbody
            if (rb)
                rb.velocity = Vector2.zero;

        }

        // stop particles
        GetFeedbackParticles(particlesFeedback).Stop();

        isResetting = false;

    }

    private void OnDrawGizmos() {

        Gizmos.color = lowestCornerVisualizerColor;
        Gizmos.DrawWireSphere(Utilities.GetLowestCorner(spriteRenderer, cornerLeniency), lowestCornerVisualizerRadius);

    }

    public bool IsResettable() => isResettable;

    private void ResetParticlesTransform(ParticleSystem particles, GameObject transformParent) {

        //ParticleSystem.MainModule main = particles.main;
        //particlesMain.simulationSpace = simSpace;
        particles.gameObject.transform.parent = transformParent.transform;
        particles.gameObject.transform.position = gameObject.transform.position;

    }

    private ParticleSystem GetFeedbackParticles(MMF_Player feedback) => feedback.FeedbacksList.OfType<MMF_Particles>().FirstOrDefault().BoundParticleSystem;

}
