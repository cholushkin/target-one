using UnityEngine;
using GameLib.Alg;
using GameLib.Log;

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
            LogChecker.Print(LogChecker.Level.Verbose, $"TriggerTileExit for {transform.GetDebugName()}");
            GlobalEventAggregator.EventAggregator.Publish(new EventTriggerTileExit
            {
                Tile = Tile 
            });
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, 0.05f * 3f);
        }
    }
}