using UnityEngine;
using UnityEngine.UI;

public class ItemSelectionIndicator : MonoBehaviour {
    [SerializeField] private Sprite defaultIndicator;
    [SerializeField] private Sprite disabledIndicator;
    [SerializeField] private Sprite selectedIndicator;
    private Image image;
    void Start() {
        image = GetComponent<Image>();
        image.sprite = disabledIndicator;
    }

    public void SetSelected(bool enabled) {
        if (enabled)
            image.sprite = selectedIndicator;
        else image.sprite = defaultIndicator;
    }



}
