using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerAction : SecondaryAction {

    [Header("Action")]
    [SerializeField] private float maxFlamethrowerDuration;
    private bool isFlamethrowerEquipped;

    public override void OnTrigger() {

        if (!isReady && !isSecondaryToggle) return;

        isFlamethrowerEquipped = !isFlamethrowerEquipped; // do this before returning to deal with toggle issues

        if (isFlamethrowerEquipped)
            player.EquipFlamethrower(maxFlamethrowerDuration);
        else
            player.UnequipFlamethrower();

        // begin cooldown
        isReady = false;
        Invoke("ReadyAction", secondaryCooldown);

    }

    public override void OnDeath() {

        base.OnDeath();

        // if flamethrower is equipped, unequip it
        if (isFlamethrowerEquipped)
            player.UnequipFlamethrower();

    }

    public override void SetInitialToggled(bool isToggled) {

        base.SetInitialToggled(isToggled);
        isFlamethrowerEquipped = isToggled;

    }

    public bool IsFlamethrowerEquipped() => isFlamethrowerEquipped;

}
