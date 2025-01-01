using Core;
using Events;
using UnityEngine;

public class PlayerController : MonoBehaviour, IHandle<TileWalker.EventWalkerReachTileCenter>
{
    public TileWalker TileWalker;
    public bool AutoWalker;


    public void Awake()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }
    
    public void DoInteraction()
    {
        if (!TileWalker.CurrentTile) 
            return;
        
        // If there is a button
        var tileButtonTrigger = TileWalker.CurrentTile.GetComponent<TriggerTileButton>();
        tileButtonTrigger?.HitTriggerPress(TileWalker);
    }

    public void Handle(TileWalker.EventWalkerReachTileCenter message)
    {
        if (AutoWalker)
        {
            // If there is a button
            var tileButtonTrigger = TileWalker.CurrentTile.GetComponent<TriggerTileButton>();
            tileButtonTrigger?.HitTriggerPress(TileWalker);
        }
    }
}
