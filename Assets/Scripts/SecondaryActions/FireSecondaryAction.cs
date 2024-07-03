using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private Weapon flamethrower;
    private CharacterHandleWeapon charWeaponHandler;
    private Weapon prevWeapon;
    private bool prevAlwaysShoot;

    [Header("Settings")]
    [SerializeField] private Vector2 objectFlamethrowerForce;
    [SerializeField] private Vector2 entityFlamethrowerForce;
    [SerializeField] private float burnDamage;
    [SerializeField] private int burnTicks;
    [SerializeField] private float burnDuration;

    [Header("Toggle")]
    private bool isFlamethrowerEquipped;

    [Header("Duration")]
    [SerializeField] private float maxDuration;
    private Coroutine durationCoroutine;

    private new void Start() {

        base.Start();

        charWeaponHandler = GetComponent<CharacterHandleWeapon>();

    }

    public override void OnTriggerHold(bool startHold) {

        if (!isReady || isFlamethrowerEquipped == startHold) return; // make sure player is ready and is not already in the desired state

        if (!canUseInAir && !playerController.IsGrounded()) { // make sure player is grounded if required

            if (isFlamethrowerEquipped) // if flamethrower is equipped, unequip it
                UnequipFlamethrower();

            return;

        }

        if (startHold) // mouse button pressed
            EquipFlamethrower();
        else // mouse button released
            UnequipFlamethrower();

    }

    private void EquipFlamethrower() {

        prevWeapon = playerController.GetCurrentWeapon(); // store current weapon to revert back to it later (don't use character weapon handler's current weapon because it gets destroyed when flamethrower is equipped)

        charWeaponHandler.ChangeWeapon(flamethrower, flamethrower.WeaponID); // equip flamethrower
        charWeaponHandler.CurrentWeapon.GetComponent<Flamethrower>().Initialize(entityFlamethrowerForce, objectFlamethrowerForce, burnDamage, burnTicks, burnDuration); // initialize flamethrower

        prevAlwaysShoot = charWeaponHandler.ForceAlwaysShoot; // store previous always shoot value
        charWeaponHandler.ForceAlwaysShoot = true; // force flamethrower to always shoot

        isFlamethrowerEquipped = true;

        durationCoroutine = StartCoroutine(HandleMaxDuration()); // start max duration coroutine

    }

    private void UnequipFlamethrower() {

        if (durationCoroutine != null) StopCoroutine(durationCoroutine); // stop max duration coroutine as flamethrower is being unequipped

        charWeaponHandler.ForceAlwaysShoot = prevAlwaysShoot; // reset always shoot
        charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID); // revert back to previous weapon
        isFlamethrowerEquipped = false;

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

    }

    private IEnumerator HandleMaxDuration() {

        float timer = 0f;

        while (timer < maxDuration) {

            timer += Time.deltaTime;
            yield return null;

        }

        UnequipFlamethrower(); // unequip flamethrower after max duration
        durationCoroutine = null;

    }

    public override void OnDeath() {

        base.OnDeath();

        // if flamethrower is equipped, unequip it
        if (isFlamethrowerEquipped) {

            charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID);
            isFlamethrowerEquipped = false;

        }
    }

    public override bool IsRegularAction() => false;

    public bool IsFlamethrowerEquipped() => isFlamethrowerEquipped;

}
