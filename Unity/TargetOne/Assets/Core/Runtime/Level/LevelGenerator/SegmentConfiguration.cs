using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Range = GameLib.Random.Range;

[Serializable]
public class SegmentConfiguration
{
    [Serializable]
    public class ChunkConfig
    {
        public string ChunkName; // Name of the chunk used from the pool
        public float Probability; // Chance of this chunk appearing in the segment
        public long Seed; // Specific starting seed for this chunk (-1 to use the segment seed)
    }

    public long Seed; // Specific seed for this segment (-1 to use seed based on SegmentID)
    public long SegmentID; // Sequential identifier for the segment
    public string FantasySetting; // Visual or thematic setting of the segment
    public Range ChunksNumber; // Range for the number of chunks to generate in this segment
    public ChunkConfig[] ChunksPool; // Pool of available chunks for generation in this segment


    public static SegmentConfiguration CreateDefaultSegmentConfiguration(long segmentID)
    {
        var segmentConfiguration = new SegmentConfiguration();
        
        segmentConfiguration.Seed = (long)(Int32.MaxValue * Random.value);
        segmentConfiguration.SegmentID = segmentID;
        segmentConfiguration.FantasySetting = FantasySettingsManager.DefaultFantasySettingName;
        segmentConfiguration.ChunksNumber = new Range(6, 8);
        segmentConfiguration.ChunksPool = new[]
        {
            new ChunkConfig{ChunkName = "ChunkA", Probability = 1f, Seed = -1},
            new ChunkConfig{ChunkName = "ChunkB", Probability = 1f, Seed = -1},
            new ChunkConfig{ChunkName = "ChunkC", Probability = 1f, Seed = -1},
            new ChunkConfig{ChunkName = "ChunkD", Probability = 1f, Seed = -1},
            new ChunkConfig{ChunkName = "ChunkE", Probability = 1f, Seed = -1}
        };
        
        return segmentConfiguration;
    }
    
    public static SegmentConfiguration LoadSegmentConfiguration(long segmentID)
    {
        // Load a segment configuration from a JSON file in Resources
        TextAsset jsonFile = Resources.Load<TextAsset>($"s{segmentID}");

        if (jsonFile == null)
        {
            Debug.LogError($"s{segmentID}.json file not found in Resources.");
            return null;
        }

        SegmentConfiguration tempConfig = JsonUtility.FromJson<SegmentConfiguration>(jsonFile.text);

        // Expand multiple chunk names in the configuration
        List<ChunkConfig> processedChunks = new List<SegmentConfiguration.ChunkConfig>();

        foreach (var chunk in tempConfig.ChunksPool)
        {
            string[] chunkNames = chunk.ChunkName.Split(',');

            foreach (string name in chunkNames)
            {
                processedChunks.Add(new ChunkConfig
                {
                    ChunkName = name.Trim(),
                    Probability = chunk.Probability,
                    Seed = chunk.Seed
                });
            }
        }

        tempConfig.ChunksPool = processedChunks.ToArray();
        return tempConfig;
    }
}