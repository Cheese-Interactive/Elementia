using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Element : MonoBehaviour {

    [Header("References")]
    protected PlayerController player;

    [Header("Settings")]
    [SerializeField][Tooltip("Provide cooldown as well")] private bool isPrimaryAuto;
    [SerializeField][Tooltip("For primary auto attacks ONLY")] protected float primaryCooldown;
    [SerializeField][Tooltip("No cooldown")] protected bool isPrimaryToggle;
    [SerializeField][Tooltip("Provide cooldown as well")] private bool isSecondaryAuto;
    [SerializeField][Tooltip("For secondary auto attacks ONLY")] protected float secondaryCooldown;
    [SerializeField][Tooltip("No cooldown")] protected bool isSecondaryToggle;

    protected void Awake() {

        player = GetComponent<PlayerController>();

    }

    public abstract void PrimaryAction();

    public abstract void SecondaryAction();

    public bool IsPrimaryAuto() => isPrimaryAuto;

    public bool IsPrimaryToggle() => isPrimaryToggle;

    public float GetPrimaryCooldown() => primaryCooldown;

    public bool IsSecondaryAuto() => isSecondaryAuto;

    public bool IsSecondaryToggle() => isSecondaryToggle;

    public float GetSecondaryCooldown() => secondaryCooldown;

}
