using Unity.Cinemachine;
using UnityEngine;

public class LevChunkCamerasContainer : MonoBehaviour
{
    public CinemachineCamera[] Cameras;

    public void Awake()
    {
        // Make sure all cameras are disabled on initialization
        // The active camera is from previous LevChunk
        foreach (var cCam in Cameras)
            cCam.gameObject.SetActive(false);
    }
}
