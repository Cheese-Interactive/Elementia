using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    [Header("Health")]
    [SerializeField] private float maxHealth;
    private float currHealth;

    private void Start() {

        currHealth = maxHealth; // set current health to max health

    }

    public void TakeDamage(float damage) {

        currHealth -= damage;

        if (currHealth <= 0f)
            Die();

    }

    public void Die() {

        Destroy(gameObject);
        // TODO: death stuff

    }
}
