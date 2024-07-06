using DG.Tweening;
using MoreMountains.CorgiEngine;
using System.Collections;
using UnityEngine;

public class WindProjectile : BaseProjectile {

    [Header("References")]
    private TrailRenderer trailRenderer;

    [Header("Settings")]
    // min -> max -> min
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField][Range(0f, 100f)] private float totalSpeedTransitionPercentage;
    [SerializeField][Range(0f, 100f)] private float totalMinSpeedDurationPercentage;
    [SerializeField] private float windFadeDuration;
    private float lifetime;

    private void OnEnable() {

        projectile = GetComponent<Projectile>();
        trailRenderer = GetComponent<TrailRenderer>();

        lifetime = projectile.LifeTime;

        projectile.Speed = 0f; // set initial speed to 0
        projectile.Acceleration = 0f; // set initial acceleration to 0
        StartCoroutine(HandleSpeed());

        StartCoroutine(HandleFadeOut());

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
