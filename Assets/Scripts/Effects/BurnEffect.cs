using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : MonoBehaviour {

    // IMPORTANT: actual burn effect is implemented with Corgi Engine's DamageOnTouch script

    [Header("Overlay")]
    [SerializeField] private Overlay overlay;

    private void Start() {

        overlay.gameObject.SetActive(false); // deactivate burn overlay by default

    }

    public void Burn(float duration) {

        overlay.gameObject.SetActive(true); // activate burn overlay

        StartCoroutine(BurnReset(duration));

    }

    private IEnumerator BurnReset(float duration) {

        yield return new WaitForSeconds(duration);
        overlay.gameObject.SetActive(false); // deactivate burn overlay

    }
}
