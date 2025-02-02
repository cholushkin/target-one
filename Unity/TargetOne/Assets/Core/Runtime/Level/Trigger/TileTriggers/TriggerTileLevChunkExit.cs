using UnityEngine;
using GameLib.Alg;
using GameLib.Log;

public class TriggerTileLevChunkExit : TriggerTileBase
{
    public class EventTriggerTileLevChunkExit
    {
        public LevChunk LevChunkExit;
        public Tile Tile; // Tile of exit
    }

    public LevChunk LevChunkExit;

    public override void CustomTriggerLogic()
    {
        LogChecker.Print(LogChecker.Level.Verbose, $"TriggerTileLevChunkExit for {transform.GetDebugName()}");
        GlobalEventAggregator.EventAggregator.Publish(new EventTriggerTileLevChunkExit
        {
            LevChunkExit = LevChunkExit,
            Tile = Tile 
        });
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.05f * 5f);
    }
}