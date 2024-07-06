using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Meter : MonoBehaviour {

    [Header("Reference")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image slot;
    [SerializeField] private Image slotFill;
    private Tweener tweener;

    public void Initialize(float duration, Sprite icon, Color color) {

        slider.maxValue = duration; // set the slider max value to the duration
        slider.value = duration; // set the slider value to the max value
        slider.fillRect.GetComponent<Image>().color = color; // set the slider fill color

        slot.color = color; // set the slot color
        slotFill.sprite = icon; // set the slot fill icon

        tweener = DOVirtual.Float(duration, 0f, duration, (value) => slider.value = value).SetEase(Ease.Linear).OnComplete(() => Destroy(gameObject)); // tick down slider value over duration and destroy meter when complete

    }

    private void OnDestroy() {

        if (tweener != null && tweener.IsActive()) tweener.Kill(); // kill tweener if it exists

    }
}
