using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spell Data")]
public class SpellData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private Sprite primarySpellIcon;
    [SerializeField] private Sprite secondarySpellIcon;
    [SerializeField] private Color spellColor;

    public Sprite GetPrimarySpellIcon() => primarySpellIcon;

    public Sprite GetSecondarySpellIcon() => secondarySpellIcon;

    public Color GetSpellColor() => spellColor;

}
