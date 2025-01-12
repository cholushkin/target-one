using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventCallback : MonoBehaviour
{
    public ScreenTransitionEffects ScreenTransitionEffect;

    public void OnAnimationFinished()
    {
        ScreenTransitionEffect.OnAnimationFinishCallBack(gameObject.name);
    }
}
