using DG.Tweening;
using UnityEngine;

public class MoveTo : MonoBehaviour
{
    public Transform Target; // The target position to move to
    public float Speed; // The movement speed
    public bool AutoStart; // Whether to start moving automatically

    void Start()
    {
        if (AutoStart)
            Move();
    }

    public void Move()
    {
        // Calculate the target position in the local space of the object's parent
        Vector3 targetLocalPosition = transform.parent.InverseTransformPoint(Target.position);

        // Use DOTween to smoothly move to the target position in the local space
        transform.DOLocalMove(
                targetLocalPosition,                // Target position in local space
                Speed * GameSessionController.Instance.GameSpeed // Adjust speed based on game speed
            )
            .SetEase(Ease.Linear)                   // Linear easing for consistent speed
            .SetSpeedBased(true);                   // Treat the second parameter as speed
    }
}