using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Meter : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image slot;
    [SerializeField] private Image slotFill;
    private Tweener meterTweener;
    private bool isDestroyed;

    public void Initialize(float duration, Sprite icon, Color color) {

        if (isDestroyed) return; // return if the meter is destroyed

        slider.maxValue = duration; // set the slider max value to the duration
        slider.value = duration; // set the slider value to the max value
        slider.fillRect.GetComponent<Image>().color = color; // set the slider fill color

        slot.color = color; // set the slot color
        slotFill.sprite = icon; // set the slot fill icon

        meterTweener = DOVirtual.Float(duration, 0f, duration, (value) => slider.value = value).SetEase(Ease.Linear).OnComplete(() => {

            Destroy(gameObject);
            isDestroyed = true;

        }); // tick down slider value over duration and destroy meter when complete
    }

    private void OnDestroy() {

        if (meterTweener != null && meterTweener.IsActive()) meterTweener.Kill(); // kill tweener if it exists

    }
}
