using Core;
using GameLib.Alg;
using GameLib.Log;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif


[SelectionBase]
public class Tile : MonoBehaviour
{
    public LogChecker LogChecker;
    public Transform Visual;
    public const float TileSize = 2f;

    public Vector3 Forward => transform.right; // Local X
    public Vector3 Right => transform.up;     // Local Y
    public Vector3 Normal => transform.forward; // Local Z
    public Vector3 Up => Normal;

    private void Reset()
    {
        Visual = transform.Find("Visual");
    }
    
    private void OnDrawGizmos()
    {
        if (LogChecker != null && LogChecker.Gizmos)
        {
            // Draw local axes
            Gizmos.color = Color.red; // X-axis
            Gizmos.DrawLine(transform.position, transform.position + Forward * TileSize * 0.5f);
            
            Gizmos.color = Color.green; // Y-axis
            Gizmos.DrawLine(transform.position, transform.position + Right * TileSize * 0.5f);

            Gizmos.color = Color.blue; // Z-axis
            Gizmos.DrawLine(transform.position, transform.position + Normal * TileSize * 0.5f);
        }
    }
   
    
    #if UNITY_EDITOR
    [Button]
    private void CreateTileWheelRotation()
    {
        // TileWheelRotation
        var tileWheelRotation = GetComponent<TileWheelRotation>();
        if (tileWheelRotation == null)
        {
            tileWheelRotation = gameObject.AddComponent<TileWheelRotation>();
            Debug.Log("Added TileWheelRotation component.");
        }
        else
        {
            Debug.LogWarning($"TileWheelRotation component is already presented on {gameObject.transform.GetDebugName()}");
        }

        // TriggerTileButton
        var triggerTileButton = GetComponent<TriggerTileButton>();
        if (triggerTileButton == null)
        {
            triggerTileButton = gameObject.AddComponent<TriggerTileButton>();
            triggerTileButton.MaxHitCount = -1;
            triggerTileButton.OneHitMaxPerVisit = true;

            var visual = transform.FirstChildNameStartsWith("Visual");
            TileButtonAnimator tileButtonAnimator = null;
            if (visual)
            {
                var buttonTransform = visual.transform.FirstChildNameStartsWith("Button");
                if (buttonTransform != null)
                {
                    tileButtonAnimator = visual.gameObject.AddComponent<TileButtonAnimator>();
                    tileButtonAnimator.ButtonTransform = buttonTransform;
                }
            }

            // Initialize UnityEvent if needed
            triggerTileButton.Handlers ??= new UnityEvent();

            // Add listener
            if(tileButtonAnimator)
                UnityEventTools.AddPersistentListener(triggerTileButton.Handlers, tileButtonAnimator.AnimateButtonClick);
            else
            {
                Debug.LogWarning("No Button visual presented");
            }
            UnityEventTools.AddPersistentListener(triggerTileButton.Handlers, tileWheelRotation.Rotate);
            Debug.Log("Added TriggerTileButton component and assigned listener.");
        }
        else
        {
            Debug.LogWarning($"TriggerTileButton component is already presented on {gameObject.transform.GetDebugName()}");
        }
        
        Debug.Log("Tile button has been created. Don't forget to assign TileWheelRotation.Rotations");
    }
    
    
    [Button]
    private void CreateTubeTeleport()
    {
        // CreateTubeTeleport
        var tubeTeleport = GetComponent<TubeTeleport>();
        if (tubeTeleport == null)
        {
            tubeTeleport = gameObject.AddComponent<TubeTeleport>();
            tubeTeleport.Tile = this;
            Debug.Log("Added TubeTeleport component.");
        }
        else
        {
            Debug.LogWarning($"TubeTeleport component is already presented on {gameObject.transform.GetDebugName()}");
        }

        // TriggerTubeTeleport
        var triggerTubeTeleport = GetComponent<TriggerTubeTeleport>();
        
        if (triggerTubeTeleport == null)
        {
            TubeTeleportAnimator tAnimator = null;
            triggerTubeTeleport = gameObject.AddComponent<TriggerTubeTeleport>();
            triggerTubeTeleport.MaxHitCount = -1;
            triggerTubeTeleport.OneHitMaxPerVisit = true;

            var visual = transform.FirstChildNameStartsWith("Visual");
            if (visual)
            {
                if (visual.gameObject.GetComponent<TubeTeleportAnimator>() == null)
                {
                    tAnimator = visual.gameObject.AddComponent<TubeTeleportAnimator>();
                    tubeTeleport.TubeAnimator = tAnimator;
                    tAnimator.Cylinder = visual.transform.FirstChildNameStartsWith("Cylinder");
                    Debug.Log("Added TubeTeleportAnimator component.");
                }
            }

            // Initialize UnityEvent if needed
            triggerTubeTeleport.Handlers ??= new UnityEvent();

            // Add listener
            UnityEventTools.AddPersistentListener(triggerTubeTeleport.Handlers, tubeTeleport.StartTeleporting);
            UnityEventTools.AddPersistentListener(triggerTubeTeleport.Handlers, tAnimator.AnimateSuckInside);
            Debug.Log("Added TriggerTubeTeleport component and assigned listener TubeTeleport.StartTeleporting.");
        }
        else
        {
            Debug.LogWarning($"TriggerTubeTeleport component is already presented on {gameObject.transform.GetDebugName()}");
        }
        
        Debug.Log("Tube teleport has been created. Don't forget to assign TubeTeleport.ConnectedTube");
    }
    #endif
}