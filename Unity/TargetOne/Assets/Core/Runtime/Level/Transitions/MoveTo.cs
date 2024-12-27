using DG.Tweening;
using UnityEngine;

public class MoveTo : MonoBehaviour
{
    public Transform Target;
    public float Speed;
    public bool AutoStart;

    void Start()
    {
        if (AutoStart)
            Move();
    }

    public void Move()
    {
        transform.DOLocalMove(transform.InverseTransformPoint(Target.position), Speed * GameSession.Instance.GameSpeed)
            .SetEase(Ease.Linear)
            .SetSpeedBased(true);
    }
}