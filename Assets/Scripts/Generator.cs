using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Generator : MonoBehaviour {

    [Header("Reference")]
    [SerializeField] private FieldType fieldType;
    [SerializeField] private GameObject field;
    [SerializeField] private Collider2D rectangleFieldCollider;
    [SerializeField] private Collider2D circleFieldCollider;
    private SpriteRenderer fieldSpriteRenderer;
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

    [Header("Debug")]
    [SerializeField] private Color fieldOutlineVisualizerColor;

    private void Start() {

        field.SetActive(false); // deactivate the field by default
        fieldSpriteRenderer = field.GetComponent<SpriteRenderer>();
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
        if (fieldTweener != null && fieldTweener.IsActive()) fieldTweener.Kill(); // kill field tweener if it is active
        fieldCoroutine = StartCoroutine(HandleField());

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

        yield return new WaitForSeconds(coolingDuration);
        cooldownCoroutine = null;

    }

    private void OnDrawGizmos() {

        Gizmos.color = fieldOutlineVisualizerColor;

        if (fieldType == FieldType.Rectangle) {

            if (!rectangleFieldCollider.gameObject.activeInHierarchy || circleFieldCollider.gameObject.activeInHierarchy) { // if rectangle collider is not active or circle collider is active

                circleFieldCollider.gameObject.SetActive(false); // deactivate circle collider
                rectangleFieldCollider.gameObject.SetActive(true); // activate rectangle collider

            }

            Gizmos.DrawWireCube(transform.position, rectangleFieldCollider.bounds.size);

        } else if (fieldType == FieldType.Circle) {

            if (!circleFieldCollider.gameObject.activeInHierarchy || rectangleFieldCollider.gameObject.activeInHierarchy) { // if circle collider is not active or rectangle collider is active

                rectangleFieldCollider.gameObject.SetActive(false); // deactivate rectangle collider
                circleFieldCollider.gameObject.SetActive(true); // activate circle collider

            }

            Gizmos.DrawWireSphere(transform.position, circleFieldCollider.bounds.size.x / 2f);

        }
    }
}
