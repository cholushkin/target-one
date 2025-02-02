using UnityEngine;
using GameLib.Alg;
using GameLib.Log;

public class TriggerTileLevChunkEnter : TriggerTileBase
{
    public class EventTriggerTileLevChunkEnter
    {
        public LevChunk LevChunkEnter;
        public Tile Tile;
    }

    public LevChunk LevChunkEnter;

    public override void CustomTriggerLogic()
    {
        LogChecker.Print(LogChecker.Level.Verbose, $"TriggerTileLevChunkEnter for {transform.GetDebugName()}");
        GlobalEventAggregator.EventAggregator.Publish(new EventTriggerTileLevChunkEnter
        {
            LevChunkEnter = LevChunkEnter,
            Tile = Tile 
        });
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.05f * 4f);
    }
}
