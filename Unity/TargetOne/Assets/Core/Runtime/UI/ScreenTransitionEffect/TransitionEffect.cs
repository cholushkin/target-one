using UnityEngine;
using UnityEngine.Assertions;

public class TransitionEffect : MonoBehaviour
{
    public bool DisableOnFinish;
    public float Duration;

    public virtual void OnEnable()
    {
        Assert.IsTrue(Duration != 0f);
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = Duration;
        }
    }
}