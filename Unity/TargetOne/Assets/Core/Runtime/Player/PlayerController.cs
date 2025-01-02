using Core;
using Events;
using UnityEngine;

public class PlayerController : 
    MonoBehaviour, 
    IHandle<TileWalker.EventWalkerReachTileCenter>,
    IHandle<TileWalker.EventStartFalling>,
    IHandle<TileWalker.EventFallRecover>
{
    public TileWalker TileWalker;
    public CharacterHover Hover;
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

    public void Handle(TileWalker.EventStartFalling message)
    {
        Hover.Set(Hover.WakeUpValues.amp, Hover.WakeUpValues.height, message.Duration);
    }

    public void Handle(TileWalker.EventFallRecover message)
    {
        Hover.Set(Hover.NormalValues.amp, Hover.NormalValues.height, message.Duration);
    }
}