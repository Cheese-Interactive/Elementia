using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Animator anim;
    private Rigidbody2D rb;

    [Header("Drop")]
    private bool destroyOnCollision;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void OnTriggerEnter2D(Collider2D collision) {

        Debug.LogWarning("Fix rock collisions");
        if (destroyOnCollision) Destroy(gameObject);

    }

    public void Drop() {

        anim.enabled = false; // stop all animations so object can fall
        SetRigidbodyKinematic(false);
        destroyOnCollision = true;

    }

    public void SetRigidbodyKinematic(bool isKinematic) => rb.bodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

}
