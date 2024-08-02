using UnityEngine;

public class Door : Interactable {

    [Header("References")]
    private Animator animator;

    private void Start() => animator = GetComponent<Animator>();

    public override void Interact() {

        print("Opening door...");

    }
}
