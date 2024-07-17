public abstract class PrimaryAction : Action {

    private new void OnEnable() {

        base.OnEnable();
        charWeaponHandler.CurrentWeapon.OnShoot += OnShoot; // subscribe to shoot event

    }

    // runs before weapon is switched
    protected new void OnDisable() {

        base.OnDisable();
        charWeaponHandler.CurrentWeapon.OnShoot -= OnShoot; // remove shoot event

    }

    public virtual void OnShoot() => currMeter = CreateMeter(charWeaponHandler.CurrentWeapon.TimeBetweenUses); // create new meter for cooldown (use the weapon cooldown instead of primary action cooldown)

}
