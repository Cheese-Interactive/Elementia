using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierAction : SecondaryAction {

    [Header("Action")]
    [SerializeField] private float maxBarrierDuration;
    private bool isBarrierDeployed;

    public override void OnTrigger() {

        if (!isReady && !isSecondaryToggle) return;

        isBarrierDeployed = !isBarrierDeployed; // do this before returning to deal with toggle issues

        if (!player.IsGrounded()) return;

        if (isBarrierDeployed)
            player.DeployBarrier(maxBarrierDuration);
        else
            player.RetractBarrier();

        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

    }
}
