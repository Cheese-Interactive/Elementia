using System.Collections;
using UnityEngine;

public class EarthSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private ParticleSystem boulderExplosion;
    private GameObject currBoulder;

    [Header("Settings")]
    [SerializeField] private bool spawnAtMousePosition;

    [Header("Duration")]
    [SerializeField] private float maxDuration;
    private Coroutine durationCoroutine;

    public override void OnTriggerRegular() {

        if (!isReady) return; // make sure action is ready

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

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(maxDuration); // create new meter for max duration

        durationCoroutine = StartCoroutine(HandleMaxDuration()); // start max duration coroutine

    }

    private void DestroyBoulder() {

        if (durationCoroutine != null) StopCoroutine(durationCoroutine); // stop max duration coroutine as boulder is being destroyed
        durationCoroutine = null;

        Instantiate(boulderExplosion, currBoulder.transform.position, boulderExplosion.transform.rotation); // spawn explosion particles
        Destroy(currBoulder); // destroy boulder

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", cooldown);

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(cooldown); // create new meter for cooldown

    }

    private IEnumerator HandleMaxDuration() {

        float timer = 0f;

        while (timer < maxDuration) {

            timer += Time.deltaTime;
            yield return null;

        }

        DestroyBoulder(); // destroy boulder after max duration
        durationCoroutine = null;

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
