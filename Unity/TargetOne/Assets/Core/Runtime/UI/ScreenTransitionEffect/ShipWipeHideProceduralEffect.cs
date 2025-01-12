using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


// Works with multiaspect screen ratios
public class ShipWipeHideProceduralEffect : TransitionEffect
{
    public AnimationEventCallback Callback;
    public RectTransform ImageCircle;
    public Ease ShipEase;
    public RectTransform ImageShip;
    public RectTransform StartPivot;
    public RectTransform EndPivot;

    public override void  OnEnable()
    {
        StartCoroutine(WipeHide());
    }

    IEnumerator WipeHide()
    {
        ImageCircle.gameObject.SetActive(false);
        ImageShip.gameObject.SetActive(false);
        yield return null; // skip frame to allow unity do layout recalculations
        Play();
    }

    private void Play()
    {
        ImageCircle.gameObject.SetActive(true);
        ImageShip.gameObject.SetActive(true);
        ImageCircle.position = StartPivot.position;
        ImageShip.position = StartPivot.position;

        var a = Screen.width;
        var b = Screen.height;
        var r = Mathf.Sqrt(a * a + b * b) * 0.5f; // the radius of the circumscribed circle of the rectangle
        var k = r / (Screen.height * 0.5f);

        ImageCircle.DOAnchorPos(Vector3.zero, Duration)
            .SetEase(ShipEase)
            .OnComplete(Callback.OnAnimationFinished);
        ImageCircle.DOScale(Vector3.one * k * 1.1f, Duration)
            .SetEase(ShipEase);
        ImageShip.DOMove(EndPivot.position, Duration)
            .SetEase(ShipEase);
    }
}
