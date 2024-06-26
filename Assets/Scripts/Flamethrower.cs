using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamethrower : MonoBehaviour {

    [Header("Physics")]
    [SerializeField] private float force;

    private void OnTriggerEnter2D(Collider2D collision) => collision.GetComponent<Rigidbody2D>()?.AddForce(Vector2.right * force, ForceMode2D.Impulse); // for physics effect

}
