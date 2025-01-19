using UnityEngine;

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
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.05f * 6f);
        }
    }
}