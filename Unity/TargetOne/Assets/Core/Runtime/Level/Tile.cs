using GameLib.Log;
using UnityEngine;


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
}