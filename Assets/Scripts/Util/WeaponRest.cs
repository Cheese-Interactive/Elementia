using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRest : MonoBehaviour {

    [Header("Rest")]
    [SerializeField] private Vector3 restRotation;

    public Vector3 GetRestRotation() => restRotation;

}
