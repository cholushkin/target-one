using UnityEngine;
using Random = UnityEngine.Random;

public class LevChunkSpawnEffectContainer : MonoBehaviour
{
    public bool PlayRandomEffectOnStart;
    private LevChunkSpawnEffectBase[] _levChunkSpawnEffects;
    
    void Awake()
    {
        _levChunkSpawnEffects = GetComponentsInChildren<LevChunkSpawnEffectBase>();
        foreach (var effect in _levChunkSpawnEffects)
            effect.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (PlayRandomEffectOnStart)
        {
            var effect = _levChunkSpawnEffects[Random.Range(0, _levChunkSpawnEffects.Length)];
            effect.gameObject.SetActive(true);
        }
    }
}
