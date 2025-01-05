using Unity.Cinemachine;
using UnityEngine;

public class BlendToCinemachineCamera : MonoBehaviour
{
    public CinemachineCamera Camera;

    public void BlendToCamera()
    {
        Camera.gameObject.SetActive(true);
        
        var curCam = CameraController.Instance.CinemachineBrain.ActiveVirtualCamera;
        if (curCam != null)
        {
            // Get the GameObject of the active virtual camera
            GameObject activeCamGameObject = (curCam as CinemachineVirtualCameraBase)?.gameObject;
            activeCamGameObject?.SetActive(false);
        }
    }
}
