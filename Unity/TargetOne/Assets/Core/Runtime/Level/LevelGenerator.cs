using System;
using System.Linq;
using System.Threading;
using Core;
using GameLib.Alg;
using GameLib.Random;
using UnityEngine;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

public class LevelGenerator : Singleton<LevelGenerator>
{
    [Serializable]
    public class SegmentConfiguration
    {
        public long Seed;
        public int SegmentID;
        public GameObject[] ChunksPool;    
    }

    public Transform LevelGeneratorPointer;
    public TileWalker Walker;
    [Tooltip("Level will be generated within radius")]
    public float GeneratorPointerRadius;

    public SegmentConfiguration CurrentSegment;
    
    public Vector3 GetGeneratorPointerPosition => transform.position;

    public Transform LevelParent;
    public GameObject PrefabTest;
    public Tile SpawningTile { get; private set; }

    private GameObject _lastCreated;
    private IPseudoRandomNumberGenerator _rnd = RandomHelper.CreateRandomNumberGenerator();
    private CancellationTokenSource _cancellationTokenSource;

    void Awake()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        StartLevelGenerationAsync(_cancellationTokenSource.Token).Forget();
    }

    private void OnDestroy()
    {
        Stop();
    }

    void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
    
    private async UniTaskVoid StartLevelGenerationAsync(CancellationToken cancellationToken)
    {
        // Generate initial chunks
        GenerateLevelChunks().Forget(); // First frame we create all chunks (loading stage, performance wise heavy)

        // Async loop for continuous chunk generation and cleanup
        while (!cancellationToken.IsCancellationRequested)
        {
            UpdateLevelGeneratorPointer();
            await GenerateLevelChunks();
            CleanupChunks();

            // Yield control back to the main thread to avoid blocking
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        // Perform cleanup or log the cancellation
        Debug.Log("Level generation stopped.");
    }


    private void UpdateLevelGeneratorPointer()
    {
        LevelGeneratorPointer.position =
            new Vector3(Mathf.Max(Walker.transform.position.x, LevelGeneratorPointer.position.x),
                Walker.transform.position.y, Walker.transform.position.z);
    }

    private void CleanupChunks()
    {
        // Implement cleanup logic to remove chunks outside the radius
    }

    private async UniTask GenerateLevelChunks()
    {
        var exitPoint = Vector3.zero;
        do
        {
            // Get exit point for the last created chunk
            if (_lastCreated)
            {
                // Filter only active PatternExit components
                var activeExits = _lastCreated
                    .GetComponentsInChildren<PatternExit>()
                    .Where(exit => exit.gameObject.activeInHierarchy)
                    .ToArray();

                if (activeExits.Length == 0)
                {
                    Debug.LogError($"No active PatternExit found in {_lastCreated.name} chunk.");
                    return;
                }

                exitPoint = _rnd.FromArray(activeExits).transform.position;
            }

            if (Vector3.Distance(exitPoint, LevelGeneratorPointer.position) > GeneratorPointerRadius)
                return;

            var newChunk = InstantiateChunk(PrefabTest, -1, LevelParent, exitPoint);
            Assert.IsNotNull(newChunk);
            UniTask.Yield();
            
            // Fit entry with exit
            var activeEntries = newChunk
                .GetComponentsInChildren<PatternEntry>()
                .Where(e => e.gameObject.activeInHierarchy)
                .ToArray();
            
            if (activeEntries.Length == 0)
            {
                Debug.LogError($"No active PatternEntry found in the new chunk {newChunk.name}.");
                return;
            }

            var entry = _rnd.FromArray(activeEntries);
            var entryPoint = entry.transform.position;
            var offset = exitPoint - entryPoint;
            newChunk.transform.position += offset + Vector3.right*Tile.TileSize;
            
            // Assign spawning tile
            if (!_lastCreated)
                SpawningTile = entry.GetComponent<Tile>();
                
            _lastCreated = newChunk; 

        } while (Vector3.Distance(exitPoint, LevelGeneratorPointer.position) < GeneratorPointerRadius);
    }

    public GameObject InstantiateChunk(GameObject prefab, long seed, Transform parent, Vector3 connectionPoint)
    {
        // Instantiate and position the chunk
        var chunk = Instantiate(prefab, parent);
        chunk.transform.position = connectionPoint;
        chunk.name = prefab.name;
        return chunk;
    }

    private void OnDrawGizmos()
    {
        if (!LogChecker.Gizmos)
            return;

        // Draw the generator pointer radius in the scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GeneratorPointerRadius);
    }
}
