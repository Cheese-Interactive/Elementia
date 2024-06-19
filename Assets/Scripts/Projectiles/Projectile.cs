using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour {

    [Header("References")]
    protected Rigidbody2D rb;

    [Header("Data")]
    protected float damage;

    [Header("Impact")]
    [SerializeField] private GameObject impactEffectPrefab;

    public void Initialize(Collider2D shooterSpellCollider, float damage, float speed, float lifetime, bool isFacingRight) {

        this.damage = damage;
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(((Vector2) (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized * speed, ForceMode2D.Impulse);
        Destroy(gameObject, lifetime);

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), shooterSpellCollider); // ignore collision between projectile and shooter spell collider

    }

    private void Start() {

        rb = GetComponent<Rigidbody2D>();

    }

    private void OnDestroy() {

        Instantiate(impactEffectPrefab, transform.position, transform.rotation); // spawn impact effect whenever destroyed

    }
}
