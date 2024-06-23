using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    [Header("References")]
    private SlowEffect slowEffect;

    [Header("Mechanics")]
    private Health health;

    private void Start() {

        slowEffect = GetComponent<SlowEffect>();

        health = GetComponent<Health>();

        health.OnDeath += slowEffect.RemoveEffect; // remove slow effect on death

    }

    private void OnDisable() {

        health.OnDeath -= slowEffect.RemoveEffect; // remove slow effect on death

    }

    private void OnDestroy() {

        health.OnDeath -= slowEffect.RemoveEffect; // remove slow effect on death

    }
}
