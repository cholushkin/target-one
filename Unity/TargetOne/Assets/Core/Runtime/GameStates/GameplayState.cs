using Events;
using GameLib;
using GameLib.Log;
using UnityEngine;

namespace Core
{
    public class GameplayState : AppStateManager.AppState<GameplayState>
        , IHandle<LevelGenerator.EventLevelLoaded>
    {
        public LevelGenerator LevelGenerator;
        public AccountManager.GameState CurrentGameState;

        public override void AppStateInitialization()
        {
            GlobalEventAggregator.EventAggregator.Subscribe(this);
        }
        
        public override void AppStateEnter()
        {
            LogChecker.Print(LogChecker.Level.Verbose, "> GameplayState.AppStateEnter");
            Application.targetFrameRate = 60;

            var activeAccountIndex = AccountManager.Instance.ActiveAccounts.ActiveAccountIndex;
            CurrentGameState = AccountManager.Instance.GetAccountData(activeAccountIndex);
            if (!AccountManager.Instance.IsAccountInitialized(activeAccountIndex))
            {
                CurrentGameState.Checkpoint = 0;
                CurrentGameState.Coins = 0;
            }
            
            LevelGenerator.SetCurrentSegment(CurrentGameState.Checkpoint);
            LevelGenerator.StartGenerate();
        }

        public override void AppStateLeave()
        {
            LogChecker.Print(LogChecker.Level.Verbose, "< GameplayState.AppStateLeave");
        }

        public void Handle(LevelGenerator.EventLevelLoaded message)
        {
            ScreenTransitionEffects.Instance.PlayEffect("ColorFadeReveal", null);
        }
    }
}