using MoreMountains.CorgiEngine;
using UnityEngine;

public class BaseProjectile : MonoBehaviour {

    [Header("References")]
    protected Projectile projectile;
    protected DamageOnTouch damageOnTouch;
    private bool hasCollided;

    [Header("Direction")]
    protected Vector2 lastPos;

    [Header("Settings")]
    [SerializeField] private bool oneCollisionOnly;
    [Space]
    [SerializeField] private bool isPushProjectile;
    [SerializeField] private Vector2 entityPushForce;
    [SerializeField] private Vector2 objectPushForce;
    [Space]
    [SerializeField] private Vector2 entityPullForce;
    [SerializeField] private Vector2 objectPullForce;

    protected void OnEnable() => hasCollided = false; // reset has collided because object pooling cycles the same objects

    protected void Start() {

        projectile = GetComponent<Projectile>();
        damageOnTouch = GetComponent<DamageOnTouch>();

        lastPos = transform.position;

    }

    protected void Update() => lastPos = transform.position;

    protected void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0 && !hasCollided) { // make sure hit object is active, is in target layer, and has not collided yet

            if (isPushProjectile) { // FORCE DEPENDS ON PROJECTILE VELOCITY DIRECTION (PUSH)

                Vector2 entityForce = ((Vector2) transform.position - lastPos).normalized; // get force direction (vector faces projectile velocity direction)
                Vector2 objectForce = entityForce;

                // handle entity force
                entityForce.x *= entityPushForce.x; // increase horizontal push force
                entityForce.y *= entityPushForce.y; // increase vertical push force
                collision.gameObject.GetComponent<CorgiController>()?.SetForce(entityForce); // push entity away from projectile

                // handle object force
                objectForce.x *= objectPushForce.x; // increase horizontal pull force
                objectForce.y *= objectPushForce.y; // increase vertical pull force
                collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(objectForce, ForceMode2D.Impulse); // push object away from projectile

            } else { // FORCE DIRECTION DEPENDS ON SHOOTER POSITION (PULL)

                Vector2 entityForce = (projectile.GetOwner().transform.position - collision.transform.position).normalized; // get force direction (vector faces shooter)
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

            if (oneCollisionOnly) // if projectile can only collide once
                hasCollided = true; // set collided to true

        }
    }
}
