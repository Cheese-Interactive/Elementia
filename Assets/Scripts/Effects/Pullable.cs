using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pullable : MonoBehaviour {

    [Header("References")]
    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void Start() {

        if (rb.bodyType != RigidbodyType2D.Dynamic)
            Debug.LogWarning("Pullable object has a non-dynamic Rigidbody2D component. This will prevent the object from being pulled.");

    }

    public void Pull(Transform target, float totalPullMultiplier, float verticalPullMultiplier) {

        Vector2 force = (target.position - transform.position) * totalPullMultiplier;
        force.y *= verticalPullMultiplier; // increase vertical pull force
        rb.AddForce(force, ForceMode2D.Impulse);

    }
}
