using MoreMountains.CorgiEngine;
using MoreMountains.Feedbacks;
using UnityEngine;

public class WindSecondaryAction : SecondaryAction {

    [Header("References")]
    private CorgiController corgiController;

    [Header("Settings")]
    [SerializeField] private float playerWindForce;

    [Header("MM Feedbacks")]
    [SerializeField] private MMF_Player onUseFeedback;

    private void Start() => corgiController = GetComponent<CorgiController>();

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        corgiController.SetForce(((Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2)transform.position).normalized * playerWindForce);
        onUseFeedback.PlayFeedbacks();

        StartCooldown(); // start cooldown

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
