using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileSecondaryAction : SecondaryAction {

    [Header("Action")]
    [SerializeField] private float maxBarrierDuration;
    private bool isBarrierDeployed;

    public override void OnTriggerHold(bool startHold) {

        if (!isReady || isBarrierDeployed == startHold) return; // make sure player is ready, and is not already in the desired state

        if (!canUseInAir && !playerController.IsGrounded()) { // make sure player is grounded if required

            if (isBarrierDeployed) { // if barrier is deployed, retract it

                playerController.RetractBarrier();
                isBarrierDeployed = false;

                // begin cooldown
                isReady = false;
                Invoke("ReadyAction", secondaryCooldown);

            }

            return;

        }

        isBarrierDeployed = startHold;

        if (isBarrierDeployed) {

            playerController.DeployBarrier(maxBarrierDuration);

        } else {

            playerController.RetractBarrier();

            // begin cooldown (placed here on toggle actions)
            isReady = false;
            Invoke("ReadyAction", secondaryCooldown);

        }
    }

    public override void OnDeath() {

        base.OnDeath();

        // if barrier is deployed, retract it
        if (isBarrierDeployed) {

            playerController.RetractBarrier();
            isBarrierDeployed = false;

        }
    }

    public override bool IsRegularAction() => false;

    public bool IsBarrierDeployed() => isBarrierDeployed;

}
