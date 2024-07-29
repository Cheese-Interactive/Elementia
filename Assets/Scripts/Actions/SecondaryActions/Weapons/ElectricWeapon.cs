using DigitalRuby.LightningBolt;
using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class ElectricWeapon : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Lightning lightningPrefab;
    private HitscanWeapon weapon;

    [Header("Settings")]
    private float lightningDuration;
    private bool hasDistanceMultiplier;
    private bool isPushProjectile;
    private Vector2 entityForceMultiplier;
    private Vector2 objectForceMultiplier;

    public void Initialize(float lightningDuration, bool hasDistanceMultiplier, bool isPushProjectile, Vector2 entityForceMultiplier, Vector2 objectForceMultiplier) {

        this.lightningDuration = lightningDuration;
        this.hasDistanceMultiplier = hasDistanceMultiplier;
        this.isPushProjectile = isPushProjectile;
        this.entityForceMultiplier = entityForceMultiplier;
        this.objectForceMultiplier = objectForceMultiplier;

        weapon = GetComponent<HitscanWeapon>();
        weapon.OnHit += OnHit;

    }

    public void OnHit(GameObject hitObject, Vector3 hitPoint) => StartCoroutine(HandleHit(hitObject, hitPoint));

    private IEnumerator HandleHit(GameObject hitObject, Vector3 hitPoint) {

        if (hitObject && hitObject.activeInHierarchy) { // make sure hit object exists and is active

            if ((weapon.HitscanTargetLayers & (1 << hitObject.layer)) != 0) { // make sure hit object is in target layer

                Vector2 entityForce;
                Vector2 lightningSpawn = (Vector2) transform.TransformPoint(transform.localPosition + weapon.ProjectileSpawnOffset); // get lightning spawn position

                // FORCE DIRECTIONS DEPEND ON LIGHTNING SPAWN LOCATION
                if (isPushProjectile) // push effect
                    entityForce = ((Vector2) hitPoint - lightningSpawn).normalized; // get force direction (vector faces lightning spawn location)
                else // pull effect
                    entityForce = (lightningSpawn - (Vector2) hitPoint).normalized; // get force direction (vector faces collision point)

                Vector2 objectForce = entityForce;

                if (hasDistanceMultiplier) { // if distance multiplier is enabled

                    float distance = Vector2.Distance(lightningSpawn, hitPoint); // get distance between lightning spawn position and collision point
                    entityForce *= distance; // apply distance multiplier to entity force
                    objectForce *= distance; // apply distance multiplier to object force

                }

                entityForce *= entityForceMultiplier; // apply multiplier to entity force
                objectForce *= objectForceMultiplier; // apply multiplier to object force

                hitObject.GetComponent<CorgiController>()?.SetForce(entityForce); // add force to entity

                Rigidbody2D objectRb = hitObject.GetComponent<Rigidbody2D>();

                if (objectRb) // make sure rigidbody exists
                    objectRb.AddForce(objectForce, ForceMode2D.Impulse); // add force to object

            }

            hitObject.GetComponent<Generator>()?.Activate(); // activate generator on hit object if it exists

        }

        Lightning lightning = Instantiate(lightningPrefab, transform); // instantiate lightning on player position (triggers automatically)
        lightning.transform.localPosition = Vector2.zero; // set local position to zero to allow proper positioning for start and end objects
        lightning.StartObject.transform.localPosition = weapon.ProjectileSpawnOffset; // set local position to projectile offset
        lightning.EndObject.transform.position = hitPoint; // set global position to hit point
        lightning.Trigger(); // trigger lightning

        yield return new WaitForSeconds(lightningDuration); // wait for lightning duration

        Destroy(lightning.gameObject); // destroy lightning

    }
}
