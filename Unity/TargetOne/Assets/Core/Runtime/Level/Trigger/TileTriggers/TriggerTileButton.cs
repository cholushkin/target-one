namespace Core
{
    public class TriggerTileButton : TriggerTileBase
    {
        public class EventTriggerTileButton
        {
            public Tile Tile;
        }
        
        public override void CustomTriggerLogic()
        {
            GlobalEventAggregator.EventAggregator.Publish(new EventTriggerTileButton
            {
                Tile = Tile 
            });
        }
    }
}