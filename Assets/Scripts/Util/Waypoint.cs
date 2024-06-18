using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

    [Header("Data")]
    [SerializeField][Tooltip("Time to travel TO the waypoint")] private float travelDuration;
    [SerializeField][Tooltip("Time to wait at the waypoint")] private float waitDuration;
    private Vector3 startPos;

    private void Start() {

        startPos = transform.position;

    }

    private void Update() {

        transform.position = startPos; // maintain original position

    }

    public Vector3 GetStartPosition() => startPos;

    public float GetTravelDuration() => travelDuration;

    public float GetWaitDuration() => waitDuration;

}
