using System.Collections;
using UnityEngine;

public class EarthSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private ParticleSystem boulderExplosion;
    private GameObject currBoulder;

    [Header("Settings")]
    [SerializeField] private bool spawnAtMousePosition;
    [SerializeField] private bool destroyOnSwitch;
    [SerializeField][Tooltip("Only for boulder not spawning at mouse position")] private Vector2 spawnOffset;

    [Header("Duration")]
    [SerializeField] private bool hasMaxLifetime;
    [SerializeField] private float maxLifetimeDuration;
    private Coroutine durationCoroutine;

    private new void OnDisable() {

        base.OnDisable();

        // destroy boulder if destroy on switch is enabled and boulder exists
        if (currBoulder && destroyOnSwitch)
            DestroyBoulder();

    }

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        if (currBoulder)
            DestroyBoulder(); // destroy boulder if it exists

        SpawnBoulder(); // spawn boulder

    }

    private void SpawnBoulder() {

        if (spawnAtMousePosition) {

            currBoulder = Instantiate(boulderPrefab, (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition), boulderPrefab.transform.rotation); // spawn boulder at mouse position

        } else {

            Vector2 offset = playerController.GetDirectionRight() * spawnOffset.x + Vector2.up * spawnOffset.y;
            currBoulder = Instantiate(boulderPrefab, (Vector2) transform.position + offset, boulderPrefab.transform.rotation); // spawn boulder in direction player is facing

        }

        if (hasMaxLifetime) // if boulder has max lifetime duration
            durationCoroutine = StartCoroutine(HandleMaxDuration()); // start max duration coroutine

        StartCooldown(); // start cooldown

    }

    private void DestroyBoulder() {

        if (durationCoroutine != null) StopCoroutine(durationCoroutine); // stop max duration coroutine if it exists as boulder is being destroyed
        durationCoroutine = null;

        Instantiate(boulderExplosion, currBoulder.transform.position, boulderExplosion.transform.rotation); // spawn explosion particles
        Destroy(currBoulder); // destroy boulder

    }

    private IEnumerator HandleMaxDuration() {

        float timer = 0f;

        while (timer < maxLifetimeDuration) {

            timer += Time.deltaTime;
            yield return null;

        }

        DestroyBoulder(); // destroy boulder after max duration
        durationCoroutine = null;

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
