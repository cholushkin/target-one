using Core;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public TileWalker TileWalker;
    public void DoInteraction()
    {
        if (!TileWalker.CurrentTile) 
            return;
        
        // If there is a button
        var tileButtonTrigger = TileWalker.CurrentTile.GetComponent<TriggerTileButton>();
        tileButtonTrigger?.HitTriggerPress(TileWalker);
    }
}
