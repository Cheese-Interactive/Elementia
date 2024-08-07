using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainMenuConfettiBlast : MonoBehaviour {

    [Header("References")]
    private ParticleSystem particles;
    private new Light2D light;

    [Header("Settings")]
    [SerializeField] private float maxLightBrightness;
    [SerializeField] private float lightDuration;
    [SerializeField][Range(0f, 1f)] private float lightFadeInDurationMultiplier;

    private void Start() {

        particles = GetComponent<ParticleSystem>();
        light = GetComponent<Light2D>();
        StartCoroutine(PlayConfetti());

    }

    private IEnumerator PlayConfetti() {

        light.intensity = 0;
        particles.Play();

        float fadeInDuration = lightDuration * lightFadeInDurationMultiplier;
        float fadeOutDuration = lightDuration - fadeInDuration;

        StartCoroutine(LerpLightIntensityTo(maxLightBrightness, fadeInDuration));

        yield return new WaitForSeconds(fadeInDuration);

        StartCoroutine(LerpLightIntensityTo(0, fadeOutDuration));

        yield return new WaitForSeconds(fadeOutDuration);
        yield return new WaitForSeconds(particles.main.startLifetime.constant);

        Destroy(gameObject);

    }

    private IEnumerator LerpLightIntensityTo(float target, float duration) {

        float start = light.intensity;
        float elapsed = 0f;
        float time;

        while (elapsed < duration) {

            time = elapsed / duration;
            time = time * time * (3 - 2 * time);
            light.intensity = Mathf.Lerp(start, target, time);
            elapsed += Time.deltaTime;
            yield return null;

        }

        light.intensity = target;

    }
}
