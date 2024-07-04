using DG.Tweening;
using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class WindProjectile : BaseProjectile {

    [Header("References")]
    private Projectile projectile;
    private DamageOnTouch damageOnTouch;
    private TrailRenderer trailRenderer;

    [Header("Direction")]
    private Vector2 lastPos;

    [Header("Settings")]
    // min -> max -> min
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField][Range(0f, 100f)] private float totalSpeedTransitionPercentage;
    [SerializeField][Range(0f, 100f)] private float totalMinSpeedDurationPercentage;
    [SerializeField] private Vector2 entityWindForce;
    [SerializeField] private Vector2 objectWindForce;
    [SerializeField] private float windFadeDuration;
    private float lifetime;

    private void OnEnable() {

        projectile = GetComponent<Projectile>();
        damageOnTouch = GetComponent<DamageOnTouch>();
        trailRenderer = GetComponent<TrailRenderer>();

        lastPos = transform.position;
        lifetime = projectile.LifeTime;

        projectile.Speed = 0f; // set initial speed to 0
        projectile.Acceleration = 0f; // set initial acceleration to 0
        StartCoroutine(HandleSpeed());

        StartCoroutine(HandleFadeOut());

    }

    void Update() => lastPos = transform.position;

    private void OnTriggerEnter2D(Collider2D collision) { // triggers when projectile collides with something | IMPORTANT: triggers after the object is disabled on death

        if (collision.gameObject.activeInHierarchy && (damageOnTouch.TargetLayerMask & (1 << collision.gameObject.layer)) != 0) { // make sure hit object is active & is in target layer

            /* FORCE DEPENDS ON PROJECTILE VELOCITY DIRECTION */
            Vector2 entityForce = ((Vector2) transform.position - lastPos).normalized;
            Vector2 objectForce = entityForce;

            // handle entity force
            entityForce.x *= entityWindForce.x; // increase horizontal push force
            entityForce.y *= entityWindForce.y; // increase vertical push force
            collision.gameObject.GetComponent<CorgiController>()?.SetForce(entityForce); // push entity away from projectile

            // handle object force
            objectForce.x *= objectWindForce.x; // increase horizontal pull force
            objectForce.y *= objectWindForce.y; // increase vertical pull force
            collision.gameObject.GetComponent<Rigidbody2D>()?.AddForce(objectForce, ForceMode2D.Impulse); // push object away from projectile

        }
    }

    private IEnumerator HandleSpeed() {

        float totalMinSpeedDuration = lifetime * (totalMinSpeedDurationPercentage / 100f);
        float maxSpeedDuration = lifetime - totalMinSpeedDuration;
        float totalSpeedTransition = lifetime * (totalSpeedTransitionPercentage / 100f);

        /* MINIMUM SPEED */
        projectile.Speed = minSpeed; // set projectile initial speed to minimum speed
        yield return new WaitForSeconds((totalMinSpeedDuration / 2f) - (totalSpeedTransition / 4f)); ; // wait for half of minimum speed duration minus a fourth of total speed transition duration

        /* MAXIMUM SPEED */
        DOVirtual.Float(projectile.Speed, maxSpeed, totalSpeedTransition / 2f, (float value) => projectile.Speed = value); // increase projectile speed to maximum speed in half of total speed transition duration
        yield return new WaitForSeconds(maxSpeedDuration); // wait for the max speed duration plus half of speed transition duration

        /* MINIMUM SPEED */
        DOVirtual.Float(projectile.Speed, minSpeed, totalSpeedTransition / 2f, (float value) => projectile.Speed = value); // decrease projectile speed to minimum speed in half of total speed transition duration
        yield return new WaitForSeconds((totalMinSpeedDuration / 2f) + (totalSpeedTransition / 4f)); // wait for remaining half of minimum speed duration plus a fourth of speed transition duration

    }

    private IEnumerator HandleFadeOut() {

        yield return new WaitForSeconds(lifetime - windFadeDuration);
        Color color = trailRenderer.material.GetColor("_Color");
        DOVirtual.Float(1f, 0f, windFadeDuration, (float alpha) => trailRenderer.material.SetColor("_Color", new Color(color.r, color.g, color.b, alpha))); // fade out trail renderer material

    }
}
