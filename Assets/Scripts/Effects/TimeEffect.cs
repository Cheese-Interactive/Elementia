using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeEffect : BaseEffect {

    [Header("References")]
    private EntityController entityController;

    [Header("Slow")]
    private Coroutine unfreezeTimeCoroutine;

    [Header("Overlay")]
    [SerializeField] private Overlay timeOverlay;

    private void Start() {

        entityController = GetComponent<EntityController>();

        if (!entityController) Debug.LogError("EntityController not found on " + gameObject.name); // make sure entity controller is set

        timeOverlay.HideOverlay(); // hide slow overlay by default

    }

    public void FreezeTime(float duration) {

        timeOverlay.ShowOverlay(); // show time overlay

        if (unfreezeTimeCoroutine != null) StopCoroutine(unfreezeTimeCoroutine);

        entityController.DisableAllMechanics(); // disable all mechanics
        entityController.DisableCoreScripts(); // disable core scripts
        entityController.SetInvulnerable(true); // set entity invulnerable

        unfreezeTimeCoroutine = StartCoroutine(UnfreezeTime(duration));

    }

    private IEnumerator UnfreezeTime(float duration) {

        yield return new WaitForSeconds(duration);
        RemoveEffect();

    }

    public void RemoveEffect() {

        if (unfreezeTimeCoroutine != null) StopCoroutine(unfreezeTimeCoroutine); // stop time unfreeze coroutine

        timeOverlay.HideOverlay(); // hide slow overlay

        entityController.EnableCoreScripts(); // enable core scripts
        entityController.EnableAllMechanics(); // enable all mechanics
        entityController.SetInvulnerable(false); // set entity vulnerable

    }
}
