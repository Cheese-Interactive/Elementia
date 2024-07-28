using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class ElectricEffect : BaseEffect {

    [Header("References")]
    [SerializeField] private ParticleSystem electricOverlay;
    private CharacterHorizontalMovement charMovement;
    private Health health;
    private Coroutine resetEffectCoroutine;

    [Header("Settings")]
    private GameObject instigator;
    private float electricDamage;
    private float invincibilityDuration;
    private float prevSpeed;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        health = GetComponent<Health>();

        electricOverlay.gameObject.SetActive(false); // disable electric effect by default
        prevSpeed = charMovement.MovementSpeed; // store initial speed

    }

    private void OnTriggerStay2D(Collider2D collision) {

        if (collision.gameObject.activeInHierarchy && resetEffectCoroutine != null) // make sure hit object is active and electric effect is active
            collision.GetComponent<Health>()?.Damage(electricDamage, instigator, invincibilityDuration, invincibilityDuration, (collision.transform.position - transform.position).normalized); // damage object if it has health component

    }

    public void AddEffect(GameObject instigator, float damage, float duration, float invincibilityDuration, float speedMultiplier) {

        this.instigator = instigator; // store instigator
        this.electricDamage = damage; // store electric damage
        this.invincibilityDuration = invincibilityDuration; // store invincibility duration

        prevSpeed = charMovement.MovementSpeed; // store current speed before applying electric effect
        electricOverlay.gameObject.SetActive(true); // enable electric effect
        charMovement.MovementSpeed *= speedMultiplier; // apply speed multiplier
        health.ImmuneToDamage = true; // enable player immunity to  (however they are not immune to water/drown damage)

        if (resetEffectCoroutine != null) StopCoroutine(resetEffectCoroutine); // stop previous effect coroutine if it exists
        resetEffectCoroutine = StartCoroutine(ResetEffect(duration)); // start effect coroutine

    }

    private IEnumerator ResetEffect(float duration) {

        yield return new WaitForSeconds(duration);
        RemoveEffect(); // remove effect after duration

    }

    public override void RemoveEffect() {

        if (resetEffectCoroutine != null) StopCoroutine(resetEffectCoroutine); // stop effect coroutine if it exists
        resetEffectCoroutine = null; // reset effect coroutine

        health.ImmuneToDamage = false; // disable player immunity to damage
        charMovement.MovementSpeed = prevSpeed; // reset speed to previous value
        electricOverlay.gameObject.SetActive(false); // disable electric effect

    }
}
