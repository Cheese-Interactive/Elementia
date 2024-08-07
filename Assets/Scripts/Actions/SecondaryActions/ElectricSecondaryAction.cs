using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class ElectricSecondaryAction : SecondaryAction {

    [Header("References")]
    private ElectricEffect electricEffect;
    private Coroutine effectCoroutine;
    private bool hasEffect;

    [Header("Settings")]
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float electricDuration;
    [SerializeField] private float electricDamage;
    [SerializeField] private float electricDamageInvincibilityDuration;

    [Header("Feedback")]
    [SerializeField] private MMF_Player onUseFeedback;
    [SerializeField] private MMF_Player onEndFeedback;

    private new void Awake() {

        base.Awake();
        electricEffect = GetComponent<ElectricEffect>();

    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (enabled && collision.gameObject.activeInHierarchy && collision.gameObject.CompareTag("Generator")) // make sure action is enabled, collider is active, and collider is a generator
            collision.gameObject.GetComponent<Generator>()?.Activate();

    }

    public override void OnTriggerRegular() {

        if (cooldownTimer > 0f || effectCoroutine != null) return; // make sure action is ready and effect is not active

        if (!canUseInAir && !playerController.IsGrounded()) return; // make sure player is grounded if required

        AddEffect(); // add electric effect

    }

    private void AddEffect() {

        if (hasEffect) return; // make sure effect is not already active

        hasEffect = true;

        electricEffect.AddEffect(gameObject, electricDamage, electricDuration, electricDamageInvincibilityDuration, speedMultiplier); // apply electric effect
        onUseFeedback.PlayFeedbacks(); // play start sound

        if (effectCoroutine != null) StopCoroutine(effectCoroutine); // stop previous effect coroutine if it exists
        effectCoroutine = StartCoroutine(HandleEffect()); // start effect coroutine

    }

    private IEnumerator HandleEffect() {

        yield return new WaitForSeconds(electricDuration);
        RemoveEffect(); // remove electric effect
        effectCoroutine = null; // reset effect coroutine

    }

    private void RemoveEffect() {

        if (!hasEffect) return; // make sure effect is active

        if (effectCoroutine != null) StopCoroutine(effectCoroutine); // stop effect coroutine if it exists
        effectCoroutine = null;

        electricEffect.RemoveEffect(); // remove electric effect
        onEndFeedback.PlayFeedbacks(); // play end sound

        hasEffect = false;

        StartCooldown(); // start cooldown

    }

    public override void OnDeath() {

        cooldownTimer = 0f; // reset cooldown timere
        RemoveEffect(); // remove electric effect

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
