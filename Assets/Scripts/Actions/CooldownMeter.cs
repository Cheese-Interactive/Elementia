using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CooldownMeter : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image image;
    private Tweener cooldownTweener;
    private bool isDestroyed;

    private void Start() => gameObject.SetActive(false); // disable the cooldown meter by default

    private void OnDestroy() {

        if (cooldownTweener != null && cooldownTweener.IsActive()) cooldownTweener.Kill(); // stop cooldown tweener if it is playing
        isDestroyed = true;

    }

    public void SetValue(Color color, float normalizedCooldown, float cooldownTimer) {

        if (isDestroyed) return; // do not set the cooldown meter if it is destroyed

        // if value is not a number, reset the cooldown meter
        if (float.IsNaN(normalizedCooldown)) {

            image.fillAmount = 0f;
            gameObject.SetActive(false); // disable the cooldown meter
            return;

        }

        image.color = color;
        image.fillAmount = normalizedCooldown;
        gameObject.SetActive(true); // enable the cooldown meter when value is set

        if (cooldownTweener != null && cooldownTweener.IsActive()) cooldownTweener.Kill(); // stop cooldown tweener if it exists
        cooldownTweener = image.DOFillAmount(0f, cooldownTimer).SetEase(Ease.Linear).OnComplete(() => gameObject.SetActive(false)); // tick the cooldown meter down and disable it when it reaches zero

    }
}
