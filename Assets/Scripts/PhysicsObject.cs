using DG.Tweening;
using System.Collections;
using UnityEngine;

public abstract class PhysicsObject : MonoBehaviour {

    [Header("References")]
    [SerializeField] private LineRenderer beacon;
    [SerializeField] private Transform basePos;
    protected Rigidbody2D rb;
    private GameManager gameManager;

    [Header("Settings")]
    [SerializeField][Range(0f, 100f)] private float fadeInDurationPercentage;
    [SerializeField][Range(0f, 100f)] private float fadeOutDurationPercentage;
    [SerializeField] private float beaconStartWidth;
    [SerializeField] private float beaconEndWidth;
    [SerializeField] private float beaconHeight;
    private Coroutine resetCoroutine;
    private Tweener beaconTweener;
    private float resetDuration;

    protected void Start() {

        rb = GetComponent<Rigidbody2D>();

        gameManager = FindObjectOfType<GameManager>();

        beacon.gameObject.SetActive(false); // hide channel beacon by default
        beacon.startWidth = beaconStartWidth;
        beacon.endWidth = beaconEndWidth;

    }

    public void StartResetting(float resetDuration) {

        this.resetDuration = resetDuration;

        float fadeIn = (fadeInDurationPercentage / 100f) * resetDuration;
        float fadeOut = (fadeOutDurationPercentage / 100f) * resetDuration;

        if (fadeIn + fadeOut > resetDuration)
            Debug.LogWarning("Reset fade in and fade out durations exceed reset duration.");

        // show channel beacon
        beacon.gameObject.SetActive(true);
        beacon.SetPositions(new Vector3[] { basePos.position, basePos.position + (transform.up * beaconHeight) });

        // fade channel beacon in
        float channelFadeDuration = resetDuration * (fadeInDurationPercentage / 100f);
        Color color = beacon.material.GetColor("_Color");
        beaconTweener = DOVirtual.Float(0f, 1f, channelFadeDuration, (float alpha) => beacon.material.SetColor("_Color", new Color(color.r, color.g, color.b, alpha))).SetEase(Ease.InExpo);

        resetCoroutine = StartCoroutine(HandleReset());

    }

    private void StopResetting() {

        if (resetCoroutine != null) StopCoroutine(resetCoroutine); // stop channel coroutine if it is running
        resetCoroutine = null;

        if (beaconTweener != null && beaconTweener.IsActive()) beaconTweener.Kill(); // stop channel beacon tweener if it is running

        // fade channel beacon out
        float channelFadeDuration = resetDuration * (fadeOutDurationPercentage / 100f);
        Color color = beacon.material.GetColor("_Color");
        beaconTweener = DOVirtual.Float(1f, 0f, channelFadeDuration, (float alpha) => beacon.material.SetColor("_Color", new Color(color.r, color.g, color.b, alpha))).SetEase(Ease.OutCubic).OnComplete(() => beacon.gameObject.SetActive(false)); // fade out and hide channel beacon after fade out

    }

    private IEnumerator HandleReset() {

        yield return new WaitForSeconds(resetDuration); // wait for channel duration

        gameManager.ResetAllResettables(); // reset all resettables
        StopResetting(); // stop channeling

        resetCoroutine = null;

    }
}
