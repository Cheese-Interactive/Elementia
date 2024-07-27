using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class ElectricSecondaryAction : SecondaryAction {

    [Header("References")]
    [SerializeField] private ParticleSystem electricEffect;
    private CharacterHorizontalMovement charMovement;
    private Coroutine effectCoroutine;

    [Header("Settings")]
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float effectDuration;
    private float prevSpeed;

    private new void Awake() {

        base.Awake();

        charMovement = GetComponent<CharacterHorizontalMovement>();
        electricEffect.gameObject.SetActive(false); // disable electric effect by default

    }

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f || effectCoroutine != null) return; // make sure action is ready and effect is not already active

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        effectCoroutine = StartCoroutine(HandleEffect());

    }

    private IEnumerator HandleEffect() {

        electricEffect.gameObject.SetActive(true); // enable electric effect
        prevSpeed = charMovement.MovementSpeed;
        charMovement.MovementSpeed *= speedMultiplier;
        health.ImmuneToDamage = true; // enable player immunity to damage

        yield return new WaitForSeconds(effectDuration);

        health.ImmuneToDamage = false; // disable player immunity to damage
        charMovement.MovementSpeed = prevSpeed;
        electricEffect.gameObject.SetActive(false); // disable electric effect

        cooldownTimer = cooldown; // restart cooldown timer
        weaponSelector.SetSecondaryCooldownValue(GetNormalizedCooldown(), cooldownTimer); // update secondary cooldown meter

        effectCoroutine = null;

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
