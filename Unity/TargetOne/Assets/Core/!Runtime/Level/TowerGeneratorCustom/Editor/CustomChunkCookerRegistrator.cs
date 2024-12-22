using UnityEditor;

namespace TowerGenerator.ChunkImporter
{
    
    [InitializeOnLoad]
    public static class CustomChunkCookerRegistrator
    {
        static CustomChunkCookerRegistrator()
        {
            ChunkCookerFactory.RegisterChunkCooker(new ChunkCookerFactory.RegistrationEntry { Name = "TileChunkCooker", Creator = () => new TileChunkCooker() });
        }
    }
}