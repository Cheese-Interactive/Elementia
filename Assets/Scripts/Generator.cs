using DG.Tweening;
using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;

public class Generator : MonoBehaviour {

    [Header("Reference")]
    [SerializeField] private GameObject field;
    [SerializeField] private Collider2D fieldCollider; // must be serialized for gizmos
    private SpriteRenderer fieldSpriteRenderer;
    private Animator animator;
    private GameManager gameManager;
    private Coroutine fieldCoroutine;
    private Tweener fieldTweener;
    private Coroutine cooldownCoroutine;

    [Header("Settings")]
    [SerializeField] private float activateDuration; // basically the duration of the field
    [SerializeField] private float coolingDuration; // basically the cooldown duration
    [SerializeField] private Color targetFieldColor;
    [SerializeField][Range(0f, 100f)] private float fieldFadeInDurationPercentage;
    [SerializeField][Range(0f, 100f)] private float fieldFadeOutDurationPercentage;
    private Color startColor;
    private float fadeInDuration;
    private float fadeOutDuration;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player activationFeedback;
    [SerializeField] private MMF_Player deactivationFeedback;

    [Header("Debug")]
    [SerializeField] private Color fieldVisualizerColor;
    [SerializeField] private Color fieldOutlineVisualizerColor;

    private void Start() {

        animator = GetComponent<Animator>();

        field.SetActive(false); // deactivate the field by default

        fieldSpriteRenderer = field.GetComponent<SpriteRenderer>();
        fieldCollider = field.GetComponent<Collider2D>();
        gameManager = FindObjectOfType<GameManager>();

        fieldSpriteRenderer.color = new Color(fieldSpriteRenderer.color.r, fieldSpriteRenderer.color.g, fieldSpriteRenderer.color.b, 0f); // set field opacity to 0
        startColor = fieldSpriteRenderer.color; // store start color of field

        // make sure fade in and fade out durations do not exceed field duration
        if (fieldFadeInDurationPercentage + fieldFadeOutDurationPercentage > 100)
            Debug.LogError("Field fade in and fade out durations exceed field duration.");

        fadeInDuration = (fieldFadeInDurationPercentage / 100f) * activateDuration; // calculate fade in duration
        fadeOutDuration = (fieldFadeOutDurationPercentage / 100f) * activateDuration; // calculate fade out duration

    }

    public void Activate() {

        if (fieldCoroutine != null || cooldownCoroutine != null) return; // return if field is active or generator is cooling down

        field.SetActive(true); // activate field
        activationFeedback.PlayFeedbacks(transform.position);
        if (fieldTweener != null && fieldTweener.IsActive()) fieldTweener.Kill(); // kill field tweener if it is active
        fieldCoroutine = StartCoroutine(HandleField());

        // set generator to activated state
        animator.SetBool("isOverheated", false);
        animator.SetBool("isActivated", true);

    }

    private IEnumerator HandleField() {

        gameManager.SetCooldownsEnabled(false); // disable cooldowns

        fieldTweener = fieldSpriteRenderer.DOColor(targetFieldColor, fadeInDuration).SetEase(Ease.InExpo); // fade in field
        yield return new WaitForSeconds(activateDuration - fadeOutDuration); // wait till start of fade out
        fieldTweener = fieldSpriteRenderer.DOColor(startColor, fadeOutDuration).SetEase(Ease.OutExpo); // fade out field
        yield return new WaitForSeconds(fadeOutDuration); // wait for fade out to finish

        field.SetActive(false);
        gameManager.SetCooldownsEnabled(true); // re-enable cooldowns

        fieldCoroutine = null;

        if (cooldownCoroutine != null) StopCoroutine(cooldownCoroutine); // stop previous cooldown coroutine if it exists
        cooldownCoroutine = StartCoroutine(HandleCooldown());

    }

    private IEnumerator HandleCooldown() {

        // set generator to overheat/cooldown state
        animator.SetBool("isActivated", false);
        animator.SetBool("isOverheated", true);
        deactivationFeedback.PlayFeedbacks(transform.position);

        yield return new WaitForSeconds(coolingDuration);

        // set generator to idle state
        animator.SetBool("isActivated", false);
        animator.SetBool("isOverheated", false);

        cooldownCoroutine = null;

    }

    private void OnDrawGizmos() {

        // draw outline
        Gizmos.color = fieldOutlineVisualizerColor;
        Gizmos.DrawWireSphere(transform.position, fieldCollider.bounds.size.x / 2f);

        // draw field
        Gizmos.color = fieldVisualizerColor;
        Gizmos.DrawSphere(transform.position, fieldCollider.bounds.size.x / 2f);

    }
}
