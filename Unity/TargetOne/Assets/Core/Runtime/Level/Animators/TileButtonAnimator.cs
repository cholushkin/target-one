using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

public class TileButtonAnimator : MonoBehaviour
{
    [Tooltip("The transform of the button to animate.")]
    [Required]
    public Transform ButtonTransform;

    [Tooltip("How far the button moves when pressed.")]
    public float PressDepth;

    [Tooltip("Duration of the button press animation.")]
    public float PressDuration;

    [Tooltip("Duration of the button release animation.")]
    public float UnpressDuration;

    [Tooltip("Easing function for the press animation.")]
    public Ease PressEase;

    [Tooltip("Easing function for the release animation.")]
    public Ease UnpressEase;

    public Vector3 LocalNormal => Vector3.forward; // Local Z axis
    
    public  void Reset()
    {
        PressDepth = 0.2f;
        PressDuration = 0.2f;
        UnpressDuration = 0.8f;
        PressEase = Ease.InBack;
        UnpressEase = Ease.OutElastic;
    }
    
    // click = move down and up
    public void AnimateButtonClick() 
    {
        Assert.IsNotNull(ButtonTransform);

        // Animate the button press
        Vector3 originalPosition = ButtonTransform.localPosition; // Store the original position
        Vector3 pressedPosition = originalPosition - LocalNormal * PressDepth; // Calculate pressed position

        // Animate the button going down and coming back up
        ButtonTransform.DOLocalMove(pressedPosition, PressDuration)
            .SetEase(PressEase)
            .OnComplete(() =>
                ButtonTransform.DOLocalMove(originalPosition, UnpressDuration)
                    .SetEase(UnpressEase)
            );
    }
}
