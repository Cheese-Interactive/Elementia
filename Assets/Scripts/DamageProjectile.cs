using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageProjectile : MonoBehaviour {

    [Header("References")]
    [SerializeField] private GameObject impactEffectPrefab;
    private Rigidbody2D rb;

    [Header("Data")]
    private float damage;

    public void Initialize(Collider2D shooterSpellCollider, float damage, float speed, float duration, bool isFacingRight) {

        this.damage = damage;
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce((isFacingRight ? transform.right : -transform.right) * speed, ForceMode2D.Impulse);
        Destroy(gameObject, duration);

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooterSpellCollider); // ignore collision between projectile and shooter spell collider

    }

    private void OnCollisionEnter2D(Collision2D collision) {

        if (collision.collider.gameObject.TryGetComponent<SpellCollider>(out SpellCollider collider))
            collider.GetEntityController().TakeDamage(damage);

        Instantiate(impactEffectPrefab, transform.position, transform.rotation);
        Destroy(gameObject);

    }
}
