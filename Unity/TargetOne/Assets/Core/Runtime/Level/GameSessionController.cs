using Core;
using GameLib.Alg;
using NaughtyAttributes;
using UnityEngine.Assertions;

public class GameSessionController : Singleton<GameSessionController>
{
    [Required]
    public TileWalker Walker;
    [Required]
    public LevelGenerator LevelGenerator;
    public float GameSpeed = 2;

    public void Start()
    {
        var startingTile = LevelGenerator.SpawningTile;
        Assert.IsNotNull(startingTile);
        Walker.Init(startingTile);
    }
    

}
