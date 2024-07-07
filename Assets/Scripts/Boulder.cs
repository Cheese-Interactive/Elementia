using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour {

    [Header("References")]
    private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float damage;
    [Space]
    [SerializeField] private bool isPushForce;
    [SerializeField] private Vector2 entityPushForce;
    [SerializeField] private Vector2 objectPushForce;
    [Space]
    [SerializeField] private Vector2 entityPullForce;
    [SerializeField] private Vector2 objectPullForce;

    private void Start() => rb = GetComponent<Rigidbody2D>();

    private void OnTriggerEnter2D(Collider2D collision) {

        if (collision.gameObject.activeInHierarchy && rb.velocity != Vector2.zero) { // make sure hit object is active and boulder is moving

            collision.gameObject.GetComponent<Health>()?.Damage(damage, gameObject, 0.2f, 0.2f, rb.velocity.normalized);

            if (isPushForce) { // FORCE DEPENDS ON BOULDER VELOCITY DIRECTION (PUSH)

                Vector2 entityForce = rb.velocity.normalized; // get force direction (vector faces boulder velocity direction)
                Vector2 objectForce = entityForce;

                // handle entity force
                entityForce.x *= entityPushForce.x; // increase horizontal push force
                entityForce.y *= entityPushForce.y; // increase vertical push force
                collision.gameObject.GetComponent<CorgiController>()?.SetForce(entityForce); // push entity away from boulder

                // handle object force
                objectForce.x *= objectPushForce.x; // increase horizontal pull force
                objectForce.y *= objectPushForce.y; // increase vertical pull force
                collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(objectForce, ForceMode2D.Impulse); // push object away from boulder

            } else { // FORCE DIRECTION DEPENDS ON BOULDER POSITION (PULL)

                Vector2 entityForce = (transform.position - collision.transform.position).normalized; // get force direction (vector faces boulder)
                Vector2 objectForce = entityForce;

                // handle entity force
                entityForce.x *= entityPullForce.x; // increase horizontal push force
                entityForce.y *= entityPullForce.y; // increase vertical push force
                collision.gameObject.GetComponent<CorgiController>()?.SetForce(entityForce); // pull entity towards shooter

                // handle object force
                objectForce.x *= objectPullForce.x; // increase horizontal pull force
                objectForce.y *= objectPullForce.y; // increase vertical pull force
                collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(objectForce, ForceMode2D.Impulse); // pull object towards shooter

            }
        }
    }
}
