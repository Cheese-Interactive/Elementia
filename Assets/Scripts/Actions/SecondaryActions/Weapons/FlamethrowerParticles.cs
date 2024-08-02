using MoreMountains.CorgiEngine;
using UnityEngine;

public class FlamethrowerParticles : MonoBehaviour {

    [Header("References")]
    private ParticleSystem ps;

    public void Initialize(HitscanWeapon weapon, float flameSpeed) {

        ps = GetComponent<ParticleSystem>();

        ParticleSystem.MainModule main = ps.main;
        main.startSpeed = flameSpeed; // set particle speed
        main.startLifetime = weapon.HitscanMaxDistance / flameSpeed; // set particle lifetime

    }
}
