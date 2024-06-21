using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform waterTilesParent;
    [SerializeField] private Tilemap waterTilemap;
    [SerializeField] private TileBase waterTileTop;
    [SerializeField] private TileBase waterTileBottom;
    [SerializeField] private TileBase iceTileTop;
    [SerializeField] private TileBase iceTileBottom;

    [Header("Freeze")]
    private Dictionary<Vector3Int, WaterTile> frozen;

    private void Start() {

        frozen = new Dictionary<Vector3Int, WaterTile>();

        for (int x = waterTilemap.cellBounds.min.x; x < waterTilemap.cellBounds.max.x; x++) {

            for (int y = waterTilemap.cellBounds.min.y; y < waterTilemap.cellBounds.max.y; y++) {

                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase tile = waterTilemap.GetTile(tilePos);

                if (tile != null) {

                    bool isTopTile = waterTilemap.GetTile(new Vector3Int(x, y + 1, 0)) == null;

                    WaterTile waterTile = Instantiate(new GameObject(), waterTilesParent).AddComponent<WaterTile>();
                    waterTile.Initialize(waterTilemap, tilePos, isTopTile ? waterTileTop : waterTileBottom, isTopTile ? iceTileTop : iceTileBottom);
                    frozen.Add(tilePos, waterTile);

                }
            }
        }
    }

    public void Freeze(Vector3 origin, Vector3Int tilePos, float freezeDuration, float radius) {

        if (waterTilemap.GetTile(tilePos) == null) return; // skip if the tile is null

        Vector3 tileWorldPos = waterTilemap.GetCellCenterWorld(tilePos); // calculate the world position of the tile's center

        if (Vector3.Distance(origin, tileWorldPos) <= radius && frozen.ContainsKey(tilePos)) // check if the tile is within the radius and tile exists in the dictionary
            frozen[tilePos].Freeze(freezeDuration);

    }

    public Vector3Int WorldToCell(Vector3 worldPos) => waterTilemap.WorldToCell(worldPos);

}
