using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Image fill;
    [SerializeField] private Sprite blankSpellIcon;
    private SpellData spellData;

    [Header("Settings")]
    [SerializeField] private float selectedScale;
    [SerializeField] private float scaleDuration;
    private Vector3 initialScale;
    private Tweener scaleTweener;

    private void Start() => initialScale = transform.localScale;

    public void Initialize(SpellData spellData) {

        this.spellData = spellData;

        fill.color = spellData.GetSpellColor();

    }

    public void SetSelected(bool isSelected) {

        if (scaleTweener != null && scaleTweener.IsActive()) scaleTweener.Kill(); // kill the previous scale tweener if it's still active

        scaleTweener = transform.DOScale(isSelected ? initialScale * selectedScale : initialScale, scaleDuration);

    }

    public Sprite GetPrimarySpellIcon() => spellData && spellData.GetPrimarySpellIcon() ? spellData.GetPrimarySpellIcon() : blankSpellIcon;

    public Sprite GetSecondarySpellIcon() => spellData && spellData.GetSecondarySpellIcon() ? spellData.GetSecondarySpellIcon() : blankSpellIcon;

}
