using TowerGenerator.ChunkImporter;
using UnityEngine;
using UnityEngine.Assertions;

namespace TowerGenerator.FbxCommands
{
    // Some objects don't change their state by groups. They could change the state by 'induction' or 'suppress'
    // To hide some objects initially you could use 'Hidden'.
    public class FbxCommandNonReplaceable : FbxCommandBase
    {
        public FbxCommandNonReplaceable(string fbxCommandName, int executionPriority) : base(fbxCommandName, executionPriority)
        {
        }

        public override void ParseParameters(string parameters, GameObject gameObject)
        {
            Assert.IsNotNull(gameObject, $"There must be an object for the command '{GetFbxCommandName()}'");
        }

        public override void Execute(GameObject gameObject, ChunkImportState importState)
        {
            Assert.IsNotNull(gameObject, $"There must be an object for the command '{GetFbxCommandName()}'");
            Assert.IsNotNull(importState);
            Assert.IsNull(gameObject.GetComponent<NonReplaceable>());

            gameObject.AddComponent<NonReplaceable>();
            importState.Inc("NonReplaceableAmount");
        }
    }
}
