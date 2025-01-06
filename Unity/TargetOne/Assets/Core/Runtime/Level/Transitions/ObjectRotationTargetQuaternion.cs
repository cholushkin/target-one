using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

public class ObjectRotationTargetQuaternion : MonoBehaviour
{
    public Quaternion TargetRotation;
    public bool UseIncrement;
    public float RotationSpeed = 100f; // Speed multiplier for rotation
    public bool AutoStart;
    public Ease Ease = Ease.InOutQuad;

    void Start()
    {
        if (AutoStart)
            Rotate();
    }

    
    [Button]
    public void Rotate()
    {
        Quaternion finalTargetRotation;

        if (UseIncrement)
        {
            // Incremental rotation: add TargetRotation to the current rotation
            finalTargetRotation = transform.localRotation * TargetRotation;
        }
        else
        {
            // Absolute rotation: use TargetRotation directly
            finalTargetRotation = TargetRotation;
        }
        
        var speed = RotationSpeed * GameSessionController.Instance.GameSpeed;
        var distance =  Quaternion.Angle(transform.localRotation, finalTargetRotation);
        float duration = distance / RotationSpeed;

        transform.DOLocalRotateQuaternion(finalTargetRotation, duration)
            .SetEase(Ease);
    }
}