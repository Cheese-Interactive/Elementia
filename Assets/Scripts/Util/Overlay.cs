using UnityEngine;

public class Overlay : MonoBehaviour {

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private SpriteMask spriteMask;

    private void Start() => spriteMask = GetComponent<SpriteMask>();

    private void Update() {

        spriteMask.sprite = spriteRenderer.sprite;

        // deal with flipped player (gets flipped on sprite renderer)
        if (spriteRenderer.flipX)
            transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // flip overlay
        else
            transform.localRotation = Quaternion.identity; // reset overlay rotation

    }

    public void ShowOverlay() => gameObject.SetActive(true);

    public void HideOverlay() => gameObject.SetActive(false);

}
