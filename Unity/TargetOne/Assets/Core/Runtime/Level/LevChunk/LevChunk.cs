using GameLib.Alg;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class LevChunk : MonoBehaviour
{
    public LevChunkCamerasContainer Cameras;
    public References References;


    private void OnValidate()
    {
        var chunkGameObject = transform.gameObject;
        var levChunk = chunkGameObject.GetComponent<LevChunk>();
        
        // Check LevChunk
        Assert.IsNotNull(levChunk, transform.GetDebugName());
        Assert.IsNotNull(levChunk.References, transform.GetDebugName());
        Assert.IsNotNull(levChunk.Cameras, transform.GetDebugName());
        
        // Trigger levchunkspawn
        var triggerLevChunkSpawn = chunkGameObject.GetComponent<TriggerLevChunkSpawn>();
        Assert.IsNotNull(triggerLevChunkSpawn, transform.GetDebugName());
        Assert.IsNotNull(triggerLevChunkSpawn.LevChunk, transform.GetDebugName());
        Assert.IsTrue(triggerLevChunkSpawn.MaxHitCount == 1, transform.GetDebugName());
        
        // Entries
        var triggerTileLevChunkEntries = transform.GetComponentsInChildren<TriggerTileLevChunkEnter>();
        var levChunkEntries = transform.GetComponentsInChildren<LevChunkEntry>();
        Assert.IsTrue(triggerTileLevChunkEntries.Length >= 1, transform.GetDebugName());
        Assert.IsTrue(levChunkEntries.Length >= 1, transform.GetDebugName());
        Assert.IsTrue(triggerTileLevChunkEntries.Length == levChunkEntries.Length);
        foreach (var entry in triggerTileLevChunkEntries)
        {
            Assert.IsNotNull(entry.LevChunkEnter );
            Assert.IsTrue(entry.MaxHitCount == -1 );
        }

        // Exits
        var levChunkExits = transform.GetComponentsInChildren<LevChunkExit>();
        if (levChunkExits.Length > 0) // some thunks could have no exit
        {
            var triggerTileLevChunksExits = transform.GetComponentsInChildren<TriggerTileLevChunkExit>();
            Assert.IsTrue(triggerTileLevChunksExits.Length >= 1, transform.GetDebugName());
            Assert.IsTrue(levChunkExits.Length >= 1, transform.GetDebugName());
            Assert.IsTrue(triggerTileLevChunksExits.Length == levChunkExits.Length);
            foreach (var exit in triggerTileLevChunksExits)
            {
                Assert.IsNotNull(exit.LevChunkExit);
                Assert.IsTrue(exit.MaxHitCount == -1);
            }
        }

        Debug.Log($"Validation completed for {transform.GetDebugName()}.");
    }

#if UNITY_EDITOR
    [Button]
    void ConfigureLevChunk()
    {
        var chunkGameObject = transform.gameObject;

        // Check necessary components and add them if needed
        var levChunk = chunkGameObject.AddSingleComponentSafe<LevChunk>();
        var triggerLevChunkSpawn = chunkGameObject.AddSingleComponentSafe<TriggerLevChunkSpawn>();

        // Configure added components
        triggerLevChunkSpawn.MaxHitCount = 1;
        triggerLevChunkSpawn.LevChunk = chunkGameObject;

        // Add References
        levChunk.References = AddPrefabToChunk<References>("LevChunkReferences", chunkGameObject);

        // Add cameras container
        levChunk.Cameras = AddPrefabToChunk<LevChunkCamerasContainer>("CinamachineCamerasContainer", chunkGameObject);
        
        Debug.Log("Processed: <LevChunk> <TriggerLevChunkSpawn> LevChunkReferences CinamachineCamerasContainer");

        // Configure tiles
        ConfigureTiles(transform.gameObject);
    }

    private void ConfigureTiles(GameObject levChunk)
    {
        // Some stats params
        bool isAdded = false;
        int tilesCountAdded = 0;
        int tilesCount = 0;
        int entriesCountAdded = 0;
        int entriesCount = 0;
        int exitsCountAdded = 0;
        int exitsCount = 0;
        int sphereColliderCountAdded = 0;
        int sphereColliderCount = 0;

        levChunk.transform.ForEachChildrenRecursive(t =>
        {
            var tileGameObject = t.gameObject;

            // Check if the object's name starts with "Tile"
            if (!tileGameObject.name.StartsWith("Tile"))
                return;

            // Process Tile
            Tile tile = tileGameObject.AddSingleComponentSafe<Tile>(out isAdded);
            if (isAdded)
                ++tilesCountAdded;
            ++tilesCount;

            // Process Entry
            LevChunkEntry levChunkEntry = null;
            if (tileGameObject.name.Contains("Entry"))
            {
                levChunkEntry = tileGameObject.AddSingleComponentSafe<LevChunkEntry>(out isAdded);
                if (isAdded)
                    ++entriesCountAdded;
                ++entriesCount;
                var triggerLevChunkEnter = tileGameObject.AddSingleComponentSafe<TriggerTileLevChunkEnter>();
                triggerLevChunkEnter.LevChunkEnter = levChunk.GetComponent<LevChunk>();
            }

            // Process Exit
            LevChunkExit levChunkExit = null;
            if (tileGameObject.name.Contains("Exit"))
            {
                levChunkExit = tileGameObject.AddSingleComponentSafe<LevChunkExit>(out isAdded);
                if (isAdded)
                    ++exitsCountAdded;
                ++exitsCount;
                var triggerLevChunkExit = tileGameObject.AddSingleComponentSafe<TriggerTileLevChunkExit>();
                triggerLevChunkExit.LevChunkExit = levChunk.GetComponent<LevChunk>();
            }

            // Process Visual
            Transform visualTransform = tileGameObject.transform.FirstChildNameStartsWith("Visual", false);

            if (visualTransform == null)
            {
                Debug.LogError($"Tile '{t.GetDebugName()}' does not have a nested object whose name starts with 'Visual'.");
                return;
            }

            tile.Visual = visualTransform;

            // Process SphereCollider
            var sphereCollider = tileGameObject.AddSingleComponentSafe<SphereCollider>(out isAdded);
            var meshFilter = visualTransform.GetComponent<MeshFilter>();

            if (isAdded)
                ++sphereColliderCountAdded;
            ++sphereColliderCount;

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError($"Tile '{t.GetDebugName()}' does not have a valid MeshFilter or Mesh on Tile/Visual.");
                return;
            }

            // Calculate the bounding box of the mesh for the SphereCollider
            Mesh mesh = meshFilter.sharedMesh;
            Bounds bounds = mesh.bounds;
            sphereCollider.center = bounds.center;
            sphereCollider.radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        });

        Debug.Log(
            $"Tiles configuration completed: tilesCountAdded:{tilesCountAdded} tilesCount:{tilesCount} entriesCountAdded:{entriesCountAdded} entriesCount:{entriesCount} exitsCountAdded:{exitsCountAdded} exitsCount:{exitsCount} sphereColliderCountAdded:{sphereColliderCountAdded} sphereColliderCount:{sphereColliderCount}");
    }


    // Adds a prefab to the specified parent GameObject if it doesn't already exist.
    private T AddPrefabToChunk<T>(string resourcePath, GameObject parentObject) where T : Component
    {
        // Check if the component already exists
        var existingComponent = parentObject.GetComponentInChildren<T>();
        if (existingComponent != null)
        {
            return existingComponent;
        }

        // Load the prefab from Resources
        var prefab = Resources.Load<T>(resourcePath);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found at path: {resourcePath}");
            return null;
        }

        var instance = (T)PrefabUtility.InstantiatePrefab(prefab);
        if (instance == null)
        {
            Debug.LogError($"Failed to instantiate prefab: {prefab.name}");
            return null;
        }

        // Configure the instance
        instance.name = prefab.name; // Use the prefab's name for the instance
        instance.transform.SetParent(parentObject.transform);

        Debug.Log($"Successfully added prefab '{instance.name}' to {parentObject.name}");
        return instance;
    }

#endif
}