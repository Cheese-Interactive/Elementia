using UnityEngine;

public class OverlayParticles : MonoBehaviour {

    [Header("References")]
    private ParticleSystem overlayParticle;
    private Transform target;

    public void Initialize(Transform target) {

        this.target = target;

        overlayParticle = GetComponent<ParticleSystem>();
        ParticleSystem.MainModule main = overlayParticle.main;
        main.stopAction = ParticleSystemStopAction.None; // set stop action to none

    }

    private void Update() => transform.position = target.position;

    public void HideParticle() {

        ParticleSystem.MainModule main = overlayParticle.main;
        main.loop = false; // set loop to false so particle will stop emitting new particles
        main.stopAction = ParticleSystemStopAction.Destroy; // set stop action to destroy so particle will be destroyed after it stops (one last round of particles will be emitted)

    }
}
