using GameLib.Alg;

public class GameAnalytics : Singleton<GameAnalytics>
{
    public class AnalyticsEventBase
    {
        public string EventType;
    }

    public class AnalyticsEventDie : AnalyticsEventBase
    {
        
    }

    private AnalyticsEventBase[] _analyticsEvents;
    
    protected override void Awake()
    {
        base.Awake();
    }

    public void DumpToFile()
    {
    }
    
    // todo: Register custom handler for third-party analytics


    #region Events

    public static void PostEventDie()
    {
    }
    
    #endregion
}
