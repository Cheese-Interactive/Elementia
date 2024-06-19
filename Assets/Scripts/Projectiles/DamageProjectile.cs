using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageProjectile : Projectile {

    private void OnCollisionEnter2D(Collision2D collision) {

        if (collision.collider.gameObject.TryGetComponent<SpellCollider>(out SpellCollider collider))
            collider.GetEntityController().TakeDamage(damage);

        Destroy(gameObject);

    }
}
