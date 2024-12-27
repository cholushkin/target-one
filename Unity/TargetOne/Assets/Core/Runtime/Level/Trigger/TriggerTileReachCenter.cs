using UnityEngine;

public class TriggerTileReachCenter : TriggerBase
{
    public void HitTriggerReachCenter(TileWalker tileWalker)
    {
        if (WillHit())
        {
            base.HitTrigger();
        }
    }
}