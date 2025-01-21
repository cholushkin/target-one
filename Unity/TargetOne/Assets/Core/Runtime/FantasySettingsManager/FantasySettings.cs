using System;
using GameLib.ColorScheme;
using UnityEngine;


[CreateAssetMenu(fileName = "DefaultFantasySettings", menuName = "Game/FantasySettings", order = 1)]
public class FantasySettings : ScriptableObject
{
    [Flags]
    public enum FantasySettingsType
    {
        Castle,
        Chess,
        Maya, // jungle, aztec, snakes
    }

    public FantasySettingsType Type;
    public GameObject[] TileVisualVariations;
    public ColorScheme[] ChunkColorSchemeVariations;
    public ColorScheme[] DecorationsColorSchemeVariations;

    // todo: music tracks
    // todo: sun
    // todo: fog
    // todo: lights
}