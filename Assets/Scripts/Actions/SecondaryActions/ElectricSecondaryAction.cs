using System.Collections;
using UnityEngine;

public class ElectricSecondaryAction : SecondaryAction {

    [Header("References")]
    private ElectricEffect electricEffect;
    private Coroutine effectCoroutine;

    [Header("Settings")]
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float electricDuration;
    [SerializeField] private float electricDamage;
    [SerializeField] private float electricDamageInvincibilityDuration;

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

        electricEffect.AddEffect(gameObject, electricDamage, electricDuration, electricDamageInvincibilityDuration, speedMultiplier); // apply electric effect

        if (effectCoroutine != null) StopCoroutine(effectCoroutine); // stop previous effect coroutine if it exists
        effectCoroutine = StartCoroutine(HandleEffect()); // start effect coroutine

    }

    private IEnumerator HandleEffect() {

        yield return new WaitForSeconds(electricDuration);

        StartCooldown(); // start cooldown

        effectCoroutine = null; // reset effect coroutine

    }

    public override bool IsRegularAction() => true;

    public override bool IsUsing() => false;

}
