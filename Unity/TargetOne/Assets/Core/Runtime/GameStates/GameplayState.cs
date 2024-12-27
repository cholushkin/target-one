using GameLib;
using GameLib.Log;

namespace Core
{
    public class GameplayState : AppStateManager.AppState<GameplayState>
    {
        public override void AppStateEnter()
        {
            LogChecker.Print(LogChecker.Level.Verbose, "> GameplayState.AppStateEnter");
        }

        public override void AppStateLeave()
        {
        }
    }
}