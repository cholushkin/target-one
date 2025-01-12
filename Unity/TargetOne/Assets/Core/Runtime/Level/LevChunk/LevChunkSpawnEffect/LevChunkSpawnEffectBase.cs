using UnityEngine;

public class LevChunkSpawnEffectBase : MonoBehaviour
{
    public Transform LevChunk;
    public bool PlayOnStart;
    protected Tile[] _tiles;

    public void Awake()
    {
        GatherTiles();
    }

    public void Start()
    {
        if(PlayOnStart)
            PlayEffect();
    }
    
    public virtual void PlayEffect()
    {
    }

    protected void GatherTiles()
    {
        _tiles = LevChunk.GetComponentsInChildren<Tile>();
    }
}
