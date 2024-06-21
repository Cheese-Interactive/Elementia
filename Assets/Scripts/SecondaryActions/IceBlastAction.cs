using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IceBlastAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private Tilemap waterTilemap;
    private TilemapManager tilemapManager;

    [Header("Blast")]
    [SerializeField] private ParticleSystem iceBlastParticles;
    [SerializeField] private float blastRadius;
    [SerializeField][Tooltip("Must be >= particle duration")] private float freezeDuration; // in order to prevent overlapping blasts
    [SerializeField] private LayerMask waterMask;

    private new void Start() {

        base.Start();

        tilemapManager = FindObjectOfType<TilemapManager>();

        iceBlastParticles.gameObject.SetActive(false); // ice blast particles are not active by default

        if (iceBlastParticles.main.duration > freezeDuration)
            Debug.LogWarning("Particle duration must be less than or equal to freeze duration"); // in order to prevent overlapping blasts

    }

    public override void OnTrigger() {

        if (!isReady) return;

        iceBlastParticles.gameObject.SetActive(true); // show ice blast particles (disables itself after duration) | to modify duration check particle settings

        // TODO: possibly make the position the wand tip
        Vector3Int centerCell = waterTilemap.WorldToCell(transform.position); // get center cell

        // calculate the bounds in which to check for water tiles
        int cellRadius = Mathf.CeilToInt(blastRadius / waterTilemap.cellSize.x);

        // loop through bounds
        for (int x = -cellRadius; x <= cellRadius; x++) {

            for (int y = -cellRadius; y <= cellRadius; y++) {

                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int tilePos = centerCell + offset;

                if (waterTilemap.GetTile(tilePos) == null) continue; // skip if the tile is null

                Vector3 tileWorldPos = waterTilemap.GetCellCenterWorld(tilePos); // calculate the world position of the tile's center

                if (Vector3.Distance(transform.position, tileWorldPos) <= blastRadius) // check if the tile is within the radius
                    tilemapManager.Freeze(tilePos, freezeDuration); // freeze the water tile

            }
        }

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

    }
}
