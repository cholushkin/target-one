using System;
using System.Linq;
using DG.Tweening;
using GameLib.Log;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

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
    public float RotationSpeed;
    public Ease Ease;
    private Tile _lastFromTile;


    public void Reset()
    {
    }

    [Button]
    public void Rotate()
    {
        var item = Rotations.FirstOrDefault(i => i.EnterTile == _lastFromTile);
        if (item == null)
        {
            LogChecker.Print(LogChecker.Level.Important, $"can't find {_lastFromTile.name} in defined rotations");
            return;
        }

        var finalTargetRotation = transform.localRotation * item.AddQuaternion;
        var speed = RotationSpeed * GameSession.Instance.GameSpeed;
        var distance = Quaternion.Angle(transform.localRotation, finalTargetRotation);
        float duration = distance / RotationSpeed;

        transform.DOLocalRotateQuaternion(finalTargetRotation, duration)
            .SetEase(Ease);
    }

    public void RegisterEntering(Tile fromTile)
    {
        LogChecker.Print(LogChecker.Level.Normal, $"Entering from: {fromTile.name}");
        _lastFromTile = fromTile;
    }
}