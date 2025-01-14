using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core;
using GameLib.Alg;
using GameLib.Random;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using RangeInt = GameLib.Random.RangeInt;

public class LevelGenerator : Singleton<LevelGenerator>
{
    public class EventLevelLoaded
    {
    }
    
    #region Unity inspector
    
    public Transform LevelGeneratorPointer; // Pointer marking the current level generation position
    public TileWalker Walker; // Entity controlling the generator's movement
    [Tooltip("Radius within which level chunks will be generated")]
    public float GeneratorPointerRadius;
    public Transform LevelParent; // Parent object to organize level chunks
    [Tooltip("There are a limited number of checkpoint chunks (unlike regular LevChunks), therefore we can list them here")]
    public GameObject[] CheckpointLevChunks;
    
    #endregion

    
    public Tile StartingTile { get; private set; } // The tile where character will be spawned

    // Variables related to the current segment
    private SegmentConfiguration _currentSegmentConfiguration; // Configuration for the current segment
    private int _chunksAmountInSegment; // Total number of chunks in this segment
    private int _chunkInstantiatedInSegment;
    private bool _isRegenerated; // In case if player can't complete the segment, we allow to regenerate it without pickup-items
    private long _currentSegmentIndex;
    private float[] _levChunksProbsSegment;
    private IPseudoRandomNumberGenerator _segmentRnd;

    public Vector3 GetGeneratorPointerPosition => transform.position; // Position of the generator pointer
    private HashSet<GameObject> _createdChunks; // Set of currently instantiated chunks
    private GameObject _lastCreatedChunk; // Reference to the most recently created chunk
    private Vector3 _lastChunkExit; // Position of the last chunk's exit point
    private bool _sessionFirstPackOfChunks; // Tracks whether the first set of chunks is being generated
    
    private CancellationTokenSource _cancellationTokenSource; // Token source for canceling asynchronous operations


    private void OnDestroy()
    {
        StopGenerate(); // Ensure generation stops and resources are cleaned up when the generator is destroyed
    }

    public void StartGenerate()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        InitiateNewGeneration();
        StartLevelGenerationAsync(_cancellationTokenSource.Token).Forget(); // Start asynchronous level generation
    }

    public void StopGenerate()
    {
        Purge(); // Clean up all generated chunks
        _cancellationTokenSource?.Cancel(); // Cancel ongoing generation tasks
        _cancellationTokenSource?.Dispose(); // Dispose of the cancellation token source
    }
    
    private void InitiateNewGeneration()
    {
        _lastChunkExit = Vector3.zero;
        _createdChunks = new HashSet<GameObject>();
        _sessionFirstPackOfChunks = false;
        _lastCreatedChunk = null;
    }

    private async UniTaskVoid StartLevelGenerationAsync(CancellationToken cancellationToken)
    {
        // Continuously generate chunks and clean up in an asynchronous loop
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_sessionFirstPackOfChunks)
            {
                _sessionFirstPackOfChunks = false;
                GlobalEventAggregator.EventAggregator.Publish(new EventLevelLoaded()); // Notify that the level has loaded
            }

            UpdateLevelGeneratorPointer(); // Update the position of the generator pointer
            await GenerateLevelChunks(); // Generate chunks based on the current configuration
            //await CleanupChunks(); // Optionally clean up distant chunks

            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken); // Yield to prevent blocking the main thread
        }
        Debug.Log("Level generation stopped."); // Log when generation is canceled
    }

    private void UpdateLevelGeneratorPointer()
    {
        // Update the generator pointer position based on the walker's position
        LevelGeneratorPointer.position = new Vector3(
            Mathf.Max(Walker.transform.position.x, LevelGeneratorPointer.position.x),
            Walker.transform.position.y, 
            Walker.transform.position.z);
    }

    private async UniTask CleanupChunks()
    {
        Vector3 generatorPosition = LevelGeneratorPointer.position;
        List<GameObject> chunksToRemove = new List<GameObject>(); // Chunks to remove based on distance

        foreach (var chunk in _createdChunks)
        {
            // Identify chunks outside the generation radius
            if (chunk == null || Vector3.Distance(chunk.transform.position, generatorPosition) > GeneratorPointerRadius)
            {
                chunksToRemove.Add(chunk);
            }
        }

        foreach (var chunk in chunksToRemove)
        {
            _createdChunks.Remove(chunk);
            Destroy(chunk); // Destroy or deactivate chunks
            await UniTask.Yield(PlayerLoopTiming.Update); // Yield to avoid blocking
        }
    }

    private void Purge()
    {
        // Destroy all active chunks and clear the set
        foreach (var chunk in _createdChunks)
            if (chunk != null)
                Destroy(chunk);
        _createdChunks.Clear();
    }

    private async UniTask GenerateLevelChunks()
    {
        var isFirstEverChunkInSession = _lastCreatedChunk == null;
        
        // Generate new chunks until the pointer is within the generation radius
        while (Vector3.Distance(_lastChunkExit, LevelGeneratorPointer.position) < GeneratorPointerRadius)
        {
            var newLevChunkPrefab = GetNextLevChunkPrefab(); // Get the next chunk prefab
            Assert.IsNotNull(newLevChunkPrefab);
            Debug.Log($"Spawning level chunk: {newLevChunkPrefab.name}, frame:{Time.frameCount}");
            var newChunk = InstantiateChunk(newLevChunkPrefab, -1, LevelParent, _lastChunkExit); // Instantiate the chunk
            _chunkInstantiatedInSegment++;
            Assert.IsNotNull(newChunk);

            await UniTask.Yield(PlayerLoopTiming.Update); // Yield to let the chunk fully instantiate

            var newChunkEntry = GetRandomEntry(newChunk); // Find an entry point in the chunk
            var newChunkExit = GetRandomExit(newChunk); // Find an exit point in the chunk

            Debug.Assert(newChunkEntry);
            Debug.Assert(newChunkExit);

            // Position the new chunk to align with the previous exit
            var offset = _lastChunkExit - newChunkEntry.transform.position;
            newChunk.transform.position += offset + Vector3.right * Tile.TileSize;

            newChunk.GetComponent<TriggerLevChunkSpawn>()?.Trigger(); // Trigger events associated with the new chunk

            if (_lastCreatedChunk == null)
            {
                _sessionFirstPackOfChunks = true; // Mark the start of the first chunk generation
                StartingTile = newChunkEntry.GetComponent<Tile>();
            }

            _lastCreatedChunk = newChunk;
            _lastChunkExit = newChunkExit.transform.position;
            _createdChunks.Add(newChunk); // Add the new chunk to the active set
        }
    }

    private LevChunkExit GetRandomExit(GameObject newChunk)
    {
        // Find and return a random active exit point in the chunk
        var activeExits = newChunk.GetComponentsInChildren<LevChunkExit>()
            .Where(exit => exit.gameObject.activeInHierarchy)
            .ToArray();

        if (activeExits.Length == 0)
        {
            Debug.LogError($"No active exit found in chunk {newChunk.name}.");
            return null;
        }

        return _segmentRnd.FromArray(activeExits); // move rnd
    }

    private LevChunkEntry GetRandomEntry(GameObject newChunk)
    {
        // Find and return a random active entry point in the chunk
        var activeEntries = newChunk.GetComponentsInChildren<LevChunkEntry>()
            .Where(entry => entry.gameObject.activeInHierarchy)
            .ToArray();

        if (activeEntries.Length == 0)
        {
            Debug.LogError($"No active entry found in chunk {newChunk.name}.");
            return null;
        }

        return _segmentRnd.FromArray(activeEntries); // move rnd
    }

    public void SetCurrentSegment(long segmentIndex)
    {
        _currentSegmentIndex = segmentIndex;
        _currentSegmentConfiguration = SegmentConfiguration.LoadSegmentConfiguration(segmentIndex);
        if (_currentSegmentConfiguration == null)
        {
            Debug.Log("No SegmentConfiguration found, loading the default one");
            // todo: procedurally prepare segment configuration based on segmentIndex
            _currentSegmentConfiguration = SegmentConfiguration.CreateDefaultSegmentConfiguration(segmentIndex);
        }

        if (_currentSegmentConfiguration.Seed == -1)
            _currentSegmentConfiguration.Seed = _currentSegmentConfiguration.SegmentID;
        
        foreach (var chunkConfig in _currentSegmentConfiguration.ChunksPool)
            if (chunkConfig.Seed == -1)
                chunkConfig.Seed = _currentSegmentConfiguration.SegmentID;

        _segmentRnd = RandomHelper.CreateRandomNumberGenerator(_currentSegmentConfiguration.Seed); 
        _chunksAmountInSegment = _segmentRnd.FromRangeIntInclusive(_currentSegmentConfiguration.ChunksNumber); // move rnd
        _chunkInstantiatedInSegment = 0;
        _isRegenerated = false;
        
        // Prepare probs array
        _levChunksProbsSegment = new float[_currentSegmentConfiguration.ChunksPool.Length];
        for (int i = 0; i < _currentSegmentConfiguration.ChunksPool.Length; ++i)
            _levChunksProbsSegment[i] = _currentSegmentConfiguration.ChunksPool[i].Probability;
    }

    private GameObject GetNextLevChunkPrefab()
    {
        // First LevChunk of a segment
        if (_chunkInstantiatedInSegment == 0)
        {
            // First LevChunk in a segment is always a checkpoint
            return _segmentRnd.FromArray(CheckpointLevChunks); // move rnd
        }
        
        // Last LevChunk of a segment
        else if (_chunkInstantiatedInSegment > _chunksAmountInSegment)
        {
            // Time to switch to a next segment
            SetCurrentSegment(++_currentSegmentIndex);
            return GetNextLevChunkPrefab();
        }

        // Randomly pick LevChunk using probability 
        var levChunkPoolIndex = _segmentRnd.SpawnEvent(_levChunksProbsSegment); // move rnd
        var levChunkName = _currentSegmentConfiguration.ChunksPool[levChunkPoolIndex].ChunkName;
        
        // Load LevChunk prefab
        return Resources.Load(levChunkName) as GameObject;
    }

    public GameObject InstantiateChunk(GameObject prefab, long seed, Transform parent, Vector3 connectionPoint)
    {
        // Instantiate a chunk and set its initial position
        var chunk = Instantiate(prefab, parent);
        chunk.transform.position = connectionPoint;
        chunk.name = prefab.name;
        return chunk;
    }

    private void OnDrawGizmos()
    {
        if (!LogChecker.Gizmos)
            return;

        // Draw a wireframe sphere to represent the generation radius in the Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, GeneratorPointerRadius);
    }
}
