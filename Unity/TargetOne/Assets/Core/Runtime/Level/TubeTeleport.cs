using UnityEngine;

public class TubeTeleport : MonoBehaviour
{
    public TubeTeleport ConnectedTube;

    private void OnDrawGizmos()
    {
        if (ConnectedTube != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, ConnectedTube.transform.position);
            Gizmos.DrawSphere(ConnectedTube.transform.position, 0.2f);
        }
    }
}