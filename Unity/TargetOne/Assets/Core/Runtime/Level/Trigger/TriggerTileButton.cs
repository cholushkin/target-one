using DG.Tweening;
using GameLib.Log;
using NaughtyAttributes;
using UnityEngine;

namespace Core
{
    public class TriggerTileButton : TriggerBase
    {
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

        [Tooltip("Percent of half tile size")]
        [Range(0f,1f)]
        public float ActiveRadius;

        public Vector3 LocalNormal => Vector3.forward; // Local Z axis

        public override void Reset()
        {
            base.Reset();
            ActiveRadius = 0.9f;
            PressDepth = 0.2f;
            PressDuration = 0.2f;
            UnpressDuration = 0.8f;
            PressEase = Ease.InBack;
            UnpressEase = Ease.OutElastic;
        }

        [Button("HitTriggerPress")]
        public void HitTriggerPress(TileWalker tileWalker = null)
        {
            if (WillHit())
            {
                // Quit if not within Active radius 
                {
                    var distance = Vector3.Distance(tileWalker.transform.position, transform.position);
                    if (distance / (Tile.TileSize * 0.5f) > ActiveRadius)
                    {
                        Debug.Log($"Out of button active radius!");
                        return;
                    }
                }
                
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