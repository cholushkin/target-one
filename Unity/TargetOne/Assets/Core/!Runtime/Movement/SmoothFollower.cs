using UnityEngine;

public class SmoothFollower : MonoBehaviour
{
    public Transform Target;
    public float SmoothTime;
    public float RotationSmoothTime = 0.3f; // Separate smooth time for rotation
    private Vector3 _velocity = Vector3.zero;

    void Reset()
    {
        SmoothTime = 0.3f;
        RotationSmoothTime = 0.3f;
    }

    void Update()
    {
        UpdateInternal();
    }

    private void UpdateInternal()
    {
        if (!Target)
            return;

        // Smoothly follow position
        transform.position = Vector3.SmoothDamp(
            transform.position, Target.position, ref _velocity, SmoothTime);

        // Smoothly rotate to match the target's rotation
        Quaternion targetRotation = Target.rotation; // Target's rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRotation, Time.deltaTime / RotationSmoothTime);
    }
}