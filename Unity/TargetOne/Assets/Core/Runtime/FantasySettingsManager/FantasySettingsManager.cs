using System;
using GameLib.Alg;
using GameLib.ColorScheme;
using UnityEngine;

public class FantasySettingsManager : Singleton<FantasySettingsManager>
{
    [Serializable]
    public class Settings
    {
        public string Name;
        public GameObject[] TileVisualVariations;

        public ColorScheme[] ChunkColorSchemeVariations;
        public ColorScheme[] DecorationsColorSchemeVariations;

        // todo: music tracks
        // todo: sun
        // todo: fog
        // todo: lights
    }

    public Settings[] FanatasySettings;

}
