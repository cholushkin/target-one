using UnityEngine;

public class TransitionEffect : MonoBehaviour
{
    public bool DisableOnFinish;
    public float Duration;

    public virtual void OnEnable()
    {
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = Duration;
        }
    }
}
