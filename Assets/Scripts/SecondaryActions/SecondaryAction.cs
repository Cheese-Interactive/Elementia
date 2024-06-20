using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SecondaryAction : MonoBehaviour {

    [Header("References")]
    protected PlayerController player;

    [Header("Settings")]
    [SerializeField][Tooltip("Provide cooldown as well")] private bool isSecondaryAuto;
    [SerializeField][Tooltip("For auto attacks ONLY")] protected float secondaryCooldown;
    [SerializeField][Tooltip("No cooldown")] protected bool isSecondaryToggle;

    [Header("Actions")]
    protected bool isReady;

    protected void Awake() => player = GetComponent<PlayerController>();

    protected void Start() => isReady = true;

    public abstract void OnTrigger();

    public bool IsAuto() => isSecondaryAuto;

    public bool IsToggle() => isSecondaryToggle;

    public float GetCooldown() => secondaryCooldown;

    protected void ReadyAction() => isReady = true;

}
