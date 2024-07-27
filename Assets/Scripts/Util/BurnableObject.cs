using DG.Tweening;
using UnityEngine;

public class BurnableObject : MonoBehaviour {

    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private Tweener burnTweener;
    private Tweener resetTweener;

    [Header("Settings")]
    [SerializeField] private Gradient burnGradient; // gradient is used to track progress numerically
    [SerializeField][Tooltip("Amount of time this object can withstand flames")] private float burnResistanceDuration;
    [SerializeField] private float resetDuration;
    private float currGradientProgress;
    private bool isDestroyed; // destroyed flag to prevent tweens from running after object is destroyed

    private void Start() {

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer.color != spriteRenderer.material.color) Debug.LogWarning("Burnable object " + gameObject + " sprite color is not sprite renderer material color, this may cause issues with burn effect.");

        if (burnGradient.Evaluate(0f) != spriteRenderer.material.color) Debug.LogWarning("Burnable object " + gameObject + " burn gradient start color is not sprite renderer material color, this may cause issues with burn effect.");

    }

    public void StartBurn() {

        if (isDestroyed) return; // return if object is already destroyed

        if (burnTweener != null && burnTweener.IsActive()) return; // return if burn tweener is active (object is already burning)

        if (resetTweener != null && resetTweener.IsActive()) resetTweener.Kill(); // kill reset tweener if it is active as burning is beginning

        burnTweener = DOVirtual.Float(currGradientProgress, 1f, burnResistanceDuration * (1f - currGradientProgress), (progress) => { // burn object material color to end of gradient (flip gradient progress because it is the distance from 0f, and we want the distance from 1f to calculate the time left to the end of the gradient)

            spriteRenderer.material.color = burnGradient.Evaluate(progress); // update object material color to gradient color
            currGradientProgress = progress; // update current gradient progress

        }).SetEase(Ease.Linear).OnComplete(() => {

            isDestroyed = true; // set destroyed flag to true
            Destroy(gameObject); // destroy object after burning is complete

        });
    }

    public void StopBurn() {

        if (isDestroyed) return; // return if object is already destroyed

        if (resetTweener != null && resetTweener.IsActive()) return; // return if reset tweener is active (object is already resetting)

        if (burnTweener != null && burnTweener.IsActive()) burnTweener.Kill(); // kill burn tweener if it is active as resetting is beginning

        resetTweener = DOVirtual.Float(currGradientProgress, 0f, resetDuration * currGradientProgress, (progress) => { // reset object material color to beginning of gradient (don't flip gradient progress because it is the distance from 0f, which is what we need to calculate the time left to the beginning of the gradient)

            spriteRenderer.material.color = burnGradient.Evaluate(progress); // update object material color to gradient color
            currGradientProgress = progress; // update current gradient progress

        }).SetEase(Ease.Linear);
    }
}
