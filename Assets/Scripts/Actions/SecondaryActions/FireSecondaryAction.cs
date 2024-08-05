using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class FireSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private Weapon flamethrowerWeapon;
    private Weapon prevWeapon;
    private bool prevAlwaysShoot;
    private bool isFlamethrowerEquipped;

    [Header("Settings")]
    [SerializeField] private float flameSpeed;
    [SerializeField] private Vector2 objectFlamethrowerForce;
    [SerializeField] private Vector2 entityFlamethrowerForce;
    [SerializeField] private float burnDamage;
    [SerializeField] private int burnTicks;
    [SerializeField] private float burnDuration;

    [Header("Duration")]
    [SerializeField] private float maxDuration;
    private Coroutine durationCoroutine;

    [Header("Feedback")]
    [SerializeField] private MMF_Player onUseFeedback;

    public override void OnTriggerHold(bool startHold) {

        if (cooldownTimer > 0f || isFlamethrowerEquipped == startHold) return; // make sure action is ready and is not already in the desired state

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

        isFlamethrowerEquipped = true;

        prevWeapon = playerController.GetCurrentWeapon(); // store current weapon to revert back to it later (don't use character weapon handler's current weapon because it gets destroyed when flamethrower is equipped)

        charWeaponHandler.ChangeWeapon(flamethrowerWeapon, flamethrowerWeapon.WeaponID); // equip flamethrower
        charWeaponHandler.CurrentWeapon.GetComponent<Flamethrower>().Initialize(flameSpeed, entityFlamethrowerForce, objectFlamethrowerForce, burnDamage, burnTicks, burnDuration); // initialize flamethrower

        prevAlwaysShoot = charWeaponHandler.ForceAlwaysShoot; // store previous always shoot value
        charWeaponHandler.ForceAlwaysShoot = true; // force flamethrower to always shoot

        durationCoroutine = StartCoroutine(HandleMaxDuration()); // start max duration coroutine

    }

    private void UnequipFlamethrower() {

        if (!isFlamethrowerEquipped) return; // make sure flamethrower is equipped

        if (durationCoroutine != null) StopCoroutine(durationCoroutine); // stop max duration coroutine as flamethrower is being unequipped
        durationCoroutine = null;

        charWeaponHandler.ForceAlwaysShoot = prevAlwaysShoot; // reset always shoot
        charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID); // revert back to previous weapon
        isFlamethrowerEquipped = false;

        onUseFeedback.StopFeedbacks(); // stop use sound
        StartCooldown(); // start cooldown

    }

    private IEnumerator HandleMaxDuration() {

        float timer = 0f;
        onUseFeedback.PlayFeedbacks(); // play use sound

        while (timer < maxDuration) {

            timer += Time.deltaTime;
            yield return null;

        }

        onUseFeedback.StopFeedbacks(); // stop use sound
        UnequipFlamethrower(); // unequip flamethrower after max duration
        durationCoroutine = null;

    }

    public override void OnDeath() {

        cooldownTimer = 0f; // reset cooldown timer
        UnequipFlamethrower(); // unequip flamethrower if equipped

    }

    public override bool IsRegularAction() => false;

    public override bool IsUsing() => isFlamethrowerEquipped;

}
