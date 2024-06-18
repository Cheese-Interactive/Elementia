using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCollider : MonoBehaviour {

    [Header("References")]
    [SerializeField] private EntityController entityController;

    public EntityController GetEntityController() => entityController;

}
