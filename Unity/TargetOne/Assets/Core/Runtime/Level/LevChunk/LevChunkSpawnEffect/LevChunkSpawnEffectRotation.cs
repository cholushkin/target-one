using DG.Tweening;
using GameLib.Random;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class TileSpawnEffectRotation : LevChunkSpawnEffectBase
{
    [ShowAsRange] public float2 Duration;

    public override void PlayEffect()
    {
        foreach (var tile in _tiles)
        {
            var q = GetRandomQuaternion();
            tile.Visual.transform.DORotateQuaternion(q, 
                RandomHelper.Rnd.Range(Duration.From(), Duration.To()))
                .From()
                .SetEase(Ease.OutQuart);
        }
    }

    public static Quaternion GetRandomQuaternion()
    {
        // Generate three random numbers
        float u1 = RandomHelper.Rnd.ValueFloat();
        float u2 = RandomHelper.Rnd.ValueFloat();
        float u3 = RandomHelper.Rnd.ValueFloat();

        // Convert the random numbers into quaternion components
        float w = Mathf.Sqrt(1 - u1) * Mathf.Sin(2 * Mathf.PI * u2);
        float x = Mathf.Sqrt(1 - u1) * Mathf.Cos(2 * Mathf.PI * u2);
        float y = Mathf.Sqrt(u1) * Mathf.Sin(2 * Mathf.PI * u3);
        float z = Mathf.Sqrt(u1) * Mathf.Cos(2 * Mathf.PI * u3);

        // Return the random quaternion
        return new Quaternion(x, y, z, w);
    }
}