using UnityEngine;
using UnityEngine.Assertions;

namespace TowerGenerator.ChunkImporter
{
    public class TileChunkCooker : ChunkCookerDefault
    {
        protected override void ApplyColliders(GameObject semifinishedEnt, ChunkImportState chunkImportInformation)
        {
            var renders = semifinishedEnt.GetComponentsInChildren<Renderer>();
            Assert.IsTrue(renders.Length > 0, "No renderers found in the GameObject.");

            foreach (var render in renders)
            {
                if (!render.gameObject.name.StartsWith("Tile"))
                    continue;
                if (render.gameObject.GetComponent<IgnoreAddCollider>() != null)
                    continue;
                if (render.gameObject.GetComponent<MeshCollider>() != null)
                    continue;
                if (render.gameObject.GetComponent<BoxCollider>() != null)
                    continue;

                // Create a new GameObject named "Tile" and move render.gameObject under it
                var tileGameObject = new GameObject(render.gameObject.name);
                tileGameObject.AddComponent<Tile>();
                tileGameObject.transform.SetParent(render.gameObject.transform.parent); // Maintain original hierarchy
                tileGameObject.transform.localPosition = render.gameObject.transform.localPosition;
                tileGameObject.transform.localRotation = render.gameObject.transform.localRotation;
                tileGameObject.transform.localScale = render.gameObject.transform.localScale;

                // Reparent the original render.gameObject under the new Tile GameObject
                render.gameObject.transform.SetParent(tileGameObject.transform);
                render.gameObject.name = "Visual"; // Rename render.gameObject to "Visual"

                // Add SphereCollider to the Tile GameObject (not the Visual GameObject)
                var sphereCollider = tileGameObject.AddComponent<SphereCollider>();
                chunkImportInformation.CollidersApplied++;

                // Calculate the bounding box of the mesh for the SphereCollider
                var meshFilter = render.gameObject.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                    continue;

                Mesh mesh = meshFilter.sharedMesh;
                Bounds bounds = mesh.bounds;

                sphereCollider.center = bounds.center;
                sphereCollider.radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            }
        }
    }
}
