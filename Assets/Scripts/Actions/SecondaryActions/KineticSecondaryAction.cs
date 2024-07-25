using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class KineticSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private Weapon secondaryKineticWeapon;
    private KineticPrimaryAction primaryAction;
    private Weapon prevWeapon;
    private Coroutine weaponSwitchCoroutine;

    private void Start() => primaryAction = GetComponent<KineticPrimaryAction>();

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f || weaponSwitchCoroutine != null) return; // make sure action is ready and weapon switch is not already in progress

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        weaponSwitchCoroutine = StartCoroutine(HandleWeaponSwitch()); // handle weapon switch

    }

    private IEnumerator HandleWeaponSwitch() {

        prevWeapon = playerController.GetCurrentWeapon(); // save current weapon
        charWeaponHandler.ChangeWeapon(secondaryKineticWeapon, secondaryKineticWeapon.WeaponID); // change weapon to secondary kinetic weapon
        playerController.SetWeaponHandlerEnabled(true); // enable weapon handler so weapon can be shot
        primaryAction.OnSwitchFrom(); // trigger primary action switch from event

        yield return null; // wait for weapon change

        charWeaponHandler.ShootStart(); // shoot kinetic projectile

        // must wait for two frames to allow projectile to be fired
        yield return null;
        yield return null;

        charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID); // change weapon back to previous weapon
        primaryAction.OnSwitchTo(); // trigger primary action switch to event

        cooldownTimer = cooldown; // restart cooldown timer
        weaponSelector.SetSecondaryCooldownValue(GetNormalizedCooldown(), cooldownTimer); // update secondary cooldown meter

        weaponSwitchCoroutine = null;

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
