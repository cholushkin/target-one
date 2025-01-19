using UnityEngine;

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
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, 0.05f * 2f);
        }
    }
}