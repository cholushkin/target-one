using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Required]
    public Transform Visual;
    
    public async UniTask PlayDisappearInTube(Tile tile)
    {
        const float scaleDownDuration = 0.6f;
        const float localMoveDuration = 0.5f;
        
        await DOTween.Sequence()
            .Join(Visual.DOScale(Vector3.one * 0.3f, scaleDownDuration).SetEase(Ease.InOutCirc)) // Scale down
            .Join(Visual.DOLocalMove( Visual.transform.InverseTransformPoint(tile.transform.position), localMoveDuration).SetEase(Ease.InCubic)) // Move to zero
            .ToUniTask();
    }

    public async Task PlayAppearFromTube(Tile tile, float targetHeight)
    {
        await DOTween.Sequence()
            .Join(Visual.DOScale(Vector3.one, 1f).SetEase(Ease.InOutCirc))
            .Join(Visual.DOLocalMove( Visual.transform.InverseTransformDirection(tile.Normal) * targetHeight, 1f).SetEase(Ease.InCubic))
            .ToUniTask();
    }
}