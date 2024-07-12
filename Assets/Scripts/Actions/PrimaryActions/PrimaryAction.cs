public abstract class PrimaryAction : Action {

    // runs before weapon is switched
    private new void OnDisable() {

        base.OnDisable();
        charWeaponHandler.CurrentWeapon.OnShoot -= OnShoot; // remove shoot event

    }

    public virtual void OnShoot() => currMeter = CreateMeter(charWeaponHandler.CurrentWeapon.TimeBetweenUses); // create new meter for cooldown (use the weapon cooldown instead of primary action cooldown)

}
