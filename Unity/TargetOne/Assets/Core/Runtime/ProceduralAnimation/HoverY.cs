using DG.Tweening;
using UnityEngine;

public class HoverY : MonoBehaviour
{
    public float EndValue = 1f;
    public float Speed = 1f;
    private Tweener _tweener;
    void Start()
    {
        _tweener = transform
            .DOLocalMoveY(EndValue, Speed * GameSession.Instance.GameSpeed)
            .SetEase(Ease.InSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetSpeedBased(true);
    }
}
