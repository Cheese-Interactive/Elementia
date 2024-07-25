public class KineticPrimaryAction : PrimaryAction {

    // current weapon is weapon that is being switched to (primary action weapon)
    public void OnSwitchTo() => playerController.SetWeaponHandlerEnabled(cooldownTimer == 0f); // enable weapon handler if shot is ready

    // current weapon is weapon that is being switched to (secondary action weapon)
    public void OnSwitchFrom() => playerController.SetWeaponHandlerEnabled(true); // enable weapon handler

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
