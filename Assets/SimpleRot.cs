using UnityEngine;

public class SimpleRot : MonoBehaviour {
    [SerializeField] private float rotAmount;
    void Update() {
        transform.Rotate(0, 0, rotAmount);
    }
}
