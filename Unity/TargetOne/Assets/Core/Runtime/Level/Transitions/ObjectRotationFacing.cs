using UnityEngine;
using DG.Tweening;

public class ObjectRotationFacing : MonoBehaviour
{
    public Transform CurrentFacing; // First object: current facing direction
    public Transform TargetFacing;  // Second object: target direction
    public float RotationSpeed = 1f; // Duration for the rotation animation
    public bool AutoStart;
    public Ease Ease = Ease.InOutQuad;

    void Start()
    {
        if(AutoStart)
            Rotate();
    }
    
    void Rotate()
    {
        // Ensure objects are set
        if (CurrentFacing == null || TargetFacing == null)
        {
            Debug.LogError("Please assign both currentFacing and targetFacing objects.");
            return;
        }

        // Get world-space directions
        Vector3 currentDirection = CurrentFacing.position - transform.position;
        Vector3 targetDirection = TargetFacing.position - transform.position;

        // Normalize directions
        currentDirection.Normalize();
        targetDirection.Normalize();

        // Transform directions into the Tile's local space
        Vector3 localCurrentDir = transform.InverseTransformDirection(currentDirection);
        Vector3 localTargetDir = transform.InverseTransformDirection(targetDirection);

        // Calculate rotation needed in local space
        Quaternion localRotation = Quaternion.FromToRotation(localCurrentDir, localTargetDir);

        // Animate the local rotation using DOTween
        Quaternion targetLocalRotation = transform.localRotation * localRotation;
        transform.DOLocalRotateQuaternion(targetLocalRotation, RotationSpeed * GameSession.Instance.GameSpeed)
            //.SetSpeedBased(true)
            .SetEase(Ease);
    }
}