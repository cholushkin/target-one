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
    private Vector3 _lastChunkExit;
    private bool _sessionFirstPackOfChunks;
    private readonly IPseudoRandomNumberGenerator _rnd = RandomHelper.CreateRandomNumberGenerator();
    private CancellationTokenSource _cancellationTokenSource;

    protected override void Awake()
    {
        base.Awake();
        DeserializeGeneratorState();
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
            //await CleanupChunks();

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

        // Remove the chunks
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
        while (Vector3.Distance(_lastChunkExit, LevelGeneratorPointer.position) < GeneratorPointerRadius)
        {
            // Spawn next chunk
            var newChunkPrefab = GetNextChunkPrefab();
            Debug.Log($"Spawning level chunk: {newChunkPrefab.name}, frame:{Time.frameCount}");
            var newChunk = InstantiateChunk(newChunkPrefab, -1, LevelParent, _lastChunkExit);
            Assert.IsNotNull(newChunk);

            await UniTask.Yield(PlayerLoopTiming
                .Update); // Skip frame to have a complete instance of the chunk on the next frame

            // Get random entry 
            var newChunkEntry = GetRandomEntry(newChunk);

            // Get random exit
            var newChunkExit = GetRandomExit(newChunk);

            Debug.Assert(newChunkEntry);
            Debug.Assert(newChunkExit);

            // Fit position of new chunk to previous exit
            var offset = _lastChunkExit - newChunkEntry.transform.position;
            newChunk.transform.position += offset + Vector3.right * Tile.TileSize;

            // Trigger Level Chunk Spawn event
            newChunk.GetComponent<TriggerLevChunkSpawn>()?.Trigger();

            // Session first pack of chunks started to generate 
            if (_lastCreatedChunk == null)
            {
                _sessionFirstPackOfChunks = true;
                SpawningTile = newChunkEntry.GetComponent<Tile>();
            }

            _lastCreatedChunk = newChunk;
            _lastChunkExit = newChunkExit.transform.position;
            _createdChunks.Add(newChunk);
        }
    }

    private LevChunkExit GetRandomExit(GameObject newChunk)
    {
        var activeExits = newChunk
            .GetComponentsInChildren<LevChunkExit>()
            .Where(exit => exit.gameObject.activeInHierarchy)
            .ToArray();

        if (activeExits.Length == 0)
        {
            Debug.LogError($"No active PatternExit found in {newChunk.name} chunk.");
            return null;
        }

        return _rnd.FromArray(activeExits);
    }

    private LevChunkEntry GetRandomEntry(GameObject newChunk)
    {
        var activeEntries = newChunk
            .GetComponentsInChildren<LevChunkEntry>()
            .Where(e => e.gameObject.activeInHierarchy)
            .ToArray();

        if (activeEntries.Length == 0)
        {
            Debug.LogError($"No active PatternEntry found in chunk {newChunk.name}.");
            return null;
        }

        return _rnd.FromArray(activeEntries);
    }

    private GameObject GetNextChunkPrefab()
    {
        return PrefabTest;
        // if (_currentSegmentConfiguration == null)
        // {
        //     // Load segment configuration and set variables 
        //     _currentSegmentConfiguration = LoadSegmentConfiguration(_lastSegmentID);
        // }
        //
        // return "";
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