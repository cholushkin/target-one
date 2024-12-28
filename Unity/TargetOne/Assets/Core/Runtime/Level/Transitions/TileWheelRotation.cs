using System;
using System.Linq;
using Core;
using DG.Tweening;
using GameLib.Log;
using NaughtyAttributes;
using UnityEngine;

public class TileWheelRotation : MonoBehaviour
{
    [Serializable]
    public class DirectionItem
    {
        public Tile EnterTile;
        public Quaternion AddQuaternion;
    }

    public LogChecker LogChecker;
    public DirectionItem[] Rotations;
    public Ease Ease;
    private Tile _lastFromTile;


    public void Reset()
    {
        Ease = Ease.InOutQuart;
    }

    [Button]
    public void Rotate()
    {
        if (!_lastFromTile)
        {
            LogChecker.PrintWarning(LogChecker.Level.Important, $"There is no registered Walker entered tile {name}");
            return;
        }

        var item = Rotations.FirstOrDefault(i => i.EnterTile == _lastFromTile);
        if (item == null)
        {
            LogChecker.PrintWarning(LogChecker.Level.Important, $"can't find {_lastFromTile.name} in defined rotations");
            return;
        }

        var finalTargetRotation = transform.localRotation * item.AddQuaternion;
        
        // Get walker
        var walker = GetComponentInChildren<TileWalker>();
        if (!walker)
        {
            LogChecker.PrintWarning(LogChecker.Level.Important, $"can't find Walker. Tile: {name}");
            return;
        }

        // Get walker duration to walk through the tile
        var duration = walker.GetTimeLeftToQuitCurrentTile();
        Debug.Log(duration);


        transform.DOLocalRotateQuaternion(finalTargetRotation, duration)
            .SetEase(Ease);
    }

    public void RegisterEntering(Tile fromTile)
    {
        LogChecker.Print(LogChecker.Level.Normal, $"Entering from: {fromTile.name}");
        _lastFromTile = fromTile;
    }
}