using System;
using System.Linq;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class LevChunkSkin : MonoBehaviour
{
    public FantasySettings.FantasySettingsType CompatibleFantasy;
    public FantasySettings.FantasySettingsType SelectedFantasySettingsType;
    public bool SlightlyRotateTiles;

    private void Awake()
    {
    }

    // Picks a random FantasySettingsType from the provided source list (srcList) 
    // that is also compatible with the CompatibleFantasy property of the current instance.
    public FantasySettings.FantasySettingsType PickRandomFantasySettings(FantasySettings.FantasySettingsType srcList, Random rng)
    {
        // Filter the srcList to find matching flags in CompatibleFantasy
        var compatibleList = Enum.GetValues(typeof(FantasySettings.FantasySettingsType))
            .Cast<FantasySettings.FantasySettingsType>()
            .Where(fantasy => (srcList & fantasy) != 0 && (CompatibleFantasy & fantasy) != 0)
            .ToList();

        // If there are no compatible settings
        if (compatibleList.Count == 0)
            return FantasySettingsManager.DefaultFantasySettings;

        SelectedFantasySettingsType = rng.FromList(compatibleList);
        return SelectedFantasySettingsType;
    }


    void Start()
    {
        // Debug.Log("Active Render Pipeline Asset: " + GraphicsSettings.defaultRenderPipeline.name);
        // var tiles = gameObject.GetComponentsInChildren<Tile>();
        // foreach (var tile in tiles)
        // {
        //     var visual = tile.transform.GetChild(0);
        //
        //     // Destroy the existing child (visual)
        //     if (visual != null)
        //     {
        //         Object.DestroyImmediate(visual.gameObject);
        //     }
        //
        //     var tilePrefab = FantasySettingsManager.Instance.FanatasySettings[0]
        //         .TileVisualVariations[Random.Range(0, 4)];
        //
        //     // Instantiate the prefab and parent it to the tile
        //     var newVisual = Object.Instantiate(tilePrefab, tile.transform);
        //     newVisual.transform.localPosition = Vector3.zero; // Optional: Reset position
        //     newVisual.transform.localRotation = Quaternion.identity; // Optional: Reset rotation
        //     newVisual.transform.localScale = Vector3.one; // Optional: Reset scale
        //     
        //     // Slightly scale the new visual
        //     // float scaleVariation = Random.Range(0.9f, 1.1f); // Random scale between 90% and 110%
        //     // newVisual.transform.localScale = Vector3.one * scaleVariation;
        //     
        //     // Apply a random rotation (0°, 90°, 180°, 270°)
        //     int[] rotations = { 0, 90, 180, 270 };
        //     int randomRotation = rotations[Random.Range(0, rotations.Length)];
        //     newVisual.transform.localRotation = Quaternion.Euler(0, 0, randomRotation);
        //
        // }
    }
}