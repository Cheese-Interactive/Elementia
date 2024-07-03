using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthPrimaryAction : PrimaryAction {

    [Header("References")]
    [SerializeField] private Rock[] rockPrefabs;
    private CharacterHandleWeapon charWeaponHandler;

    [Header("Summon")]
    private Rock currRock; // if null, rock hasn't been summoned yet
    private bool isSummoningRock;
    private bool isRockThrowReady;

    [Header("Duration")]
    [SerializeField] private float maxThrowDuration;
    private Coroutine throwDurationCoroutine;

    private new void Start() {

        base.Start();

        charWeaponHandler = GetComponent<CharacterHandleWeapon>();
        charWeaponHandler.AbilityPermitted = false; // disable weapon until rock is summoned and ready to be thrown

    }

    private void OnDisable() => charWeaponHandler.AbilityPermitted = true; // disables when weapon is switched, re-enable weapon handler

    public override void OnTriggerRegular() {

        if (!isReady) return; // make sure player is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        if (isRockThrowReady) { // rock is fully summoned & rock is thrown (handled by weapon)

            MMSimpleObjectPooler pool = charWeaponHandler.CurrentWeapon.GetComponent<MMSimpleObjectPooler>();
            pool.GameObjectToPool = currRock.GetProjectile().gameObject; // set new rock projectile
            pool.FillObjectPool(); // fill weapon pool

            ThrowRock();
            return;

        }

        if (!currRock) // mouse button pressed
            SummonRock();
        else // mouse button released & rock hasn't been fully summoned yet
            DestroyRock();

    }

    private void SummonRock() {

        isSummoningRock = true; // rock is being summoned
        currRock = playerController.OnSummonRock(this, rockPrefabs[Random.Range(0, rockPrefabs.Length)], maxThrowDuration);

    }

    private void ThrowRock() {

        playerController.OnRockThrow();
        isRockThrowReady = false;

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", primaryCooldown);

    }

    private void DestroyRock() {

        if (throwDurationCoroutine != null) StopCoroutine(throwDurationCoroutine); // stop max duration coroutine as rock is being destroyed

        playerController.OnDestroyRock(true); // activate mechanics after rock is destroyed

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", primaryCooldown);

    }

    private IEnumerator HandleMaxThrowDuration() {

        float timer = 0f;

        while (timer < maxThrowDuration) {

            timer += Time.deltaTime;
            yield return null;

        }

        DestroyRock(); // destroy rock after max throw duration
        throwDurationCoroutine = null;

    }

    public void OnThrowReady() {

        isSummoningRock = false; // rock is fully summoned
        isRockThrowReady = true; // rock is ready to be thrown
        throwDurationCoroutine = StartCoroutine(HandleMaxThrowDuration()); // start max throw duration coroutine

    }

    public override bool IsRegularAction() => true;

    public bool IsSummoningRock() => isSummoningRock;

    public bool IsRockThrowReady() => isRockThrowReady;

}
