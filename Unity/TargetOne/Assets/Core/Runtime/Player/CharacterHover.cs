using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class CharacterHover : MonoBehaviour
{
    public float TargetHeight; // Initial target height
    public Transform Visual; // Object to fluctuate and hover
    public float FluctuationSpeed = 1f; // Speed of the fluctuation
    public float FluctuationAmplitude = 0.5f; // Amplitude of fluctuation

    private Tween _heightTween; // Tween for base height adjustment
    private Tween _fluctuationTween; // Tween for fluctuation
    private Tween _amplitudeTween; // Tween for amplitude
    
    [ShowNonSerializedField]
    private float _baseHeight; // The current base height

    public readonly (float amp, float height) WakeUpValues = (0.02f, 0.1f); // Dead values
    public (float amp, float height) NormalValues { get; private set; }

    void Start()
    {
        Init();
        StartFluctuation();
    }

    private void Reset()
    {
        FluctuationSpeed = 0.15f;
        FluctuationAmplitude = 0.2f;
    }

    void Init()
    {
        // Initialize the base height
        _baseHeight = TargetHeight;
        Visual.localPosition = new Vector3(0, _baseHeight, 0);
        NormalValues = (FluctuationAmplitude, TargetHeight);
    }

    private void StartFluctuation()
    {
        // Create the fluctuation tween
        _fluctuationTween = DOTween.To(() => 0f, x =>
            {
                // Add the fluctuation effect on top of the base height
                Visual.localPosition = new Vector3(
                    Visual.localPosition.x,
                    _baseHeight +
                    Mathf.Sin(Time.time * Mathf.PI * 2f * FluctuationSpeed * GameSession.Instance.GameSpeed) *
                    FluctuationAmplitude,
                    Visual.localPosition.z
                );
            }, 1f, 1f) // The actual value here (1f) is arbitrary because we handle fluctuation manually.
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental); // Infinite loop for fluctuation
    }
    
    private void SetHeight(float height, float duration)
    {
        TargetHeight = height;
        
        // Stop any ongoing height tween
        _heightTween?.Kill();

        // Start a new tween to change the base height
        _heightTween = DOTween.To(() => _baseHeight, x => _baseHeight = x, height, duration)
            .SetEase(Ease.InOutQuad);
    }

    public void Set(float amp, float height, float duration)
    {
        // Kill the previous amplitude tween if it exists
        _amplitudeTween?.Kill();

        // Animate the fluctuation amplitude smoothly
        _amplitudeTween = DOTween.To(() => FluctuationAmplitude, x => FluctuationAmplitude = x, amp, duration)
            .SetEase(Ease.InOutQuad);  // Use any easing you prefer for the transition
    
        // Update the height as well
        SetHeight(height, duration);
    }
    
    public void SetActive(bool flag)
    {
        // Enable hovering again from zero to normal state
        if (flag)
        {
            Set(NormalValues.amp, NormalValues.height, 2f);
            StartFluctuation();
        }
        // Disable hovering
        else
        {
            // Stop animating
            _heightTween?.Kill();
            _fluctuationTween?.Kill();
            _amplitudeTween?.Kill();
        }
    }

    [Button]
    void DbgSetLow()
    {
        Set(WakeUpValues.amp, WakeUpValues.height, 1f);
    }
    
    [Button]
    void DbgSetHigh()
    {
        Set(NormalValues.amp, NormalValues.height, 1f);
    }
}