using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameManager")]
public class GameManager : ScriptableObject {

    [Header("Settings")]
    [SerializeField] private float standardMoveSpeed;

    public float GetStandardMoveSpeed() => standardMoveSpeed;

}
