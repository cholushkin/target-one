using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RevealImage : MonoBehaviour
{
    public float Duration;
    public Image Image;
    public Ease Ease;

    public void Reveal()
    {
        Image.DOFade(1f, Duration)
            .SetEase(Ease);
    }
}
