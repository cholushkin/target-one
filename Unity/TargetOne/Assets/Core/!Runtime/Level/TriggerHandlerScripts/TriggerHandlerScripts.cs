using uconsole;
using UnityEngine;
using UnityEngine.VFX;

public static class TriggerHandlerScripts
{
    public static void PrintMessage(string message)
    {
        Debug.Log(message);
    }

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
    
    
}