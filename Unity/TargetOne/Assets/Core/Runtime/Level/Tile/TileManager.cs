using Core;
using Events;
using UnityEngine;

public class TileManager : MonoBehaviour
    , IHandle<TileWalker.EventWalkerAttachToTile>
    , IHandle<TileWalker.EventWalkerDetachFromTile>
    , IHandle<TileWalker.EventWalkerReachTileCenter>
{
    void Awake()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }

    public void Handle(TileWalker.EventWalkerAttachToTile message)
    {
        // Register enter for all TriggerTileBase of the current tile
        var baseTileTriggers = message.CurrentTile.GetComponents<TriggerTileBase>();
        foreach (var triggerTileBase in baseTileTriggers)
        {
            triggerTileBase.RegisterWalkerEnter(message.TileWalker, message.PrevTile);
            (triggerTileBase as TriggerTileEnter)?.Trigger(message.TileWalker);
        }
    }

    public void Handle(TileWalker.EventWalkerReachTileCenter message)
    {
        // Trigger all TriggerTileReachCenter of the current tile
        var triggersTileReachCenter = message.Tile.GetComponents<TriggerTileReachCenter>();
        foreach (var triggerTileReachCenter in triggersTileReachCenter)
            triggerTileReachCenter.Trigger(message.TileWalker);
    }

    public void Handle(TileWalker.EventWalkerDetachFromTile message)
    {
        // Register exit for all TriggerTileBase of the current tile
        var baseTileTriggers = message.DetachedTile.GetComponents<TriggerTileBase>();
        foreach (var triggerTileBase in baseTileTriggers)
        {
            triggerTileBase.RegisterWalkerExit(message.TileWalker);
            (triggerTileBase as TriggerTileExit)?.Trigger(message.TileWalker);
        }
    }
}