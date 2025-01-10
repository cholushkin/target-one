using System;
using Cysharp.Threading.Tasks;
using Gamelib;
using GameLib.ColorScheme;
using GameLib.Random;
using NaughtyAttributes;
using TowerGenerator;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Range = GameLib.Random.Range;

public class CompanyLogoController : MonoBehaviour
{
	[Serializable]
	public class LogoRenderState
	{
		public string SHA256;
		public Sprite[] Spr; // for each color index
	}
	public string NextSceneName;
	public float DelayToTheNextScene;
	[Required]
	public Image PictureFrame;
	[Required]
	public RevealImage RevealSDRender;
	[Required]
	public RevealImage RevealBG;
	[Required]
	public GameObject Chunk;

	[Required] public Rotating Rotating;
	[Required] public Scaling Scaling;

	[Header("Visual configuration")]
	// ---------------------------------
	[Tooltip("Could be a big amount of materials because it's just a texture for one of the state of the chunk")]
	public ColorScheme[] RandomizationMaterials;

	[Tooltip("Should be a small amount of materials because limited amount of renders depends on them. Shows this material only on the last frame, before transition to a render")]
	public ColorScheme[] RandomizationMaterialsForRendering;

	public LogoRenderState[] RenderStates;

	public float[] frameDelay;
	public Material LogoAtlasMaterial;

	[Header("Audio configuration")]
	// ---------------------------------
	public Range PitchRandomizationRange;

	public AudioClip[] AudioClips;

	[Required]
	public AudioSource AudioSource;


	private ChunkControllerBase _chunkControllerBase;

	async void Awake()
	{
		_chunkControllerBase = Chunk.GetComponent<ChunkControllerBase>();
		Assert.IsNotNull(_chunkControllerBase);
		_chunkControllerBase.Init();
		_chunkControllerBase.SetConfiguration();
		
		// Wait for the first frame to be fully rendered
		await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

		await RandomizeSoundAsync();
		await RandomizeAsync();
		Invoke("LoadScene", DelayToTheNextScene);
	}

	private async UniTask RandomizeSoundAsync()
	{
		AudioSource.clip = RandomHelper.Rnd.FromArray(AudioClips);
		AudioSource.pitch = RandomHelper.Rnd.FromRange(PitchRandomizationRange);

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

		Rotating.StartRotating();
		Scaling.StartScaling();
		
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
				RevealRender();
			}

			// Update chunk configuration
			_chunkControllerBase.SetConfiguration();
		}
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
		if (SceneLoader.Instance == null)
		{
			Debug.LogWarning("No SceneLoader.Instance. You need to run current scene with dependencies");
			return;
		}

		SceneLoader.Instance.Replace("Gameplay", "CompanyLogo", true);
	}
}