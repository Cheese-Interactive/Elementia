using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InteractIndicator : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image interactIcon;
    private CanvasGroup canvasGroup;
    private Tweener fadeTweener;
    private Tweener scaleTweener;

    [Header("Settings")]
    [SerializeField] private float fadeDuration;
    [SerializeField] private float targetScale;
    [SerializeField] private float scaleDuration;

    public void Initialize() {

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.gameObject.SetActive(false); // hide interact indicator by default

    }

    private void OnDestroy() {

        if (fadeTweener != null && fadeTweener.IsActive()) fadeTweener.Kill(); // kill previous tween if it exists
        if (scaleTweener != null && scaleTweener.IsActive()) scaleTweener.Kill(); // kill previous tween if it exists

    }

    public void Show() {

        canvasGroup.gameObject.SetActive(true); // show interact indicator

        if (fadeTweener != null && fadeTweener.IsActive()) fadeTweener.Kill(); // kill previous tween if it exists
        fadeTweener = canvasGroup.DOFade(1f, fadeDuration); // fade interact indicator in

    }

    public void Hide() {

        if (scaleTweener != null && scaleTweener.IsActive()) return; // if interact indicator is scaling, return because we don't want to hide it yet

        if (fadeTweener != null && fadeTweener.IsActive()) fadeTweener.Kill(); // kill previous tween if it exists
        fadeTweener = canvasGroup.DOFade(0f, fadeDuration).OnComplete(() => canvasGroup.gameObject.SetActive(false)); // fade interact indicator out and hide it when done

    }

    public void OnInteract() {

        if (scaleTweener != null && scaleTweener.IsActive()) scaleTweener.Kill(); // kill previous tween if it exists
        scaleTweener = interactIcon.transform.DOScale(targetScale, scaleDuration).SetLoops(2, LoopType.Yoyo).OnComplete(() => Hide()); // scale interact indicator up and down then hide it

    }
}
