using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class TriggerTileButton : TriggerBase
{
    public Transform ButtonTransform; // Assign the button transform in the Inspector
    public float PressDepth = 0.2f; // How far the button moves when pressed
    public float AnimationDuration = 0.2f; // Duration of the press animation
    public Ease AnimationEase = Ease.OutBack; // Easing function for the animation


    public Vector3 LocalNormal => Vector3.forward; // Local Z

    [Button("HitTriggerPress")]
    public void HitTriggerPress(TileWalker tileWalker = null)
    {
        if (WillHit())
        {
            base.HitTrigger();
            VisualHandler();
        }
    }

    private void VisualHandler()
    {
        if (ButtonTransform == null)
        {
            Debug.LogWarning("ButtonTransform is not assigned.");
            return;
        }

        // Animate the button press
        Vector3 originalPosition = ButtonTransform.localPosition; // Store the original position
        Vector3 pressedPosition = originalPosition - LocalNormal * PressDepth; // Calculate pressed position

        // Animate the button going down and coming back up
        ButtonTransform.DOLocalMove(pressedPosition, AnimationDuration)
            .SetEase(AnimationEase)
            .OnComplete(() =>
                ButtonTransform.DOLocalMove(originalPosition, AnimationDuration).SetEase(AnimationEase)
            );
    }
}