using GameLib.Alg;
using NaughtyAttributes;
using UnityEngine;

public class CameraFitToMesh : MonoBehaviour
{
	[Required] public Camera MainCamera;  // camera to fit
	[Required] public GameObject TargetObject;

	void Start()
	{
		FitToScreen();
	}

	void FitToScreen()
	{
		// Get the bounds of the mesh
		Bounds bounds = TargetObject.BoundBox();
        
		// Calculate the height of the mesh in world space
		float meshHeight = bounds.size.y;

		// Adjust the orthographic size based on the mesh's height
		MainCamera.orthographicSize = meshHeight / 2f;
	}
}