using System;
using System.Collections.Generic;
using FxProNS;
using UnityEngine;

// Token: 0x0200003C RID: 60
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/FxPro™")]
public class FxPro : MonoBehaviour
{
	// Token: 0x17000033 RID: 51
	// (get) Token: 0x06000170 RID: 368 RVA: 0x0000F128 File Offset: 0x0000D528
	public static Material Mat
	{
		get
		{
			if (null == FxPro._mat)
			{
				FxPro._mat = new Material(Shader.Find("Hidden/FxPro"))
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}
			return FxPro._mat;
		}
	}

	// Token: 0x17000034 RID: 52
	// (get) Token: 0x06000171 RID: 369 RVA: 0x0000F168 File Offset: 0x0000D568
	private static Material TapMat
	{
		get
		{
			if (null == FxPro._tapMat)
			{
				FxPro._tapMat = new Material(Shader.Find("Hidden/FxProTap"))
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}
			return FxPro._tapMat;
		}
	}

	// Token: 0x17000035 RID: 53
	// (get) Token: 0x06000172 RID: 370 RVA: 0x0000F1A8 File Offset: 0x0000D5A8
	private Camera EffectCamera
	{
		get
		{
			if (null == this._effectCamera)
			{
				this._effectCamera = base.GetComponent<Camera>();
			}
			return this._effectCamera;
		}
	}

	// Token: 0x06000173 RID: 371 RVA: 0x0000F1D0 File Offset: 0x0000D5D0
	public void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			Debug.LogError("Image effects are not supported on this platform.");
			base.enabled = false;
			return;
		}
		this._filmGrainTextures = new List<Texture2D>();
		for (int i = 1; i <= 4; i++)
		{
			string text = "filmgrain_0" + i;
			Texture2D texture2D = Resources.Load(text) as Texture2D;
			if (null == texture2D)
			{
				Debug.LogError("Unable to load grain texture '" + text + "'");
			}
			else
			{
				this._filmGrainTextures.Add(texture2D);
			}
		}
	}

	// Token: 0x06000174 RID: 372 RVA: 0x0000F268 File Offset: 0x0000D668
	public void Init(bool searchForNonDepthmapAlphaObjects = false)
	{
		if (this.HalfResolution)
		{
			Screen.SetResolution(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2, Screen.fullScreen, Screen.currentResolution.refreshRate);
		}
		if ((this.DOFEnabled || this.ColorEffectsEnabled) && this.EffectCamera.depthTextureMode == DepthTextureMode.None)
		{
			this.EffectCamera.depthTextureMode = DepthTextureMode.Depth;
		}
		if (this.DOFEnabled)
		{
			if (null == this.DOFParams.EffectCamera)
			{
				this.DOFParams.EffectCamera = this.EffectCamera;
			}
			this.DOFParams.DepthCompression = Mathf.Clamp(this.DOFParams.DepthCompression, 2f, 8f);
			Singleton<DOFHelper>.Instance.SetParams(this.DOFParams);
			Singleton<DOFHelper>.Instance.Init(searchForNonDepthmapAlphaObjects);
			if (!this.DOFParams.DoubleIntensityBlur)
			{
				Singleton<DOFHelper>.Instance.SetBlurRadius((this.Quality != EffectsQuality.Fastest && this.Quality != EffectsQuality.Fast) ? 5 : 3);
			}
			else
			{
				Singleton<DOFHelper>.Instance.SetBlurRadius((this.Quality != EffectsQuality.Fastest && this.Quality != EffectsQuality.Fast) ? 10 : 5);
			}
		}
		else
		{
			FxPro.Mat.EnableKeyword("DOF_DISABLED");
			FxPro.Mat.DisableKeyword("DOF_ENABLED");
		}
		if (this.FilmGrainIntensity >= 0.001f)
		{
			FxPro.Mat.SetFloat("_FilmGrainIntensity", this.FilmGrainIntensity);
			FxPro.Mat.SetFloat("_FilmGrainTiling", this.FilmGrainTiling);
			FxPro.Mat.EnableKeyword("FILM_GRAIN_ON");
			FxPro.Mat.DisableKeyword("FILM_GRAIN_OFF");
		}
		else
		{
			FxPro.Mat.EnableKeyword("FILM_GRAIN_OFF");
			FxPro.Mat.DisableKeyword("FILM_GRAIN_ON");
		}
		if (this.VignettingIntensity <= 1f)
		{
			FxPro.Mat.SetFloat("_VignettingIntensity", this.VignettingIntensity);
			FxPro.Mat.EnableKeyword("VIGNETTING_ON");
			FxPro.Mat.DisableKeyword("VIGNETTING_OFF");
		}
		else
		{
			FxPro.Mat.EnableKeyword("VIGNETTING_OFF");
			FxPro.Mat.DisableKeyword("VIGNETTING_ON");
		}
	}

	// Token: 0x06000175 RID: 373 RVA: 0x0000F4C5 File Offset: 0x0000D8C5
	public void OnEnable()
	{
		this.Init(true);
	}

	// Token: 0x06000176 RID: 374 RVA: 0x0000F4CE File Offset: 0x0000D8CE
	public void OnDisable()
	{
		if (null != FxPro.Mat)
		{
			global::UnityEngine.Object.DestroyImmediate(FxPro.Mat);
		}
		RenderTextureManager.Instance.Dispose();
		Singleton<DOFHelper>.Instance.Dispose();
	}

	// Token: 0x06000177 RID: 375 RVA: 0x0000F4FE File Offset: 0x0000D8FE
	public void OnValidate()
	{
		this.Init(false);
	}

	// Token: 0x06000178 RID: 376 RVA: 0x0000F508 File Offset: 0x0000D908
	public static RenderTexture DownsampleTex(RenderTexture input, float downsampleBy)
	{
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(Mathf.RoundToInt((float)input.width / downsampleBy), Mathf.RoundToInt((float)input.height / downsampleBy), input.depth, input.format);
		renderTexture.filterMode = FilterMode.Bilinear;
		Graphics.BlitMultiTap(input, renderTexture, FxPro.TapMat, new Vector2[]
		{
			new Vector2(-1f, -1f),
			new Vector2(-1f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, -1f)
		});
		return renderTexture;
	}

	// Token: 0x06000179 RID: 377 RVA: 0x0000F5D0 File Offset: 0x0000D9D0
	private RenderTexture ApplyColorEffects(RenderTexture input)
	{
		if (!this.ColorEffectsEnabled)
		{
			return input;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		Graphics.Blit(input, renderTexture, FxPro.Mat, 5);
		return renderTexture;
	}

	// Token: 0x0600017A RID: 378 RVA: 0x0000F61C File Offset: 0x0000DA1C
	private RenderTexture ApplyLensCurvature(RenderTexture input)
	{
		if (!this.LensCurvatureEnabled)
		{
			return input;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		Graphics.Blit(input, renderTexture, FxPro.Mat, (!this.LensCurvaturePrecise) ? 4 : 3);
		return renderTexture;
	}

	// Token: 0x0600017B RID: 379 RVA: 0x0000F678 File Offset: 0x0000DA78
	private RenderTexture ApplyChromaticAberration(RenderTexture input)
	{
		if (!this.ChromaticAberration)
		{
			return null;
		}
		RenderTexture renderTexture = RenderTextureManager.Instance.RequestRenderTexture(input.width, input.height, input.depth, input.format);
		renderTexture.filterMode = FilterMode.Bilinear;
		Graphics.Blit(input, renderTexture, FxPro.Mat, 2);
		FxPro.Mat.SetTexture("_ChromAberrTex", renderTexture);
		return renderTexture;
	}

	// Token: 0x0600017C RID: 380 RVA: 0x0000F6DC File Offset: 0x0000DADC
	private Vector2 ApplyLensCurvature(Vector2 uv, float barrelPower, bool precise)
	{
		uv = uv * 2f - Vector2.one;
		uv.x *= this.EffectCamera.aspect * 2f;
		float num = Mathf.Atan2(uv.y, uv.x);
		float num2 = uv.magnitude;
		if (precise)
		{
			num2 = Mathf.Pow(num2, barrelPower);
		}
		else
		{
			num2 = Mathf.Lerp(num2, num2 * num2, Mathf.Clamp01(barrelPower - 1f));
		}
		uv.x = num2 * Mathf.Cos(num);
		uv.y = num2 * Mathf.Sin(num);
		uv.x /= this.EffectCamera.aspect * 2f;
		return 0.5f * (uv + Vector2.one);
	}

	// Token: 0x0600017D RID: 381 RVA: 0x0000F7B8 File Offset: 0x0000DBB8
	private void UpdateLensCurvatureZoom()
	{
		float num = 1f / this.ApplyLensCurvature(new Vector2(1f, 1f), this.LensCurvaturePower, this.LensCurvaturePrecise).x;
		FxPro.Mat.SetFloat("_LensCurvatureZoom", num);
	}

	// Token: 0x0600017E RID: 382 RVA: 0x0000F808 File Offset: 0x0000DC08
	private void UpdateFilmGrain()
	{
		if (this.FilmGrainIntensity >= 0.001f)
		{
			int num = global::UnityEngine.Random.Range(0, 3);
			FxPro.Mat.SetTexture("_FilmGrainTex", this._filmGrainTextures[num]);
			switch (global::UnityEngine.Random.Range(0, 3))
			{
			case 0:
				FxPro.Mat.SetVector("_FilmGrainChannel", new Vector4(1f, 0f, 0f, 0f));
				break;
			case 1:
				FxPro.Mat.SetVector("_FilmGrainChannel", new Vector4(0f, 1f, 0f, 0f));
				break;
			case 2:
				FxPro.Mat.SetVector("_FilmGrainChannel", new Vector4(0f, 0f, 1f, 0f));
				break;
			case 3:
				FxPro.Mat.SetVector("_FilmGrainChannel", new Vector4(0f, 0f, 0f, 1f));
				break;
			}
		}
	}

	// Token: 0x0600017F RID: 383 RVA: 0x0000F920 File Offset: 0x0000DD20
	private void RenderEffects(RenderTexture source, RenderTexture destination)
	{
		source.filterMode = FilterMode.Bilinear;
		this.UpdateFilmGrain();
		this.curRenderTex = source;
		this.srcProcessed = source;
		RenderTextureManager.Instance.SafeAssign(ref this.curRenderTex, FxPro.DownsampleTex(this.srcProcessed, 2f));
		if (this.Quality == EffectsQuality.Fastest)
		{
			RenderTextureManager.Instance.SafeAssign(ref this.curRenderTex, FxPro.DownsampleTex(this.curRenderTex, 2f));
		}
		if (this.DOFEnabled)
		{
			if (null == this.DOFParams.EffectCamera)
			{
				Debug.LogError("null == DOFParams.camera");
				return;
			}
			this.cocRenderTex = RenderTextureManager.Instance.RequestRenderTexture(this.curRenderTex.width, this.curRenderTex.height, this.curRenderTex.depth, this.curRenderTex.format);
			Singleton<DOFHelper>.Instance.RenderCOCTexture(this.curRenderTex, this.cocRenderTex, (!this.BlurCOCTexture) ? 0f : 1.5f);
			this.dofRenderTex = RenderTextureManager.Instance.RequestRenderTexture(this.curRenderTex.width, this.curRenderTex.height, this.curRenderTex.depth, this.curRenderTex.format);
			Singleton<DOFHelper>.Instance.RenderDOFBlur(this.curRenderTex, this.dofRenderTex, this.cocRenderTex);
			FxPro.Mat.SetTexture("_DOFTex", this.dofRenderTex);
			FxPro.Mat.SetTexture("_COCTex", this.cocRenderTex);
		}
		Graphics.Blit(this.srcProcessed, destination, FxPro.Mat, 0);
		RenderTextureManager.Instance.ReleaseRenderTexture(this.srcProcessed);
		RenderTextureManager.Instance.ReleaseRenderTexture(this.cocRenderTex);
		RenderTextureManager.Instance.ReleaseRenderTexture(this.dofRenderTex);
		RenderTextureManager.Instance.ReleaseRenderTexture(this.curRenderTex);
	}

	// Token: 0x06000180 RID: 384 RVA: 0x0000FB04 File Offset: 0x0000DF04
	[ImageEffectTransformsToLDR]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		this.RenderEffects(source, destination);
		RenderTextureManager.Instance.ReleaseAllRenderTextures();
	}

	// Token: 0x0400026F RID: 623
	public EffectsQuality Quality = EffectsQuality.Normal;

	// Token: 0x04000270 RID: 624
	private static Material _mat;

	// Token: 0x04000271 RID: 625
	private static Material _tapMat;

	// Token: 0x04000272 RID: 626
	private Camera _effectCamera;

	// Token: 0x04000273 RID: 627
	public Texture2D LensDirtTexture;

	// Token: 0x04000274 RID: 628
	[Range(0f, 2f)]
	public float LensDirtIntensity = 1f;

	// Token: 0x04000275 RID: 629
	public bool ChromaticAberration = true;

	// Token: 0x04000276 RID: 630
	public bool ChromaticAberrationPrecise;

	// Token: 0x04000277 RID: 631
	[Range(1f, 2.5f)]
	public float ChromaticAberrationOffset = 1f;

	// Token: 0x04000278 RID: 632
	[Range(0f, 1f)]
	public float SCurveIntensity = 0.5f;

	// Token: 0x04000279 RID: 633
	public bool LensCurvatureEnabled = true;

	// Token: 0x0400027A RID: 634
	[Range(1f, 2f)]
	public float LensCurvaturePower = 1.1f;

	// Token: 0x0400027B RID: 635
	public bool LensCurvaturePrecise;

	// Token: 0x0400027C RID: 636
	[Range(0f, 1f)]
	public float FilmGrainIntensity = 0.5f;

	// Token: 0x0400027D RID: 637
	[Range(1f, 10f)]
	public float FilmGrainTiling = 4f;

	// Token: 0x0400027E RID: 638
	[Range(0f, 1f)]
	public float VignettingIntensity = 0.5f;

	// Token: 0x0400027F RID: 639
	public bool DOFEnabled = true;

	// Token: 0x04000280 RID: 640
	public bool BlurCOCTexture = true;

	// Token: 0x04000281 RID: 641
	public DOFHelperParams DOFParams = new DOFHelperParams();

	// Token: 0x04000282 RID: 642
	public bool VisualizeCOC;

	// Token: 0x04000283 RID: 643
	private List<Texture2D> _filmGrainTextures;

	// Token: 0x04000284 RID: 644
	public bool ColorEffectsEnabled = true;

	// Token: 0x04000285 RID: 645
	public Color CloseTint = new Color(1f, 0.5f, 0f, 1f);

	// Token: 0x04000286 RID: 646
	public Color FarTint = new Color(0f, 0f, 1f, 1f);

	// Token: 0x04000287 RID: 647
	[Range(0f, 1f)]
	public float CloseTintStrength = 0.5f;

	// Token: 0x04000288 RID: 648
	[Range(0f, 1f)]
	public float FarTintStrength = 0.5f;

	// Token: 0x04000289 RID: 649
	[Range(0f, 2f)]
	public float DesaturateDarksStrength = 0.5f;

	// Token: 0x0400028A RID: 650
	[Range(0f, 1f)]
	public float DesaturateFarObjsStrength = 0.5f;

	// Token: 0x0400028B RID: 651
	public Color FogTint = Color.white;

	// Token: 0x0400028C RID: 652
	[Range(0f, 1f)]
	public float FogStrength = 0.5f;

	// Token: 0x0400028D RID: 653
	public bool HalfResolution;

	// Token: 0x0400028E RID: 654
	private RenderTexture curRenderTex;

	// Token: 0x0400028F RID: 655
	private RenderTexture srcProcessed;

	// Token: 0x04000290 RID: 656
	private RenderTexture cocRenderTex;

	// Token: 0x04000291 RID: 657
	private RenderTexture dofRenderTex;
}
