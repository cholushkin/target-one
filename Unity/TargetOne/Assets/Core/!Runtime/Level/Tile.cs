using UnityEngine;


[SelectionBase]
public class Tile : MonoBehaviour
{
    public const float TileSize = 2f;

    public Vector3 Forward => transform.right; // Local X
    public Vector3 Right => transform.up;     // Local Y
    public Vector3 Normal => transform.forward; // Local Z
    public Vector3 Up => Normal;
}