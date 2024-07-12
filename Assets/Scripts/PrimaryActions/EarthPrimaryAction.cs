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

    private void Start() => charWeaponHandler = GetComponent<CharacterHandleWeapon>();

    private new void OnDisable() {

        base.OnDisable();
        charWeaponHandler.AbilityPermitted = true; // disables when weapon is switched, re-enable weapon handler

    }

    public override void OnTriggerRegular() {

        if (!isReady) return; // make sure action is ready

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
        currRock = playerController.OnSummonRock(this, rockPrefabs[Random.Range(0, rockPrefabs.Length)], maxThrowDuration);

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

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", primaryCooldown);

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(primaryCooldown); // create new meter for cooldown

    }

    public override void OnSwitchCooldownComplete() {

        base.OnSwitchCooldownComplete();
        charWeaponHandler.AbilityPermitted = false; // disable weapon handler here

    }

    public override bool IsRegularAction() => true;

    public bool IsSummoningRock() => isSummoningRock;

    public bool IsRockThrowReady() => isRockThrowReady;

}
