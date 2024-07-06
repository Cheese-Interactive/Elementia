using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSecondaryAction : SecondaryAction {

    [Header("References")]
    private CorgiController corgiController;
    private Meter currMeter;

    [Header("Settings")]
    [SerializeField] private float playerWindForce;

    private new void Start() {

        base.Start();
        corgiController = GetComponent<CorgiController>();

    }

    public override void OnTriggerRegular() {

        if (!isReady) return; // make sure action is ready

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        corgiController.SetForce(((Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector2) transform.position).normalized * playerWindForce);

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

        // destroy current meter if it exists
        if (currMeter)
            Destroy(currMeter.gameObject);

        currMeter = CreateMeter(secondaryCooldown); // create new meter for cooldown

    }

    public override bool IsRegularAction() => true;

}
