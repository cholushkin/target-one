using Gamelib;
using UnityEngine;

public class GameLogoController : MonoBehaviour
{
    public float ShowDuration;
    public string NextSceneName;
    public Transform GameTitle;
    void Awake()
    {
        ScreenTransitionEffects.Instance.PlayEffect("ColorFadeReveal", null);
        //GameTitle.SetParent(null);
        Invoke(nameof(LoadScene), ShowDuration);
    }
    
    private void LoadScene()
    {
        ScreenTransitionEffects.Instance.PlayEffect("ColorFadeHide",
            () =>
            {
                if (SceneLoader.Instance == null)
                {
                    Debug.LogWarning("No SceneLoader.Instance. You need to run current scene with dependencies");
                    return;
                }
                SceneLoader.Instance.Replace(NextSceneName, "GameLogo", true);
            }
        );
    }
}
