using MoreMountains.CorgiEngine;
using UnityEngine;

public class WindSecondaryAction : SecondaryAction {

    [Header("References")]
    private CorgiController corgiController;

    [Header("Settings")]
    [SerializeField] private float playerWindForce;

    private void Start() => corgiController = GetComponent<CorgiController>();

    public override void OnTriggerRegular() {

        if (!isReady) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        corgiController.SetForce(((Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2) transform.position).normalized * playerWindForce);

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", cooldown);

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(cooldown); // create new meter for cooldown

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
