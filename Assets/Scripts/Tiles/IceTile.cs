using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IceTile : MonoBehaviour {

    [Header("References")]
    private Tilemap iceTilemap;

    [Header("Data")]
    private Vector3Int tilePos;
    private TileBase iceTile;

    [Header("Freeze")]
    private Coroutine freezeResetCoroutine;

    public void Initialize(Tilemap iceTilemap, Vector3Int tilePos, TileBase iceTile) {

        this.iceTilemap = iceTilemap;
        this.tilePos = tilePos;
        this.iceTile = iceTile;

    }

    public void Freeze(float freezeDuration) {

        iceTilemap.SetTile(tilePos, iceTile);

        if (freezeResetCoroutine != null) StopCoroutine(freezeResetCoroutine); // stop coroutine if it's already running (reset freeze timer)

        freezeResetCoroutine = StartCoroutine(ResetFreeze(freezeDuration)); // start reset timer

    }

    private IEnumerator ResetFreeze(float freezeDuration) {

        yield return new WaitForSeconds(freezeDuration);
        iceTilemap.SetTile(tilePos, null);

        freezeResetCoroutine = null;

    }
}
