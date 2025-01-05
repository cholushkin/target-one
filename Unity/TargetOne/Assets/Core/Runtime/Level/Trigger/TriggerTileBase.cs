using Core;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class TriggerTileBase : TriggerBase
{
    #region Inspector

    [Header("Trigger Tile Base")]
    [Tooltip("When walker enters (visit) tile we virtually activate the trigger to work only one time for this visit")]
    public bool OneHitMaxPerVisit;

    [Tooltip("Trigger works only within active radius. Active radius is percent of the tile half size")] [Range(0f, 1f)]
    public float ActiveRadius;

    #endregion

    [FormerlySerializedAs("EnterFromTile")] public Tile EnteredFromTile;
    public bool IsWalkerEntered;
    private bool _wasActivatedOnThisVisit;


    public override void Reset()
    {
        base.Reset();
        ActiveRadius = 1f;
        OneHitMaxPerVisit = true;
    }

    // Returns true if actually activated
    public bool Trigger(TileWalker tileWalker)
    {
        Assert.IsNotNull(tileWalker);

        if (!WillActivate())
            return false;

        if (!IsWithinActiveRadius(tileWalker))
            return false;

        if (OneHitMaxPerVisit && _wasActivatedOnThisVisit)
            return false;

        Activate();
        CustomTriggerLogic();
        _wasActivatedOnThisVisit = true;
        return true;
    }


    // Note: user logic / domain logic / business logic should be in handlers.
    // Here you can specify some internal trigger support code for your custom trigger
    public virtual void CustomTriggerLogic()
    {
    }

    public bool IsWithinActiveRadius(TileWalker tileWalker)
    {
        Assert.IsNotNull(tileWalker);
        if (Mathf.Approximately(ActiveRadius, 1f))
            return true;
        var distance = Vector3.Distance(tileWalker.transform.position, transform.position);
        return distance / (Tile.TileSize * 0.5f) <= ActiveRadius;
    }
    
    public void RegisterWalkerEnter(TileWalker tileWalker, Tile fromTile)
    {
        Assert.IsNotNull(tileWalker);
        EnteredFromTile = fromTile;
        IsWalkerEntered = true;
    }
    
    public void RegisterWalkerExit(TileWalker tileWalker)
    {
        Assert.IsNotNull(tileWalker);
        IsWalkerEntered = false;
        _wasActivatedOnThisVisit = false;
        EnteredFromTile = null;
    }
}