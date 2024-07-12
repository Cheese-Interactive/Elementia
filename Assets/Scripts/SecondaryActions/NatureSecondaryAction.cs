using UnityEngine;
using UnityEngine.Tilemaps;

public class NatureSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private GameObject vinePrefab;
    private TilemapManager tilemapManager;

    [Header("Settings")]
    [SerializeField] private float gridTileHeight;
    [SerializeField] private float beanstalkHeight;

    private void Start() => tilemapManager = FindObjectOfType<TilemapManager>();

    public override void OnTriggerRegular() {

        if (!isReady) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        Vector3Int floorTileCellPos = tilemapManager.MainWorldToCell(playerController.GetBottomPosition() + (playerController.GetDirectionRight() * gridTileHeight) - new Vector2(0f, gridTileHeight / 2f));
        TileBase floorTile = tilemapManager.GetMainTileAt(floorTileCellPos); // get tile below and one tile in the direction the player is facing

        if (floorTile == null) return; // make sure player is on a valid tile

        Vector3 floorTileWorldPos = tilemapManager.MainCellToWorld(floorTileCellPos) + new Vector3(gridTileHeight / 2f, gridTileHeight / 2f, 0f); // get world position of the tile's center
        TileBase tile;

        for (float y = gridTileHeight; y < beanstalkHeight; y += gridTileHeight) {

            Vector3 tileWorldPos = floorTileWorldPos + new Vector3(0f, y, 0f);
            tile = tilemapManager.GetMainTileAt(tilemapManager.MainWorldToCell(tileWorldPos));

            if (tile != null) break; // stop stacking vines if tile is in the way

            Instantiate(vinePrefab, tileWorldPos, Quaternion.identity); // spawn vine

        }

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(secondaryCooldown); // create new meter for cooldown

    }

    public override bool IsRegularAction() => true;

}
