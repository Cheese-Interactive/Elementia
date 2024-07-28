using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ElectricGenerator : MonoBehaviour {

    [Header("Reference")]
    [SerializeField] private FieldType fieldType;
    [SerializeField] private GameObject field;
    [SerializeField] private Collider2D rectangleFieldCollider;
    [SerializeField] private Collider2D circleFieldCollider;
    private SpriteRenderer fieldSpriteRenderer;
    private GameManager gameManager;
    private Coroutine fieldCoroutine;
    private Tweener fieldTweener;

    [Header("Settings")]
    [SerializeField] private float fieldDuration;
    [SerializeField] private Color targetFieldColor;
    [SerializeField][Range(0f, 100f)] private float fadeInDurationPercentage;
    [SerializeField][Range(0f, 100f)] private float fadeOutDurationPercentage;
    private Color startColor;
    private float fadeIn;
    private float fadeOut;

    [Header("Debug")]
    [SerializeField] private Color fieldOutlineVisualizerColor;

    private void Start() {

        field.SetActive(false); // deactivate the field by default
        fieldSpriteRenderer = field.GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();

        fieldSpriteRenderer.color = new Color(fieldSpriteRenderer.color.r, fieldSpriteRenderer.color.g, fieldSpriteRenderer.color.b, 0f); // set field opacity to 0
        startColor = fieldSpriteRenderer.color; // store start color of field

        fadeIn = (fadeInDurationPercentage / 100f) * fieldDuration; // calculate fade in duration
        fadeOut = (fadeOutDurationPercentage / 100f) * fieldDuration; // calculate fade out duration

        if (fadeIn + fadeOut > fieldDuration)
            Debug.LogWarning("Field fade in and fade out durations exceed field duration.");

    }

    public void Activate() {

        if (fieldCoroutine != null) return; // return if field coroutine is active

        field.SetActive(true); // activate field
        if (fieldTweener != null && fieldTweener.IsActive()) fieldTweener.Kill(); // kill field tweener if it is active
        fieldCoroutine = StartCoroutine(HandleField());

    }

    private IEnumerator HandleField() {

        gameManager.SetCooldownsEnabled(false); // disable cooldowns

        fieldTweener = fieldSpriteRenderer.DOColor(targetFieldColor, fadeIn).SetEase(Ease.InCirc); // fade in field
        yield return new WaitForSeconds(fieldDuration - fadeOut); // wait till start of fade out
        fieldTweener = fieldSpriteRenderer.DOColor(startColor, fadeOut).SetEase(Ease.OutCirc); // fade out field
        yield return new WaitForSeconds(fadeOut); // wait for fade out to finish

        field.SetActive(false);
        gameManager.SetCooldownsEnabled(true); // re-enable cooldowns

        fieldCoroutine = null;

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
