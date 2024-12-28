using Core;
using GameLib.Log;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif


[SelectionBase]
public class Tile : MonoBehaviour
{
    public LogChecker LogChecker;
    public const float TileSize = 2f;

    public Vector3 Forward => transform.right; // Local X
    public Vector3 Right => transform.up;     // Local Y
    public Vector3 Normal => transform.forward; // Local Z
    public Vector3 Up => Normal;

    
    private void OnDrawGizmos()
    {
        if (LogChecker.Gizmos)
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
    private void CreateButton()
    {
        // TileWheelRotation
        var tileWheelRotation = GetComponent<TileWheelRotation>();
        if (tileWheelRotation == null)
        {
            tileWheelRotation = gameObject.AddComponent<TileWheelRotation>();
            Debug.Log("Added TileWheelRotation component.");
        }

        // TriggerTileButton
        var triggerTileButton = GetComponent<TriggerTileButton>();
        if (triggerTileButton == null)
        {
            triggerTileButton = gameObject.AddComponent<TriggerTileButton>();
            triggerTileButton.MaxHitCount = 1;

            var visual = transform.Find("Visual");
            if (visual)
            {
                foreach (Transform t in visual.transform)
                {
                    if (!t.gameObject.name.StartsWith("Button")) 
                        continue;
                    triggerTileButton.ButtonTransform = t;
                    break;
                }
            }

            // Initialize UnityEvent if needed
            triggerTileButton.Handlers ??= new UnityEvent();

            // Add listener
            UnityEventTools.AddPersistentListener(triggerTileButton.Handlers, tileWheelRotation.Rotate);
            Debug.Log("Added TriggerTileButton component and assigned listener.");
        }

        // TriggerTileExit
        var triggerTileExit = GetComponent<TriggerTileExit>();
        if (triggerTileExit == null)
        {
            triggerTileExit = gameObject.AddComponent<TriggerTileExit>();
            triggerTileExit.Handlers ??= new UnityEvent();
            
            UnityEventTools.AddPersistentListener(triggerTileExit.Handlers, triggerTileButton.ResetTriggerCount);
            Debug.Log("Added TriggerTileExit component.");
        }
    }
    #endif
}