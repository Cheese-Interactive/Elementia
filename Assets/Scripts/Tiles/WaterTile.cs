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

    [Header("Freeze")]
    private Coroutine freezeResetCoroutine;

    public void Initialize(Tilemap waterTilemap, Vector3Int tilePos, TileBase waterTile) {

        this.waterTilemap = waterTilemap;
        this.tilePos = tilePos;
        this.waterTile = waterTile;

    }

    public void Freeze(float freezeDuration) {

        waterTilemap.SetTile(tilePos, null);

        if (freezeResetCoroutine != null) StopCoroutine(freezeResetCoroutine); // stop coroutine if it's already running (reset freeze timer)

        freezeResetCoroutine = StartCoroutine(ResetFreeze(freezeDuration)); // start reset timer

    }

    private IEnumerator ResetFreeze(float freezeDuration) {

        yield return new WaitForSeconds(freezeDuration);
        waterTilemap.SetTile(tilePos, waterTile);

        freezeResetCoroutine = null;

    }
}
