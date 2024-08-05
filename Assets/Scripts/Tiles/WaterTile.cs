using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterTile : MonoBehaviour {

    [Header("References")]
    private TileBase icePlaceholderTile;
    private Tilemap waterTilemap;

    [Header("Data")]
    private Vector3Int tilePos;
    private TileBase waterTile;

    [Header("Freeze")]
    private Coroutine freezeResetCoroutine;

    public void Initialize(Tilemap waterTilemap, Vector3Int tilePos, TileBase waterTile, TileBase icePlaceholderTile) {

        this.waterTilemap = waterTilemap;
        this.tilePos = tilePos;
        this.waterTile = waterTile;
        this.icePlaceholderTile = icePlaceholderTile;

    }

    public void Freeze(float freezeDuration) {

        waterTilemap.SetTile(tilePos, icePlaceholderTile); // use placeholder tile so tile isn't null at that position, so we know it is frozen rather than doesn't exist

        if (freezeResetCoroutine != null) StopCoroutine(freezeResetCoroutine); // stop coroutine if it's already running (reset freeze timer)

        freezeResetCoroutine = StartCoroutine(ResetFreeze(freezeDuration)); // start reset timer

    }

    private IEnumerator ResetFreeze(float freezeDuration) {

        yield return new WaitForSeconds(freezeDuration);
        waterTilemap.SetTile(tilePos, waterTile);

        freezeResetCoroutine = null;

    }
}
