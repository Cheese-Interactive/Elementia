using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IceBlastAction : SecondaryAction {

    [Header("References")]
    private TilemapManager tilemapManager;

    [Header("Blast")]
    [SerializeField] private ParticleSystem iceBlastParticles;
    [SerializeField] private int blastRadius;
    [SerializeField][Tooltip("Must be >= particle duration")] private float freezeDuration; // in order to prevent overlapping blasts
    [SerializeField] private LayerMask waterMask;

    private new void Awake() {

        base.Awake();

        iceBlastParticles.gameObject.SetActive(false); // ice blast particles are not active by default (done in awake so it runs when game starts)

        if (iceBlastParticles.main.duration > freezeDuration)
            Debug.LogWarning("Particle duration must be less than or equal to freeze duration"); // in order to prevent overlapping blasts

    }

    private new void Start() {

        base.Start();

        tilemapManager = FindObjectOfType<TilemapManager>();

    }

    public override void OnTrigger() {

        if (!isReady) return;

        iceBlastParticles.gameObject.SetActive(false); // set to false to make sure particle activates on awake
        iceBlastParticles.gameObject.SetActive(true); // show ice blast particles (disables itself after duration) | to modify duration check particle settings
        iceBlastParticles.transform.localScale = new Vector2(blastRadius, blastRadius); // scale particles to match blast radius

        // TODO: possibly make the position the wand tip
        Vector3Int centerCell = tilemapManager.WorldToCell(transform.position); // get center cell

        // loop through bounds
        for (int x = -blastRadius; x <= blastRadius; x++) {

            for (int y = -blastRadius; y <= blastRadius; y++) {

                Vector3Int tilePos = centerCell + new Vector3Int(x, y, 0);
                tilemapManager.Freeze(transform.position, tilePos, freezeDuration, blastRadius); // freeze water tile

            }
        }

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

    }

    private void OnDrawGizmosSelected() {

        Gizmos.color = new Color(100f / 255f, 180f / 255f, 220f / 255f, 0.3f);
        Gizmos.DrawSphere(transform.position, blastRadius);

    }
}
