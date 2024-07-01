using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private Flamethrower flamethrower;

    [Header("Action")]
    [SerializeField] private float objectFlamethrowerForce;
    [SerializeField] private float maxFlamethrowerDuration;
    private bool isFlamethrowerEquipped;

    private new void Start() {

        base.Start();

        flamethrower.SetFlamethrowerForce(objectFlamethrowerForce); // set flamethrower force

    }

    public override void OnTriggerHold(bool startHold) {

        if (!isReady || isFlamethrowerEquipped == startHold) return; // make sure player is ready and is not already in the desired state

        if (!canUseInAir && !playerController.IsGrounded()) { // make sure player is grounded if required

            if (isFlamethrowerEquipped) { // if flamethrower is equipped, unequip it

                playerController.UnequipFlamethrower();
                isFlamethrowerEquipped = false;

                // begin cooldown
                isReady = false;
                Invoke("ReadyAction", secondaryCooldown);

            }

            return;

        }

        isFlamethrowerEquipped = startHold;

        if (isFlamethrowerEquipped) {

            playerController.EquipFlamethrower(maxFlamethrowerDuration);

        } else {

            playerController.UnequipFlamethrower();

            // begin cooldown (placed here on toggle actions)
            isReady = false;
            Invoke("ReadyAction", secondaryCooldown);

        }
    }

    public override void OnDeath() {

        base.OnDeath();

        // if flamethrower is equipped, unequip it
        if (isFlamethrowerEquipped) {

            playerController.UnequipFlamethrower();
            isFlamethrowerEquipped = false;

        }
    }

    public override bool IsRegularAction() => false;

    public bool IsFlamethrowerEquipped() => isFlamethrowerEquipped;

}
