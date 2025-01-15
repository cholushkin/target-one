using UnityEditor;

namespace TowerGenerator.FbxCommands
{

    [InitializeOnLoad]
    public static class CustomCommandRegistrator
    {
        static CustomCommandRegistrator()
        {
            FbxCommandExecutor.RegisterFbxCommand(new FbxCommandExecutor.CommandRegistrationEntry { Name = "NonReplaceable", Creator = () => new FbxCommandNonReplaceable("NonReplaceable", 10) });
        }
    }
}