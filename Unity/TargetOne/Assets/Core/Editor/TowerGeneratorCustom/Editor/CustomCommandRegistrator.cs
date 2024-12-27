using UnityEditor;

namespace TowerGenerator.FbxCommands
{

    [InitializeOnLoad]
    public static class CustomCommandRegistrator
    {
        static CustomCommandRegistrator()
        {
            // Example:
            // FbxCommandExecutor.RegisterFbxCommand(new FbxCommandExecutor.CommandRegistrationEntry { Name = "Fractured", Creator = () => new FbxCommandFractured("Fractured") });
        }

    }
}