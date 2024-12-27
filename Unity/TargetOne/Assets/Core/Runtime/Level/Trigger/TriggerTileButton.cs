using DG.Tweening;
using GameLib.Log;
using NaughtyAttributes;
using UnityEngine;

namespace Core
{
    public class TriggerTileButton : TriggerBase
    {
        public LogChecker LogChecker;

        [Tooltip("The transform of the button to animate.")]
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

        void Reset()
        {
            PressDepth = 0.2f;
            PressDuration = 0.2f;
            UnpressDuration = 0.4f;
            PressEase = Ease.InBack;
            UnpressEase = Ease.OutElastic;
        }

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
            if (!ButtonTransform)
            {
                LogChecker.PrintWarning(LogChecker.Level.Important, "ButtonTransform is not assigned.");
                return;
            }

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
}