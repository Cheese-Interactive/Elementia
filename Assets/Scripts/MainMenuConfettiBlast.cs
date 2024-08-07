using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainMenuConfettiBlast : MonoBehaviour {

    [Header("References")]
    private ParticleSystem particles;
    private new Light2D light;

    [Header("Light Customization")]
    [SerializeField] private float maxLightBrightness;
    [SerializeField] private float lightDuration;
    [SerializeField][Range(0f, 1f)] private float lightFadeInDurMult;


    // Start is called before the first frame update
    void Start() {
        particles = GetComponent<ParticleSystem>();
        light = GetComponent<Light2D>();
        StartCoroutine(PlayConfetti());
    }

    private IEnumerator PlayConfetti() {
        light.intensity = 0; //off just in case
        particles.Play(); //yay
        float fadeInDuration = lightDuration * lightFadeInDurMult;
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
