using System;
using System.Linq;
using Core;
using DG.Tweening;
using GameLib.Log;
using NaughtyAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var wheelRotation = GetComponent<TileWheelRotation>();
        if (wheelRotation)
        {
            Gizmos.color = Color.white;
            Vector3 position = transform.position;
            if (wheelRotation.Rotations == null)
                return;
            
            var tile = GetComponent<Tile>();
            if(!tile)
                return;
            
            foreach (var rotation in wheelRotation.Rotations)
            {
                Gizmos.color = Color.white;
                if (rotation.AddQuaternion == Quaternion.identity)
                    Gizmos.color = Color.grey;

                if (rotation.EnterTile)
                {
                    Gizmos.DrawLine(transform.position + tile.Up * 0.5f, rotation.EnterTile.transform.position);
                    Handles.Label(rotation.EnterTile.transform.position, rotation.EnterTile.gameObject.name);                        
                }
            }
            Gizmos.DrawLine(transform.position, transform.position + tile.Up * 0.5f);
        }
    }
#endif

    [Button]
    public void Rotate()
    {
        if (!_lastFromTile)
        {
            LogChecker.PrintWarning(LogChecker.Level.Important, $"There is no registered Walker entered tile {name}");
            return;
        }
        
        // Get walker
        var walker = GetComponentInChildren<TileWalker>();
        if (!walker)
        {
            LogChecker.PrintWarning(LogChecker.Level.Important, $"can't find Walker. Tile: {name}");
            return;
        }

        var item = Rotations.FirstOrDefault(i => i.EnterTile == _lastFromTile);
        if (item == null)
        {
            LogChecker.PrintWarning(LogChecker.Level.Important,
                $"can't find {_lastFromTile.name} in defined rotations");
            return;
        }

        var finalTargetRotation = transform.localRotation * item.AddQuaternion;
        var duration = walker.GetTimeLeftToQuitCurrentTile() * 0.9f;
        walker.StickToTile = true;

        transform.DOLocalRotateQuaternion(finalTargetRotation, duration)
            .SetEase(Ease)
            .OnComplete(() => OnCompleteRotation(walker));
    }

    public void RegisterEntering(Tile fromTile)
    {
        LogChecker.Print(LogChecker.Level.Normal, $"Entering from: {fromTile.name}");
        _lastFromTile = fromTile;
    }

    private void OnCompleteRotation(TileWalker walker)
    {
        walker.StickToTile = false;
        walker.RecalculateSmoothRotationTarget();
    }
}