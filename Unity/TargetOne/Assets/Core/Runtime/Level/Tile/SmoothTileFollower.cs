using Core;
using UnityEngine;
using UnityEngine.Assertions;

public class SmoothTileFollower : MonoBehaviour
{
    public TileWalker Walker;
    public bool EnablePositionSmooth;
    public float PositionSmoothTime;
    private Vector3 _velocity = Vector3.zero;


    public bool EnableRotationSmooth;
    public Transform RotationDestinationPointer; 
    public Transform RotationSourcePointer;

    public bool EnableDoubleRotationSmooth;

    private Vector3 _smoothPosition;
    private float _segmentDuration;
    private float _currentSegmentTime;


    void Reset()
    {
        PositionSmoothTime = 0.3f;
    }

    public void Init(Vector3 position, Quaternion rotation)
    {
        Assert.IsNotNull(Walker);
        transform.position = _smoothPosition = position;
        transform.rotation = rotation;

        RotationDestinationPointer.position = RotationSourcePointer.position = transform.position;
        RotationDestinationPointer.rotation = RotationSourcePointer.rotation = transform.rotation;
    }

    void Update()
    {
        if (Walker)
            UpdateInternal();
    }

    public void SetInterpolationSegment(Quaternion targetRotation, Transform parent, Vector3 targetPosition, float estDuration)
    {
        RotationDestinationPointer.position = targetPosition;
        RotationDestinationPointer.rotation = targetRotation;
        RotationDestinationPointer.SetParent(parent);
        
        RotationSourcePointer.position = transform.position;
        RotationSourcePointer.rotation = transform.rotation;
        RotationSourcePointer.SetParent(parent);
        
        _segmentDuration = estDuration;
        _currentSegmentTime = 0f;
    }
    
    public void ReparentRotationPointers(Transform newParent)
    {
        RotationDestinationPointer.SetParent(newParent);
        RotationSourcePointer.SetParent(newParent);
    }

    private void UpdateInternal()
    {
        // Process position ERP
        if (EnablePositionSmooth)
        {
            _smoothPosition = Vector3.SmoothDamp(
                _smoothPosition, Walker.transform.position, ref _velocity, PositionSmoothTime);
            
            // Shorten smooth offset
            _smoothPosition += (Walker.transform.position - _smoothPosition) * 0.6f;
            
            transform.position = _smoothPosition;
        }
        else
            transform.position = Walker.transform.position;

        // Process angle ERP
        if (EnableRotationSmooth)
        {
            _currentSegmentTime += Time.deltaTime;
            if (Mathf.Approximately(_segmentDuration, 0f))
                transform.rotation = RotationDestinationPointer.rotation;
            else
                transform.rotation = Quaternion.Slerp(RotationSourcePointer.rotation, RotationDestinationPointer.rotation, _currentSegmentTime / _segmentDuration);

            if (EnableDoubleRotationSmooth)
            {
                
            }
        }
        else
        {
            transform.rotation = Walker.transform.rotation;
        }
    }

    private void OnDrawGizmos()
    {
        DrawAxes(transform.position, transform.rotation, 0.2f);
        DrawAxes(RotationDestinationPointer.position, RotationDestinationPointer.rotation, 0.2f);
        DrawAxes(RotationSourcePointer.position, RotationSourcePointer.rotation, 0.2f);
    }
    
    private void DrawAxes(Vector3 position, Quaternion rotation, float axisLength)
    {
        // Define axis directions in local space
        Vector3 xAxis = rotation * Vector3.right * axisLength;
        Vector3 yAxis = rotation * Vector3.up * axisLength;
        Vector3 zAxis = rotation * Vector3.forward * axisLength;

        // Draw X axis (red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + xAxis);

        // Draw Y axis (green)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + yAxis);

        // Draw Z axis (blue)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(position, position + zAxis);
    }


}