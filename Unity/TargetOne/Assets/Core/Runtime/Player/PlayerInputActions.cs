using Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputActions : MonoBehaviour
{
    public PlayerController PlayerController;
    private PlayerInput playerInput; // Reference to the PlayerInput component
    private InputAction interactAction; // Action for the space button

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
    }

    private void Update()
    {
        if (PlayerController.TileWalker.CurrentState == TileWalker.State.Awake)
            return;
        if (interactAction.WasPressedThisFrame())
        {
            Debug.Log("Input: Interact");
            PlayerController.DoInteraction();
        }
    }

    public Vector3 GetOrbitCameraRotation()
    {
        return Vector3.zero;
    }
}