using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;
using Range = GameLib.Random.Range;

public class TileSpawnEffectRotation : LevChunkSpawnEffectBase
{
    public Range Duration;
    public override void PlayEffect()
    {
        foreach (var tile in _tiles)
        {
            var q = GetRandomQuaternion();
            tile.Visual.transform.DORotateQuaternion(q, Random.Range(Duration.From, Duration.To )).From().SetEase(Ease.OutQuart);
        }
    }
    
    public static Quaternion GetRandomQuaternion()
    {
        // Generate three random numbers
        float u1 = Random.value;
        float u2 = Random.value;
        float u3 = Random.value;

        // Convert the random numbers into quaternion components
        float w = Mathf.Sqrt(1 - u1) * Mathf.Sin(2 * Mathf.PI * u2);
        float x = Mathf.Sqrt(1 - u1) * Mathf.Cos(2 * Mathf.PI * u2);
        float y = Mathf.Sqrt(u1) * Mathf.Sin(2 * Mathf.PI * u3);
        float z = Mathf.Sqrt(u1) * Mathf.Cos(2 * Mathf.PI * u3);

        // Return the random quaternion
        return new Quaternion(x, y, z, w);
    }

}
