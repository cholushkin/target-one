using UnityEngine;

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
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.05f * 1f);
        }
    }
}