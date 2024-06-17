using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageProjectile : MonoBehaviour {

    [Header("References")]
    private Rigidbody2D rb;

    [Header("Data")]
    private float damage;
    private float duration;

    public void Initialize(float damage, float speed, float duration, bool isFacingRight) {

        this.damage = damage;
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce((isFacingRight ? transform.right : -transform.right) * speed, ForceMode2D.Impulse);
        Destroy(gameObject, duration);

    }

    private void OnCollisionEnter(Collision collision) {

        if (collision.gameObject.TryGetComponent<Enemy>(out Enemy enemy)) {

            enemy.TakeDamage(damage);
            Destroy(gameObject);

        }
    }
}
