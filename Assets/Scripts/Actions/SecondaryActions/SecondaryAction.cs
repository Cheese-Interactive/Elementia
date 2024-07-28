public abstract class SecondaryAction : Action {

    protected override void StartCooldown(bool restartTimer = true) {

        base.StartCooldown(restartTimer);

        if (gameManager.IsCooldownsEnabled())
            weaponSelector.SetSecondaryCooldownValue(GetNormalizedCooldown(), cooldownTimer); // update secondary cooldown meter

    }
}
