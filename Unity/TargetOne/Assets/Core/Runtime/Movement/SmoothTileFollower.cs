using Core;
using UnityEngine;

public class SmoothTileFollower : MonoBehaviour
{
    public TileWalker Walker;
    public bool EnablePositionSmooth;
    public float PositionSmoothTime;
    public float RotationSmoothTime = 0.3f; // Separate smooth time for rotation
    private Vector3 _velocity = Vector3.zero;


    public bool EnableRotationSmooth;
    private Quaternion _lerpTarget;
    private Quaternion _lerpSource;
    private float _duration;
    private float _currentSegmentTime;

    void Reset()
    {
        PositionSmoothTime = 0.3f;
        RotationSmoothTime = 0.3f;
    }

    void Start()
    {
        if (!Walker)
            return;
        transform.rotation = _lerpTarget = _lerpSource = Walker.transform.rotation;
    }

    void Update()
    {
        if (Walker)
            UpdateInternal();
    }

    public void SetErpTarget(Quaternion newWorldDirection, float estDuration)
    {
        _lerpTarget = newWorldDirection;
        _lerpSource = transform.rotation;
        _duration = estDuration;
        _currentSegmentTime = 0f;
    }

    private void UpdateInternal()
    {
        if (!Walker)
            return;

        // Process position ERP
        if (EnablePositionSmooth)
            transform.position = Vector3.SmoothDamp(
                transform.position, Walker.transform.position, ref _velocity, PositionSmoothTime);
        else
            transform.position = Walker.transform.position;

        // Process angle ERP
        if (EnableRotationSmooth)
        {
            _currentSegmentTime += Time.deltaTime;
            if (Mathf.Approximately(_duration, 0f))
                transform.rotation = _lerpTarget;
            else
                transform.rotation = Quaternion.Slerp(_lerpSource, _lerpTarget, _currentSegmentTime / _duration);
        }
        else
        {
            transform.rotation = Walker.transform.rotation;
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    //
    //
    // public void SetErpTarget(Quaternion newWorldDirection, float estDuration)
    // {
    //     var localDirection = Quaternion.Inverse(transform.parent.rotation) * newWorldDirection;
    //     _lerpTarget = localDirection;
    //     _lerpSource = transform.localRotation;
    //     _duration = estDuration;
    //     _currentSegmentTime = 0f;
    // }
    //
    // private void UpdateInternal()
    // {
    //     if (!Walker)
    //         return;
    //
    //     // Process position ERP
    //     if (EnablePositionSmooth)
    //         transform.localPosition = Vector3.SmoothDamp(
    //             transform.localPosition, Walker.transform.localPosition, ref _velocity, PositionSmoothTime);
    //     else
    //         transform.localPosition = Walker.transform.localPosition;
    //
    //     // Process angle ERP
    //     _currentSegmentTime += Time.deltaTime;
    //     transform.localRotation =
    //         Quaternion.Slerp(_lerpSource, _lerpTarget, _currentSegmentTime / _duration);
    //
    //     // Smoothly rotate to match the target's rotation
    //     // Quaternion targetRotation = Walker.transform.rotation; // Target's rotation
    //     // transform.rotation = Quaternion.Slerp(
    //     //     transform.rotation, _lerpTarget, Time.deltaTime / RotationSmoothTime);
    // }
}