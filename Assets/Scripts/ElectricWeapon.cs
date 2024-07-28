using DigitalRuby.LightningBolt;
using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class ElectricWeapon : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Lightning lightning;
    private HitscanWeapon weapon;

    [Header("Settings")]
    [SerializeField] private float lightningDuration;
    [Space]
    [SerializeField] protected bool hasDistanceMultiplier;
    [Space]
    [SerializeField] protected bool isPushProjectile;
    [SerializeField] private Vector2 entityForceMultiplier;
    [SerializeField] private Vector2 objectForceMultiplier;

    private void Start() {

        weapon = GetComponent<HitscanWeapon>();
        lightning.gameObject.SetActive(false); // disable lightning by default

        weapon.OnHit += OnHit;

    }

    public void OnHit(GameObject hitObject, Vector3 hitPoint) => StartCoroutine(HandleHit(hitObject, hitPoint));

    private IEnumerator HandleHit(GameObject hitObject, Vector3 hitPoint) {

        if (hitObject && hitObject.activeInHierarchy && (weapon.HitscanTargetLayers & (1 << hitObject.layer)) != 0) { // make sure hit object exists, is active, & is in target layer

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

        lightning.StartObject.transform.localPosition = weapon.ProjectileSpawnOffset; // set local position to projectile offset
        lightning.EndObject.transform.position = hitPoint; // set global position to hit point
        lightning.gameObject.SetActive(true); // enable lightning (triggers automatically)

        yield return new WaitForSeconds(lightningDuration); // wait for lightning duration

        lightning.Cancel(); // cancel lightning
        lightning.gameObject.SetActive(false); // disable lightning

    }
}
