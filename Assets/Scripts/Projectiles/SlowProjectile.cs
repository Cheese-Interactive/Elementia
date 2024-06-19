using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowProjectile : DamageProjectile {

    private void OnCollisionEnter2D(Collision2D collision) {

        if (collision.collider.gameObject.TryGetComponent<SpellCollider>(out SpellCollider collider)) {

            // TODO: slow enemy
            collider.GetEntityController().TakeDamage(damage);

        }

        Destroy(gameObject);

    }
}
