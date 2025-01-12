using System;
using System.Collections.Generic;
using System.Linq;
using GameLib.Alg;
using UnityEngine;
using UnityEngine.Assertions;

// slightly inspired by : https://youtu.be/CE9VOZivb3I

public class ScreenTransitionEffects : Singleton<ScreenTransitionEffects>
{
    public string LastEffectPlayed { get; private set; }
    private List<Transform> _effects;
    private GameObject _currentEffect;
    private Action _currentCallback;

    protected override void Awake()
    {
        base.Awake();
        _effects = transform.Children().ToList();

        // hide all if user forgot to hide it
        foreach (Transform effect in transform)
            Assert.IsFalse(effect.gameObject.activeSelf);
    }

    public void PlayEffect(string effectName, Action endCallback)
    {
        if (_currentEffect != null)
        {
            _currentEffect.SetActive(false);
            _currentEffect = null;
        }

        Assert.IsNull(_currentEffect);
        var effect = FindEffect(effectName);
        LastEffectPlayed = effectName;
        effect.SetActive(true);
        _currentEffect = effect;
        _currentCallback = endCallback;
    }

    public void OnAnimationFinishCallBack(string effectName)
    {
        Debug.Log($"{effectName} finished");
        Assert.IsNotNull(_currentEffect);
        if (_currentEffect.GetComponent<TransitionEffect>().DisableOnFinish)
        {
            _currentEffect.SetActive(false);
            _currentEffect = null;
        }

        _currentCallback?.Invoke();
        _currentCallback = null;
    }

    private GameObject FindEffect(string effectName)
    {
        var effect = _effects.FirstOrDefault(e => e.name == effectName);
        return effect.gameObject;
    }
}