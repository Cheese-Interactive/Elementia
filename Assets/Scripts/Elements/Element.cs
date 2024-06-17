using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Element : MonoBehaviour {

    [Header("References")]
    protected PlayerController player;

    protected void Awake() {

        player = GetComponent<PlayerController>();

    }

    public abstract void PrimaryAction();

    public abstract void SecondaryAction();

}
