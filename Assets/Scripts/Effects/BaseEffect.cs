using UnityEngine;

public abstract class BaseEffect : MonoBehaviour {

    [Header("References")]
    [SerializeField] protected Overlay overlay;
    protected Coroutine resetEffectCoroutine;

    public abstract void RemoveEffect();

}
