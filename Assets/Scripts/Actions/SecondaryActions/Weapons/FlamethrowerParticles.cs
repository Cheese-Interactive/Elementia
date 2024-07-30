using MoreMountains.CorgiEngine;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerParticles : MonoBehaviour {

    [Header("References")]
    private ParticleSystem ps;

    public void Initialize(HitscanWeapon weapon, float flameSpeed) {

        ps = GetComponent<ParticleSystem>();

        var main = ps.main;
        main.startSpeed = flameSpeed; // set particle speed
        main.startLifetime = weapon.HitscanMaxDistance / flameSpeed; // set particle lifetime

    }

    private void OnParticleTrigger() {

        List<ParticleSystem.Particle> outsideList = new List<ParticleSystem.Particle>();
        int numOutside = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Outside, outsideList);

        // destroy particles outside of trigger
        for (int i = 0; i < numOutside; i++) {

            ParticleSystem.Particle p = outsideList[i];
            p.remainingLifetime = 0f;
            outsideList[i] = p;

        }

        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Outside, outsideList);

        List<ParticleSystem.Particle> exitList = new List<ParticleSystem.Particle>();
        int numExit = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exitList);

        // destroy particles on exit
        for (int i = 0; i < numExit; i++) {

            ParticleSystem.Particle p = exitList[i];
            p.remainingLifetime = 0f;
            exitList[i] = p;

        }

        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, exitList);

    }
}
