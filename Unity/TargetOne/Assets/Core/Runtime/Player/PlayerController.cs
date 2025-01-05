using Core;
using Events;
using NaughtyAttributes;
using UnityEngine;

public class PlayerController :
    MonoBehaviour,
    IHandle<TileWalker.EventWalkerReachTileCenter>,
    IHandle<TileWalker.EventStartFalling>,
    IHandle<TileWalker.EventFallRecover>
{
    [Required] public TileWalker TileWalker;
    [Required] public CharacterAnimator CharacterAnimator;
    [Required] public CharacterHover Hover;
    [Required] public Transform Visual;

    public bool AutoWalker;
    private bool _isInteractionEnabled;


    public void Awake()
    {
        Init();
    }
    
    public void Handle(TileWalker.EventStartFalling message)
    {
        Hover.Set(Hover.WakeUpValues.amp, Hover.WakeUpValues.height, message.Duration);
    }

    public void Handle(TileWalker.EventFallRecover message)
    {
        Hover.Set(Hover.NormalValues.amp, Hover.NormalValues.height, message.Duration);
    }

    private void Init()
    {
        GlobalEventAggregator.EventAggregator.Subscribe(this);
        EnableInteractions(true);
    }

    public void EnableInteractions(bool flag)
    {
        _isInteractionEnabled = flag;
    }

    public void EnableVisual(bool flag)
    {
        Visual.gameObject.SetActive(flag);
    }

    public void DoInteraction()
    {
        if (!TileWalker.CurrentTile)
            return;
        if (!_isInteractionEnabled)
            return;

        // If there is a button press it
        TileWalker.CurrentTile.GetComponent<TriggerTileButton>()?.Trigger(TileWalker);
        
        // If there is a teleport activate it
        TileWalker.CurrentTile.GetComponent<TriggerTubeTeleport>()?.Trigger(TileWalker);
    }

    public void Handle(TileWalker.EventWalkerReachTileCenter message)
    {
        if (AutoWalker)
        {
            DoInteraction();
        }
    }
}