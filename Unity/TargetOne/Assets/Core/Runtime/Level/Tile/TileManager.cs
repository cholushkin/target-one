using Core;
using Events;
using UnityEngine;

public class TileManager : MonoBehaviour
    , IHandle<TileWalker.EventWalkerAttachToTile>
    , IHandle<TileWalker.EventWalkerReachTileCenter>
{
    void Awake()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }

    public void Handle(TileWalker.EventWalkerAttachToTile message)
    {
        // On attach to a new tile
        message.CurrentTile.GetComponent<TileWheelRotation>()
            ?.RegisterEntering(message.PrevTile);

        message.PrevTile.GetComponent<TriggerTileExit>()
            ?.HitTriggerExit(message.TileWalker);

        message.CurrentTile.GetComponent<TriggerTileEnter>()
            ?.HitTriggerEnter(message.TileWalker);
    }

    public void Handle(TileWalker.EventWalkerReachTileCenter message)
    {
        message.Tile.GetComponent<TriggerTileReachCenter>()
            ?.HitTriggerReachCenter(message.TileWalker);
    }
}