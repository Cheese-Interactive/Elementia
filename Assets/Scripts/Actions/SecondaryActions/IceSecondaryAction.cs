using UnityEngine;

public class IceSecondaryAction : SecondaryAction {

    [Header("References")]
    private TilemapManager tilemapManager;

    [Header("Blast")]
    [SerializeField] private ParticleSystem iceBlastParticles;
    [SerializeField] private int blastRadius;
    [SerializeField][Tooltip("Must be >= particle duration")] private float slowDuration; // in order to prevent overlapping blasts
    [SerializeField] private LayerMask waterMask;

    private new void Awake() {

        base.Awake();

        iceBlastParticles.gameObject.SetActive(false); // ice blast particles are not active by default (done in awake so it runs when game starts)

        if (iceBlastParticles.main.duration > slowDuration)
            Debug.LogWarning("Particle duration must be less than or equal to slow duration."); // in order to prevent overlapping blasts

    }

    private void Start() => tilemapManager = FindObjectOfType<TilemapManager>();

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        iceBlastParticles.gameObject.SetActive(false); // set to false to make sure particle activates on awake
        iceBlastParticles.gameObject.SetActive(true); // show ice blast particles (disables itself after duration) | to modify duration check particle settings
        iceBlastParticles.transform.localScale = new Vector2(blastRadius, blastRadius); // scale particles to match blast radius

        // TODO: possibly make the position the wand tip
        Vector3Int centerCell = tilemapManager.WaterWorldToCell(transform.position); // get center cell

        // loop through bounds
        for (int x = -blastRadius; x <= blastRadius; x++) {

            for (int y = -blastRadius; y <= blastRadius; y++) {

                Vector3Int tilePos = centerCell + new Vector3Int(x, y, 0);
                tilemapManager.Freeze(transform.position, tilePos, slowDuration, blastRadius); // freeze water tile

            }
        }

        cooldownTimer = cooldown; // restart cooldown timer
        weaponSelector.SetSecondaryCooldownValue(GetNormalizedCooldown(), cooldownTimer); // update secondary cooldown meter

    }

    private void OnDrawGizmosSelected() {

        Gizmos.color = new Color(100f / 255f, 180f / 255f, 220f / 255f, 0.3f);
        Gizmos.DrawSphere(transform.position, blastRadius);

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
