using UnityEngine;

public class TriggerTileLevChunkExit : TriggerTileBase
{
    public class EventTriggerLevChunkExit
    {
        public LevChunk LevChunkExit;
        public Tile Tile; // Tile of exit
    }

    public LevChunk LevChunkExit;

    public override void CustomTriggerLogic()
    {
        GlobalEventAggregator.EventAggregator.Publish(new EventTriggerLevChunkExit
        {
            LevChunkExit = LevChunkExit,
            Tile = Tile 
        });
    }
}