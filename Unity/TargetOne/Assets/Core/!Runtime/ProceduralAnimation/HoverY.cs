using DG.Tweening;
using UnityEngine;

public class HoverY : MonoBehaviour
{
    public float EndValue = 1f;
    private Tweener _tweener;
    void Start()
    {
        _tweener = transform
            .DOMoveY(EndValue, 2f * GameSession.Instance.GameSpeed)
            .SetEase(Ease.InSine)
            .SetUpdate(UpdateType.Manual)
            .SetLoops(-1, LoopType.Yoyo)
            .SetSpeedBased(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
