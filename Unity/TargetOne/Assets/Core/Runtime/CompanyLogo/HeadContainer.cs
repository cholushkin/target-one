using DG.Tweening;
using UnityEngine;

public class HeadContainer : MonoBehaviour
{
    public float Scale;
    public float LoopDuration;
    public bool RandomizeInitialScale;
    public AnimationCurve AnimCurve;

    void Awake()
    {
        var tween = transform
            .DOScale(Scale, LoopDuration)
            .SetLoops(2, LoopType.Yoyo);
        tween.SetEase(AnimCurve);
    }
}
