public class KineticPrimaryAction : PrimaryAction {

    public void OnSwitchTo() { // current weapon is weapon that is being switched to (primary action weapon)

        // destroy meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        charWeaponHandler.CurrentWeapon.OnShoot += OnShoot; // subscribe to shoot event

    }

    public void OnSwitchFrom() { // current weapon is weapon that is being switched to (secondary action weapon)

        // destroy meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        charWeaponHandler.CurrentWeapon.OnShoot -= OnShoot; // remove shoot event

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
