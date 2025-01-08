namespace Core
{
    public class TriggerTileReachCenter : TriggerTileBase
    {
        public class EventTriggerTileReachCenter
        {
            public Tile Tile;
        }
        
        public override void CustomTriggerLogic()
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventTriggerTileReachCenter
            {
                Tile = Tile 
            });
        }
    }
}