using MoreMountains.CorgiEngine;
using UnityEngine;

public class WindSecondaryAction : SecondaryAction {

    [Header("References")]
    private CorgiController corgiController;

    [Header("Settings")]
    [SerializeField] private float playerWindForce;

    private void Start() => corgiController = GetComponent<CorgiController>();

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        corgiController.SetForce(((Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2) transform.position).normalized * playerWindForce);

        cooldownTimer = cooldown; // restart cooldown timer
        weaponSelector.SetSecondaryCooldownValue(GetNormalizedCooldown(), cooldownTimer); // update secondary cooldown meter

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
