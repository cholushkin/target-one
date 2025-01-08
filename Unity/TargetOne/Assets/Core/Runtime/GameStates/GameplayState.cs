using GameLib;
using GameLib.Log;
using UnityEngine;

namespace Core
{
    public class GameplayState : AppStateManager.AppState<GameplayState>
    {
        public LevelGenerator LevelGenerator;
        
        public override void AppStateEnter()
        {
            LogChecker.Print(LogChecker.Level.Verbose, "> GameplayState.AppStateEnter");
            Application.targetFrameRate = 60;
            LevelGenerator.StartGenerate();
        }

        public override void AppStateLeave()
        {
        }
    }
}