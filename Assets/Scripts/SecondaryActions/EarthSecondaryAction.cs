using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private ParticleSystem boulderExplosion;
    private GameObject currBoulder;

    [Header("Settings")]
    [SerializeField] private bool spawnAtMousePosition;

    public override void OnTriggerRegular() {

        if (!isReady) return; // make sure player is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        if (!currBoulder)
            SpawnBoulder(); // spawn boulder
        else
            DestroyBoulder(); // destroy boulder

    }

    private void SpawnBoulder() {

        if (spawnAtMousePosition)
            currBoulder = Instantiate(boulderPrefab, (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition), boulderPrefab.transform.rotation); // spawn boulder at mouse position
        else
            currBoulder = Instantiate(boulderPrefab, (Vector2) transform.position + playerController.GetDirectionRight(), boulderPrefab.transform.rotation); // spawn boulder in direction player is facing

    }

    private void DestroyBoulder() {

        Instantiate(boulderExplosion, currBoulder.transform.position, boulderExplosion.transform.rotation); // spawn explosion particles
        Destroy(currBoulder); // destroy boulder

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

    }

    public override bool IsRegularAction() => true;

}
