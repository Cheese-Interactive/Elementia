using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class ElectricEffect : BaseEffect {

    [Header("References")]
    [SerializeField] private ParticleSystem electricOverlay;
    private CharacterHorizontalMovement charMovement;
    private CharacterLadder charLadder;
    private Health health;
    private Coroutine resetEffectCoroutine;

    [Header("Settings")]
    [SerializeField] private bool affectSpeed;
    [SerializeField] private bool affectClimb;
    [SerializeField] private bool affectDamageImmunity;
    private GameObject instigator;
    private float electricDamage;
    private float invincibilityDuration;
    private float prevMoveSpeed;
    private float prevClimbSpeed;

    private void Start() {

        charMovement = GetComponent<CharacterHorizontalMovement>();
        charLadder = GetComponent<CharacterLadder>();
        health = GetComponent<Health>();

        electricOverlay.gameObject.SetActive(false); // disable electric effect by default

        if (affectSpeed && charMovement)
            prevMoveSpeed = charMovement.MovementSpeed; // store initial movement speed

        if (affectClimb && charLadder)
            prevClimbSpeed = charLadder.LadderClimbingSpeed; // store initial climbing speed

    }

    private void OnTriggerStay2D(Collider2D collision) {

        if (collision.gameObject.activeInHierarchy && resetEffectCoroutine != null) // make sure hit object is active and electric effect is active
            collision.GetComponent<Health>()?.Damage(electricDamage, instigator, invincibilityDuration, invincibilityDuration, (collision.transform.position - transform.position).normalized); // damage object if it has health component

    }

    public void AddEffect(GameObject instigator, float damage, float duration, float invincibilityDuration, float speedMultiplier) {

        this.instigator = instigator; // store instigator
        this.electricDamage = damage; // store electric damage
        this.invincibilityDuration = invincibilityDuration; // store invincibility duration

        // movement effects
        if (affectSpeed && charMovement) {

            prevMoveSpeed = charMovement.MovementSpeed; // store current movement speed before applying electric effect
            charMovement.MovementSpeed *= speedMultiplier; // apply speed multiplier to movement speed

        }

        // climb effects
        if (affectClimb && charLadder) {

            prevClimbSpeed = charLadder.LadderClimbingSpeed; // store current climbing speed before applying electric effect
            charLadder.LadderClimbingSpeed *= speedMultiplier; // apply speed multiplier to ladder climbing speed

        }

        // damage immunity effects
        if (affectDamageImmunity && health)
            health.ImmuneToDamage = true; // enable player immunity to  (however they are not immune to water/drown damage)

        electricOverlay.gameObject.SetActive(true); // enable electric effect

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

        // reset damage immunity
        if (affectDamageImmunity && health)
            health.ImmuneToDamage = false;

        // reset climbing speed
        if (affectClimb && charLadder)
            charLadder.LadderClimbingSpeed = prevClimbSpeed;

        // reset movement speed
        if (affectSpeed && charMovement)
            charMovement.MovementSpeed = prevMoveSpeed;

        electricOverlay.gameObject.SetActive(false); // disable electric effect

    }
}
