using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamethrower : MonoBehaviour {

    [Header("Physics")]
    [SerializeField] private float objectFlamethrowerForce;

    private void OnTriggerStay2D(Collider2D collision) { // on trigger stay to keep pushing object because flamethrower is a constant stream

        if (collision.gameObject.activeInHierarchy) // make sure hit object is active
            collision.GetComponent<Rigidbody2D>()?.AddForce(transform.right * objectFlamethrowerForce, ForceMode2D.Impulse); // for physics effect

    }

    public void SetFlamethrowerForce(float force) => objectFlamethrowerForce = force;

}
