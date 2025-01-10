using GameLib.Alg;
using NaughtyAttributes;
using UnityEngine;

public class CameraFitToRenderer : MonoBehaviour
{
	[Required] public Camera MainCamera;  // camera to fit
	[Required] public Renderer TargetRenderer;

	void Start()
	{
		FitToScreen();
	}

	void FitToScreen()
	{
		// Get the bounds of the mesh
		Bounds bounds = TargetRenderer.bounds;
        
		// Calculate the height of the mesh in world space
		float meshHeight = bounds.size.y;

		// Adjust the orthographic size based on the mesh's height
		MainCamera.orthographicSize = meshHeight / 2f;
	}
}