using Core;
using Events;
using GameLib.Alg;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

public class GameSessionController : Singleton<GameSessionController>
    , IHandle<LevelGenerator.EventLevelLoaded>
    , IHandle<TriggerLevChunkEnter.EventTriggerLevChunkEnter>
{
    [Required]
    public TileWalker Walker;
    [Required]
    public LevelGenerator LevelGenerator;
    public float GameSpeed = 2;
    public long LastCheckPoint;
    public LevChunk CurrentLevChunk { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        GlobalEventAggregator.EventAggregator.Subscribe(this);
    }

    public void StartSession()
    {
        Debug.Log($"Session start, frame {Time.frameCount}");
        var startingTile = LevelGenerator.SpawningTile;
        Assert.IsNotNull(startingTile);
        Walker.Init(startingTile);
        Walker.GoToState(TileWalker.State.Walking);
    }

    public void Handle(LevelGenerator.EventLevelLoaded message)
    {
        StartSession();
    }

    public void Handle(TriggerLevChunkEnter.EventTriggerLevChunkEnter message)
    {
        CurrentLevChunk = message.LevChunkEnter;
    }
}