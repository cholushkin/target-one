using DG.Tweening;
using UnityEngine;

public class TubeTeleportAnimator : MonoBehaviour
{
    public Transform Cylinder;
    public void AnimateSuckInside()
    {
        Cylinder.transform.DOLocalMoveZ(0.1f, 0.5f)
            .SetEase(Ease.InOutBack)
            .SetLoops(2, LoopType.Yoyo);
    }
    
    public void AnimateThrowUp()
    {
        Cylinder.transform.DOLocalMoveZ(0.1f, 0.5f)
            .SetEase(Ease.InOutBack)
            .SetLoops(2, LoopType.Yoyo);
    }
}
