using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityController : MonoBehaviour {

    [Header("References")]
    [SerializeField] protected SpellCollider spellCollider;
    [SerializeField] protected Transform leftFoot;
    [SerializeField] protected Transform rightFoot;
    protected Rigidbody2D rb;
    protected Animator anim;

    [Header("Movement")]
    protected bool isFacingRight;

    [Header("Health")]
    [SerializeField] private float maxHealth;
    [SerializeField] private ParticleSystem deathExplosionPrefab;
    private float currHealth;

    protected void Awake() {

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        currHealth = maxHealth; // set current health to max health

        isFacingRight = true;

    }

    #region HEALTH

    public void TakeDamage(float damage) {

        currHealth -= damage;

        if (currHealth <= 0f)
            Die();

    }

    public void Die() {

        Destroy(gameObject);
        Instantiate(deathExplosionPrefab, transform.position, transform.rotation);

    }

    #endregion

    #region UTILITIES

    public SpellCollider GetSpellCollider() => spellCollider;

    #endregion
}
