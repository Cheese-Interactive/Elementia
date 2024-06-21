using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterTile : MonoBehaviour {

    [Header("References")]
    private Tilemap waterTilemap;

    [Header("Data")]
    private Vector3Int tilePos;
    private TileBase waterTile;
    private TileBase iceTile;

    [Header("Freeze")]
    private bool isFrozen;
    private Coroutine freezeCoroutine;

    public void Initialize(Tilemap waterTilemap, Vector3Int tilePos, TileBase waterTile, TileBase iceTile) {

        this.waterTilemap = waterTilemap;
        this.tilePos = tilePos;
        this.waterTile = waterTile;
        this.iceTile = iceTile;

    }

    public void Freeze(float freezeDuration) {

        isFrozen = true;
        waterTilemap.SetTile(tilePos, iceTile);

        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine); // stop coroutine if it's already running (reset freeze timer)

        freezeCoroutine = StartCoroutine(ResetWaterTile(freezeDuration)); // start reset timer

    }

    private IEnumerator ResetWaterTile(float freezeDuration) {

        yield return new WaitForSeconds(freezeDuration);
        waterTilemap.SetTile(tilePos, waterTile);
        isFrozen = false;

    }

    public bool IsFrozen() => isFrozen;

}
