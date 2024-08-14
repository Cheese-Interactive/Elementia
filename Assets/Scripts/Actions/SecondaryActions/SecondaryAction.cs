public abstract class SecondaryAction : Action {

    protected override void StartCooldown(bool restartTimer = true) {

        if (playerController.IsDead()) return; // do not start cooldown if player is dead

        base.StartCooldown(restartTimer);

        if (gameManager.IsCooldownsEnabled())
            weaponSelector.SetSecondaryCooldownValue(GetNormalizedCooldown(), cooldownTimer); // update secondary cooldown meter

    }
}
