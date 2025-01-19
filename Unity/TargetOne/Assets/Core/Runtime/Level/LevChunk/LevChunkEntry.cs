using Gamelib;
using UnityEngine;

public class LevChunkEntry : TrackableMonoBehaviour<LevChunkEntry>
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}