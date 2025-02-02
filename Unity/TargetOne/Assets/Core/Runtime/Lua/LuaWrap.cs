using uconsole;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LuaWrap
{
    #region Tiles 
    [ConsoleMethod("core.destroytile", "tiledestroy", "Destroy tile")]
    public static void DestroyTile(string tileName)
    {
        Debug.Log("dstr");
    }
    
    [ConsoleMethod("core.istileexist", "tileexist", "Return true if tile exists")]
    public static bool IsTileExist(string tileName, bool print = false)
    {
        var obj = GameObject.Find(tileName);
        if (print)
        {
            Debug.Log($"Tile '{tileName}' exists: { obj != null}");
        }
        return obj != null;
    }

    [ConsoleMethod("core.overridelevchunksequence", "levchunkseq", "Overrides lev chunk sequence on generator segment")]
    public static void OverrideLevChunkSequence(string[] sequence)
    {
        LevelGenerator.Instance.OverrideLevChunkSequenceForCurrentSegment(sequence);
    }
    
    #endregion
    
    #region Debug
    
    [ConsoleMethod("scene.reload", "scenereload", "Reload current active scene")]
    public static void ReloadCurrentScene(string tileName)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ConsoleMethod("level.setsegmentconfig", "setsegcfg", "Set configuration for level generator")]
    public static void SetLevelGeneratorSegmentConfig(int segmentIndex)
    {
        LevelGenerator.Instance.SetCurrentSegment(segmentIndex);
    }

    #endregion

    [ConsoleVariable("gamespeed", "speed", "Game speed")]
    public static float GameSpeed
    {
        get => GameSessionController.Instance == null ? 0f : GameSessionController.Instance.GameSpeed;
        set => GameSessionController.Instance.GameSpeed = value;
    }
    
    
}