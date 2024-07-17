using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class KineticSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private Weapon secondaryKineticWeapon;
    private KineticPrimaryAction kineticPrimaryAction;
    private Weapon prevWeapon;
    private Coroutine weaponSwitchCoroutine;

    private void Start() => kineticPrimaryAction = GetComponent<KineticPrimaryAction>();

    public override void OnTriggerRegular() {

        if (!isReady || weaponSwitchCoroutine != null) return; // make sure action is ready and weapon switch is not already in progress

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        weaponSwitchCoroutine = StartCoroutine(HandleWeaponSwitch()); // handle weapon switch

    }

    private IEnumerator HandleWeaponSwitch() {

        prevWeapon = playerController.GetCurrentWeapon(); // save current weapon
        charWeaponHandler.ChangeWeapon(secondaryKineticWeapon, secondaryKineticWeapon.WeaponID); // change weapon to secondary kinetic weapon
        kineticPrimaryAction.OnSwitchFrom();

        yield return null; // wait for weapon change

        charWeaponHandler.ShootStart(); // shoot kinetic projectile

        // must wait for two frames to allow projectile to be fired
        yield return null;
        yield return null;

        charWeaponHandler.ChangeWeapon(prevWeapon, prevWeapon.WeaponID); // change weapon back to previous weapon
        kineticPrimaryAction.OnSwitchTo();

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", cooldown);

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(cooldown); // create new meter for cooldown

        weaponSwitchCoroutine = null;

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
