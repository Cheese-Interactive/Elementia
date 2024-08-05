using MoreMountains.CorgiEngine;
using UnityEngine;

public class Flamethrower : MonoBehaviour {

    [Header("References")]
    [SerializeField] private BoxCollider2D rangeCollider;
    [SerializeField] private FlamethrowerParticles particles;
    private Character character;
    private HitscanWeapon weapon;

    [Header("Settings")]
    private Vector2 objectFlamethrowerForce;
    private Vector2 entityFlamethrowerForce;
    private float burnDamage;
    private int burnTicks;
    private float burnDuration;

    public void Initialize(float flameSpeed, Vector2 entityFlamethrowerForce, Vector2 objectFlamethrowerForce, float burnDamage, int burnTicks, float burnDuration) {

        character = FindObjectOfType<PlayerController>().GetComponent<Character>();
        weapon = GetComponent<HitscanWeapon>();

        this.objectFlamethrowerForce = objectFlamethrowerForce;
        this.entityFlamethrowerForce = entityFlamethrowerForce;
        this.burnDamage = burnDamage;
        this.burnTicks = burnTicks;
        this.burnDuration = burnDuration;

        particles.Initialize(weapon, flameSpeed); // initialize particles

        // set range collider positioning and size
        rangeCollider.transform.localPosition = new Vector2(weapon.ProjectileSpawnOffset.x, 0f); // set collider local position x axis to weapon spawn offset x axis
        rangeCollider.size = new Vector2(weapon.HitscanMaxDistance, 1f);
        rangeCollider.offset = new Vector2(weapon.HitscanMaxDistance / 2f, 0f);

    }

    private void OnTriggerStay2D(Collider2D collision) { // use on trigger stay to keep pushing object because flamethrower is a constant stream

        if (collision.gameObject.activeInHierarchy && (weapon.HitscanTargetLayers & (1 << collision.gameObject.layer)) != 0) { // make sure hit object is active & is in target layer

            /* FORCE DIRECTION DEPENDS ON SHOOTER POSITION */
            Vector2 entityForce = (collision.transform.position - weapon.Owner.transform.position).normalized;  // get force direction (vector faces collision object)
            Vector2 objectForce = entityForce;

            // handle entity force
            entityForce.x *= entityFlamethrowerForce.x; // increase horizontal push force
            entityForce.y *= entityFlamethrowerForce.y; // increase vertical push force
            collision.gameObject.GetComponent<CorgiController>()?.AddForce(entityForce); // push entity away from shooter

            // handle object force
            objectForce.x *= objectFlamethrowerForce.x; // increase horizontal pull force
            objectForce.y *= objectFlamethrowerForce.y; // increase vertical pull force

            Rigidbody2D objectRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (objectRb) // make sure rigidbody exists
                objectRb.AddForce(objectForce, ForceMode2D.Impulse); // push object away from shooter

            collision.gameObject.GetComponent<BurnEffect>()?.AddEffect(gameObject, burnDamage, burnTicks, burnDuration, weapon.DamageCausedInvincibilityDuration, character.IsFacingRight ? transform.right : -transform.right, true); // apply burn effect to object (get direction flamethrower is facing through character facing direction)

            collision.gameObject.GetComponent<BurnableObject>()?.StartBurn(); // burn object if it can be burned

        }
    }

    private void OnTriggerExit2D(Collider2D collision) => collision.gameObject.GetComponent<BurnableObject>()?.StopBurn(); // stop object burn if it can be burned

}
