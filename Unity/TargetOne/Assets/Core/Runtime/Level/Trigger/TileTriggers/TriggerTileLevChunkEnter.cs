public class TriggerLevChunkEnter : TriggerTileBase
{
    public class EventTriggerLevChunkEnter
    {
        public LevChunk LevChunkEnter;
        public Tile Tile;
    }

    public LevChunk LevChunkEnter;

    public override void CustomTriggerLogic()
    {
        GlobalEventAggregator.EventAggregator.Publish(new EventTriggerLevChunkEnter
        {
            LevChunkEnter = LevChunkEnter,
            Tile = Tile 
        });
    }
}
