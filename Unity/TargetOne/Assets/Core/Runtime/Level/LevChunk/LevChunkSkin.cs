using UnityEngine;
using UnityEngine.Rendering;

public class LevChunkSkin : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Active Render Pipeline Asset: " + GraphicsSettings.defaultRenderPipeline.name);
        var tiles = gameObject.GetComponentsInChildren<Tile>();
        foreach (var tile in tiles)
        {
            var visual = tile.transform.GetChild(0);

            // Destroy the existing child (visual)
            if (visual != null)
            {
                Object.DestroyImmediate(visual.gameObject);
            }

            var tilePrefab = FantasySettingsManager.Instance.FanatasySettings[0]
                .TileVisualVariations[Random.Range(0, 4)];

            // Instantiate the prefab and parent it to the tile
            var newVisual = Object.Instantiate(tilePrefab, tile.transform);
            newVisual.transform.localPosition = Vector3.zero; // Optional: Reset position
            newVisual.transform.localRotation = Quaternion.identity; // Optional: Reset rotation
            newVisual.transform.localScale = Vector3.one; // Optional: Reset scale
            
            // Slightly scale the new visual
            // float scaleVariation = Random.Range(0.9f, 1.1f); // Random scale between 90% and 110%
            // newVisual.transform.localScale = Vector3.one * scaleVariation;
            
            // Apply a random rotation (0°, 90°, 180°, 270°)
            int[] rotations = { 0, 90, 180, 270 };
            int randomRotation = rotations[Random.Range(0, rotations.Length)];
            newVisual.transform.localRotation = Quaternion.Euler(0, 0, randomRotation);

        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}