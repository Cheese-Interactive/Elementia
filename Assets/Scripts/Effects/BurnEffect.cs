using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : MonoBehaviour {

    // IMPORTANT: actual burn effect is implemented with Corgi Engine's DamageOnTouch script
    [Header("Overlay")]
    [SerializeField] private Overlay burnOverlay;

    private void Start() => burnOverlay.HideOverlay(); // hide burn overlay by default


    public void Burn(float duration) {

        burnOverlay.ShowOverlay(); // show burn overlay
        StartCoroutine(BurnReset(duration));

    }

    private IEnumerator BurnReset(float duration) {

        yield return new WaitForSeconds(duration);
        burnOverlay.HideOverlay(); // show burn overlay

    }
}
