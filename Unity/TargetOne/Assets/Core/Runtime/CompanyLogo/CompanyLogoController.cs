using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gamelib;
using GameLib.ColorScheme;
using GameLib.Random;
using NaughtyAttributes;
using TowerGenerator;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class CompanyLogoController : MonoBehaviour
{
    [Serializable]
    public class LogoRenderState
    {
        public string SHA256;
        public Sprite[] Spr; // for each color index
    }

    public Transform Piece1;
    public Transform Piece2;
    public Transform Piece3;
        

    public bool LoadNextScene;
    public bool ShowSDRender;
    public string NextSceneName;
    public float DelayToTheNextScene;
    [Required] public TextQuantumEffect TextQuantumEffect;
    [Tooltip("This effect actually distract player from animation, so let's consider it as an easter egg")]
    [Range(0, 1)] public float PlayTextQuantumEffectChance;
    [Required] public Image PictureFrame;
    [Required] public RevealImage RevealSDRender;
    [Required] public RevealImage RevealBG;
    [Required] public GameObject Chunk;

    [Header("Visual configuration")]
    // ---------------------------------
    [Tooltip("Could be a big amount of materials because it's just a texture for one of the state of the chunk")]
    public ColorScheme[] RandomizationMaterials;

    public LogoRenderState[] RenderStates;

    public float[] frameDelay;
    public Material LogoAtlasMaterial;

    [Header("Audio configuration")]
    // ---------------------------------
    [ShowAsRange]
    public float2 PitchRandomizationRange;

    public AudioClip[] AudioClips;

    [Required] public AudioSource AudioSource;


    private ChunkControllerBase _chunkControllerBase;

     void Awake()
    {
        _chunkControllerBase = Chunk.GetComponent<ChunkControllerBase>();
        Assert.IsNotNull(_chunkControllerBase);
        _chunkControllerBase.Init();
        _chunkControllerBase.SetConfiguration();
        
        ScreenTransitionEffects.Instance.PlayEffect("ColorFadeReveal", StartAnimation);
    }

    private async void StartAnimation()
    {
        // Wait for the first frame to be fully rendered
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

        await RandomizeSoundAsync();
        await RandomizeAsync();
        if (LoadNextScene)
            Invoke("LoadScene", DelayToTheNextScene);
    }

    private async UniTask RandomizeSoundAsync()
    {
        AudioSource.clip = RandomHelper.Rnd.FromArray(AudioClips);
        AudioSource.pitch = RandomHelper.Rnd.Range(PitchRandomizationRange);

        if (AudioSource.clip != null)
        {
            await UniTask.WaitUntil(() => AudioSource.clip.loadState == AudioDataLoadState.Loaded);
            await UniTask.DelayFrame(1); // Ensure the clip is loaded
        }
        else
        {
            Debug.LogWarning("Audio clip is null. Make sure AudioClips array is populated.");
        }
    }


    private async UniTask RandomizeAsync()
    {
        AudioSource.Play();
        while (!AudioSource.isPlaying)
        {
            await UniTask.Yield(); // Wait until the audio starts playing
        }
        
        if(RandomHelper.Rnd.ValueFloat() < PlayTextQuantumEffectChance)
            TextQuantumEffect.PlayEffect();

        StartRotatingAndScalingAnimation();

        for (int i = 0; i < frameDelay.Length; i++)
        {
            if (i != frameDelay.Length - 1)
            {
                await UniTask.Delay((int)(frameDelay[i] * 1000));
                ChangeColorPalette();
            }
            else
            {
                // Call RevealRender for the last iteration
                if(ShowSDRender)
                    RevealRender();
            }

            // Update chunk configuration
            _chunkControllerBase.SetConfiguration();
        }
    }

    private void StartRotatingAndScalingAnimation()
    {
        Chunk.transform.DOScale(Vector3.one * 1.1f, 7f).SetEase(Ease.OutCubic);
        
        Piece1.DOScale(Vector3.one * 1.5f, 2f)
            .From().SetEase(Ease.OutBack);
        
        Piece2.DOLocalRotate(transform.localRotation.eulerAngles + Vector3.forward * 25, 2f, RotateMode.FastBeyond360)
            .From().SetEase(Ease.InOutQuint).SetLoops(1);
        
        Piece3.DOLocalRotate(transform.localRotation.eulerAngles + Vector3.forward * -5, 1.125f, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuint).SetLoops(2, LoopType.Yoyo);
    }

    private void RevealRender()
    {
        PictureFrame.sprite = RenderStates[0].Spr[0];
        RevealSDRender.Reveal();
        RevealBG.Reveal();
    }

    private void ChangeColorPalette()
    {
        LogoAtlasMaterial.mainTexture = RandomHelper.Rnd.FromArray(RandomizationMaterials).Atlas;
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
                SceneLoader.Instance.Replace(NextSceneName, "CompanyLogo", true);
            }
        );
    }
}