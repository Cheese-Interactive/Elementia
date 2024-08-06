using MoreMountains.Feedbacks;
using System.Linq;
using UnityEngine;

public class IceSecondaryAction : SecondaryAction {

    [Header("References")]
    private TilemapManager tilemapManager;

    [Header("Blast")]
    [SerializeField] private int blastRadius;
    [SerializeField][Tooltip("Must be >= particle duration")] private float slowDuration; // in order to prevent overlapping blasts
    [SerializeField] private LayerMask waterMask;

    [Header("Feedback")]
    [SerializeField] private MMF_Player onUseFeedback;

    [Header("Debug")]
    [SerializeField] private Color iceBlastVisualizerColor;

    private new void Awake() {

        base.Awake();

        // overlapping blast prevention
        // this is so weird bruh

        //get the particle feedback
        //get the associated particle system
        //check the liftime of the particles (aka the duration of the effect)
        //check if its lower than the cooldown (to prevent overlapping blasts)
        if (onUseFeedback.FeedbacksList.OfType<MMF_Particles>().FirstOrDefault().BoundParticleSystem.main.startLifetime.constantMax < cooldown)
            Debug.LogWarning("Particle duration must be less than or equal to slow duration.");
    }

    private void Start() => tilemapManager = FindObjectOfType<TilemapManager>();

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        // TODO: possibly make the position the wand tip
        Vector3Int centerCell = tilemapManager.WaterWorldToCell(transform.position); // get center cell

        // loop through bounds
        for (int x = -blastRadius * 2; x <= blastRadius * 2; x++) {

            for (int y = -blastRadius * 2; y <= blastRadius * 2; y++) {

                Vector3Int tilePos = centerCell + new Vector3Int(x, y, 0);
                tilemapManager.Freeze(transform.position, tilePos, slowDuration, blastRadius); // freeze water tile (radius is checked in this method)

            }
        }

        onUseFeedback.PlayFeedbacks(); // play use sound
        StartCooldown(); // start cooldown

    }

    private void OnDrawGizmosSelected() {

        Gizmos.color = iceBlastVisualizerColor;
        Gizmos.DrawSphere(transform.position, blastRadius);

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
