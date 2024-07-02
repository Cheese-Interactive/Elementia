using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Projectile projectile;
    [SerializeField] private Transform body;

    private void Start() => body.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360)); // randomize rock rotation on z-axis

    public Projectile GetProjectile() => projectile;

}
