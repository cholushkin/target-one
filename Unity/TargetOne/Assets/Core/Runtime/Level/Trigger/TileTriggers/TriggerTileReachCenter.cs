using UnityEngine;
using GameLib.Alg;
using GameLib.Log;

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
            LogChecker.Print(LogChecker.Level.Verbose, $"TriggerTileReachCenter for {transform.GetDebugName()}");
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