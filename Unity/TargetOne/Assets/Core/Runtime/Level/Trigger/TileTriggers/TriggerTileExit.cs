namespace Core
{
    public class TriggerTileExit : TriggerTileBase
    {
        public class EventTriggerTileExit
        {
            public Tile Tile;
        }
        
        public override void CustomTriggerLogic()
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventTriggerTileExit
            {
                Tile = Tile 
            });
        }
    }
}