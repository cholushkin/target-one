using DG.Tweening;
using UnityEngine;

public class HoverY : MonoBehaviour
{
    public float EndValue = 1f;
    public float Duration = 1f;
    private Tweener _tweener;
    void Start()
    {
        _tweener = transform
            .DOMoveY(EndValue, Duration * GameSession.Instance.GameSpeed)
            .SetEase(Ease.InSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetSpeedBased(true);
    }
}
