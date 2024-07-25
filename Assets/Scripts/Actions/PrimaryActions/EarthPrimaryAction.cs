using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

public class EarthPrimaryAction : PrimaryAction {

    [Header("References")]
    [SerializeField] private Rock[] rockPrefabs;

    [Header("Summon")]
    private Rock currRock; // if null, rock hasn't been summoned yet
    private bool isSummoningRock;
    private bool isRockThrowReady;

    [Header("Duration")]
    [SerializeField] private float maxThrowDuration;
    private Coroutine throwDurationCoroutine;

    protected new void OnEnable() {

        base.OnEnable();
        playerController.SetWeaponHandlerEnabled(false); // disable weapon handler by default

    }

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        if (isRockThrowReady) { // rock is fully summoned & rock is thrown (handled by weapon)

            MMSimpleObjectPooler pool = charWeaponHandler.CurrentWeapon.GetComponent<MMSimpleObjectPooler>();
            pool.GameObjectToPool = currRock.GetProjectile().gameObject; // set new rock projectile
            pool.FillObjectPool(); // fill projectile pool

            ThrowRock();
            return;

        }

        if (!currRock) // mouse button pressed
            SummonRock();
        else // mouse button released & rock hasn't been fully summoned yet
            DestroyRock(true); // destroy rock before it's fully summoned (activate mechanics)

    }

    private void SummonRock() {

        isSummoningRock = true; // rock is being summoned
        currRock = playerController.OnSummonRock(this, rockPrefabs[Random.Range(0, rockPrefabs.Length)]);

    }

    private void ThrowRock() {

        isRockThrowReady = false;
        playerController.OnRockThrow();
        DestroyRock(false); // destroy rock after throw (don't activate mechanics)

    }

    private IEnumerator HandleMaxThrowDuration() {

        float timer = 0f;

        while (timer < maxThrowDuration) {

            timer += Time.deltaTime;
            yield return null;

        }

        DestroyRock(true); // destroy rock after max throw duration (activate mechanics)
        throwDurationCoroutine = null;

    }

    public void OnThrowReady() {

        isSummoningRock = false; // rock is fully summoned
        isRockThrowReady = true; // rock is ready to be thrown
        throwDurationCoroutine = StartCoroutine(HandleMaxThrowDuration()); // start max throw duration coroutine

    }

    // triggers on both cancels and throws
    private void DestroyRock(bool activateMechanics) {

        if (throwDurationCoroutine != null) StopCoroutine(throwDurationCoroutine); // stop max duration coroutine as rock is being destroyed
        throwDurationCoroutine = null;

        playerController.OnDestroyRock(activateMechanics); // activate mechanics after rock is destroyed

        // reset bools
        isSummoningRock = false;
        isRockThrowReady = false;

        cooldownTimer = cooldown; // restart cooldown timer
        weaponSelector.SetPrimaryCooldownValue(GetNormalizedCooldown(), cooldownTimer); // update primary cooldown meter

    }

    public override void OnDeath() {

        base.OnDeath();

        if (!enabled) return; // make sure action is enabled (done to make sure this override only runs on this script, otherwise only run base)

        playerController.SetWeaponHandlerEnabled(false); // disable weapon handler on death

    }

    public override bool IsRegularAction() => true;

    public bool IsSummoningRock() => isSummoningRock;

    public bool IsRockThrowReady() => isRockThrowReady;

    public override bool IsUsing() => isSummoningRock;

}
