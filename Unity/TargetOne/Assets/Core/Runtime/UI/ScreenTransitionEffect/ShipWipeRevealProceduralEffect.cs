using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ShipWipeRevealProceduralEffect : TransitionEffect
{
    public AnimationEventCallback Callback;
    public RectTransform ImageCircle;
    public Ease CircleEase;
    public RectTransform StartPivot;
    public RectTransform EndPivot;

    public override void OnEnable()
    {
        StartCoroutine(WipeReveal());
    }

    IEnumerator WipeReveal()
    {
        // scale
        var a = Screen.width;
        var b = Screen.height;
        var r = Mathf.Sqrt(a * a + b * b) * 0.5f; // the radius of the circumscribed circle of the rectangle
        var k = r / (Screen.height * 0.5f);
        ImageCircle.localScale = Vector3.one * k * 1.1f;

        ImageCircle.localPosition = Vector3.zero;

        yield return null; // skip frame for layout recalculation
        Play();
    }

    private void Play()
    {
        ImageCircle.DOMove(EndPivot.position, Duration)
            .SetEase(CircleEase)
            .OnComplete(Callback.OnAnimationFinished);
        ImageCircle.DOScale(Vector3.one, Duration)
            .SetEase(CircleEase);
    }
}
