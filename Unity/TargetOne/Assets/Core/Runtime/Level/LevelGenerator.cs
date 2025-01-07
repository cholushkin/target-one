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
using RangeInt = GameLib.Random.RangeInt;

public class LevelGenerator : Singleton<LevelGenerator>
{
    [Serializable]
    public class SegmentConfiguration
    {
        [Serializable]
        public class ChunkConfig
        {
            public string ChunkName; // Name of the chunk in the pool
            public float Probability; // Probability to spawn for current segment
            public long Seed; // Some specific chunk seed (or -1 to control from outer seed(SegmentConfiguration.Seed))
        }
        public long Seed; // Some specific Segment seed (or -1 to control from outer seed(LevelGenerator.Seed) 
        public int SegmentID; // More like sequential number 
        public string ColorScheme;
        public string VisualSetting; // fantasy or visual setting of the segment
        public RangeInt ChunksNumber; // Amount of chunks in current segment
        public ChunkConfig[] ChunksPool; // Current segment chunks pool to generate from
    }

    public class EventLevelLoaded
    {
    }

    public Transform LevelGeneratorPointer;
    public TileWalker Walker;

    [Tooltip("Level will be generated within radius")]
    public float GeneratorPointerRadius;
    
    public Transform LevelParent;
    public GameObject PrefabTest;
    
    public Tile SpawningTile { get; private set; }

    // Current segment variables
    private SegmentConfiguration _currentSegmentConfiguration; // We load configs from resources as json files"
    private int _lastSegmentID;
    private long _segmentStartingSeed;
    private int _chunksNumberToGenerateLeft;
    
    public Vector3 GetGeneratorPointerPosition => transform.position;
    private HashSet<GameObject> _createdChunks;
    private GameObject _lastCreatedChunk;
    private bool _sessionFirstPackOfChunks;
    private readonly IPseudoRandomNumberGenerator _rnd = RandomHelper.CreateRandomNumberGenerator();
    private CancellationTokenSource _cancellationTokenSource;

    protected override void Awake()
    {
        base.Awake();
        DeserializeGeneratorState();
        StartGenerate();
    }

    private void OnDestroy()
    {
        StopGenerate();
    }

    public void StartGenerate()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        StartLevelGenerationAsync(_cancellationTokenSource.Token).Forget();
    }

    public void StopGenerate()
    {
        Purge();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    private async UniTaskVoid StartLevelGenerationAsync(CancellationToken cancellationToken)
    {
        _createdChunks = new HashSet<GameObject>();
        
        // Async loop for continuous chunk generation and cleanup
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_sessionFirstPackOfChunks)
            {
                _sessionFirstPackOfChunks = false;
                GlobalEventAggregator.EventAggregator.Publish(new EventLevelLoaded());
            }
            
            UpdateLevelGeneratorPointer();
            await GenerateLevelChunks();
            await CleanupChunks();

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

    
    private async UniTask CleanupChunks()
    {
        Vector3 generatorPosition = LevelGeneratorPointer.position;

        // Collect chunks to be removed
        List<GameObject> chunksToRemove = new List<GameObject>();

        foreach (var chunk in _createdChunks)
        {
            // Check if the chunk is outside the defined radius
            if (chunk == null || Vector3.Distance(chunk.transform.position, generatorPosition) > GeneratorPointerRadius)
            {
                chunksToRemove.Add(chunk);
            }
        }

        // Remove or deactivate the chunks
        foreach (var chunk in chunksToRemove)
        {
            _createdChunks.Remove(chunk);
            Destroy(chunk); // Or chunk.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }
    
    private void Purge()
    {
        foreach (var chunk in _createdChunks)
            if (chunk != null)
                Destroy(chunk);
        _createdChunks.Clear();
    }

    private async UniTask GenerateLevelChunks()
    {
        var exitPoint = Vector3.zero;
        do
        {
            // Get exit point for the last created chunk
            if (_lastCreatedChunk)
            {
                // Filter only active PatternExit components
                var activeExits = _lastCreatedChunk
                    .GetComponentsInChildren<LevChunkExit>()
                    .Where(exit => exit.gameObject.activeInHierarchy)
                    .ToArray();

                if (activeExits.Length == 0)
                {
                    Debug.LogError($"No active PatternExit found in {_lastCreatedChunk.name} chunk.");
                    return;
                }

                exitPoint = _rnd.FromArray(activeExits).transform.position;
            }

            if (Vector3.Distance(exitPoint, LevelGeneratorPointer.position) > GeneratorPointerRadius)
                return;

            Debug.Log($"Chunk spawn {PrefabTest.name}, frame:{Time.frameCount}");
            var nextChunkPrefabName = GetNextChunkPrefabName();
            var newChunk = InstantiateChunk(PrefabTest, -1, LevelParent, exitPoint);
            Assert.IsNotNull(newChunk);
            
            await UniTask.Yield(PlayerLoopTiming.Update); // Skip frame to have a complete instance of the chunk on the next frame
            newChunk.GetComponent<TriggerLevChunkSpawn>()?.Trigger();
            Debug.Log($"Chunk spawned finally, frame:{Time.frameCount}");
            
            // Fit entry 
            var activeEntries = newChunk
                .GetComponentsInChildren<LevChunkEntry>()
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
            
            // Session first pack of chunks started to generate 
            if (!_lastCreatedChunk)
            {
                _sessionFirstPackOfChunks = true;
                SpawningTile = entry.GetComponent<Tile>();
            }

            _lastCreatedChunk = newChunk; 
            _createdChunks.Add(newChunk);

        } while (Vector3.Distance(exitPoint, LevelGeneratorPointer.position) < GeneratorPointerRadius);
    }

    private string GetNextChunkPrefabName()
    {
        if (_currentSegmentConfiguration == null)
        {
            // Load segment configuration and set variables 
            _currentSegmentConfiguration = LoadSegmentConfiguration(_lastSegmentID);
        }

        return "";
    }

    public GameObject InstantiateChunk(GameObject prefab, long seed, Transform parent, Vector3 connectionPoint)
    {
        // Instantiate and position the chunk
        var chunk = Instantiate(prefab, parent);
        chunk.transform.position = connectionPoint;
        chunk.name = prefab.name;
        return chunk;
    }
    
    private void DeserializeGeneratorState()
    {
    }
    
    public static SegmentConfiguration LoadSegmentConfiguration(int segmentID)
    {
        // Load the JSON file from Resources
        TextAsset jsonFile = Resources.Load<TextAsset>($"s{segmentID}");

        if (jsonFile == null)
        {
            Debug.LogError($"s{segmentID}.json file not found in Resources.");
            return null;
        }
        
        SegmentConfiguration tempConfig = JsonUtility.FromJson<SegmentConfiguration>(jsonFile.text);

        // Process the ChunksPool to handle multiple chunk names in one field
        List<SegmentConfiguration.ChunkConfig> processedChunks = new List<SegmentConfiguration.ChunkConfig>();

        foreach (var chunk in tempConfig.ChunksPool)
        {
            string[] chunkNames = chunk.ChunkName.Split(',');

            foreach (string name in chunkNames)
            {
                SegmentConfiguration.ChunkConfig newChunk = new SegmentConfiguration.ChunkConfig
                {
                    ChunkName = name.Trim(),
                    Probability = chunk.Probability,
                    Seed = chunk.Seed
                };

                processedChunks.Add(newChunk);
            }
        }

        // Replace the ChunksPool with the processed list
        tempConfig.ChunksPool = processedChunks.ToArray();

        return tempConfig;
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
