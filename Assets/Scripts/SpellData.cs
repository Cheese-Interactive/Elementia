using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spell Data")]
public class SpellData : ScriptableObject {

    [Header("Data")]
    [SerializeField] private Sprite spellIcon;
    [SerializeField] private Sprite hotbarSelectedIcon;

    public Sprite GetSpellIcon() => spellIcon;

    public Sprite GetHotbarSelectedIcon() => hotbarSelectedIcon;

}
