namespace Core
{
    public class TriggerTileEnter : TriggerTileBase
    {
        public class EventTriggerTileEnter
        {
            public Tile Tile;
        }
        
        public override void CustomTriggerLogic()
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventTriggerTileEnter
            {
                Tile = Tile 
            });
        }
    }
}