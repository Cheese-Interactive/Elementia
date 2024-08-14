using UnityEngine;

public class Overlay : MonoBehaviour {

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private OverlayParticles particlesPrefab;
    private OverlayParticles currParticles;
    private SpriteMask spriteMask;

    [Header("Settings")]
    [SerializeField][Tooltip("If true, particles will be destroyed immediately on hide. If false, particles will finish their current loop then be destroyed on hide.")] private bool destroyParticlesOnHide;

    private void Start() => spriteMask = GetComponent<SpriteMask>();

    private void Update() {

        spriteMask.sprite = spriteRenderer.sprite;

        // deal with flipped player (gets flipped on sprite renderer)
        if (spriteRenderer.flipX)
            transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // flip overlay
        else
            transform.localRotation = Quaternion.identity; // reset overlay rotation

    }

    public void ShowOverlay() {

        gameObject.SetActive(true);

        if (!currParticles) { // make sure there no current particles

            currParticles = Instantiate(particlesPrefab, transform.position, Quaternion.identity);
            currParticles.Initialize(transform);

        }
    }

    public void HideOverlay() {

        gameObject.SetActive(false);

        // hide particles if they exist
        if (currParticles)
            currParticles.HideParticles(destroyParticlesOnHide);

    }
}
