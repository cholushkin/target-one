using System.Linq;
using GameLib.Alg;

public class FantasySettingsManager : Singleton<FantasySettingsManager>
{
    public FantasySettings[] FantasySettings;
    public const FantasySettings.FantasySettingsType DefaultFantasySettings = global::FantasySettings.FantasySettingsType.Castle;

    public FantasySettings GetDefaultSettings()
    {
        return FantasySettings.FirstOrDefault(t => t.Type == DefaultFantasySettings);
    }
}
