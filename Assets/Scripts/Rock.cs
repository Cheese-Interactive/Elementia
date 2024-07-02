using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour {

    [Header("References")]
    [SerializeField] private Transform rockBody;

    private void Start() => rockBody.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

}
