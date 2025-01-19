using Gamelib;
using UnityEngine;

public class LevChunkExit : TrackableMonoBehaviour<LevChunkExit>
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}
