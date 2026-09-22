using System;
using FogVolumeUtilities;
using UnityEngine;

// Token: 0x02000017 RID: 23
[ExecuteInEditMode]
public class FogVolumeScreen : MonoBehaviour
{
	// Token: 0x17000020 RID: 32
	// (get) Token: 0x060000E9 RID: 233 RVA: 0x0000AC76 File Offset: 0x00008E76
	public int screenX
	{
		get
		{
			return this.SceneCamera.pixelWidth;
		}
	}

	// Token: 0x17000021 RID: 33
	// (get) Token: 0x060000EA RID: 234 RVA: 0x0000AC83 File Offset: 0x00008E83
	public int screenY
	{
		get
		{
			return this.SceneCamera.pixelHeight;
		}
	}

	// Token: 0x17000022 RID: 34
	// (get) Token: 0x060000EB RID: 235 RVA: 0x0000AC90 File Offset: 0x00008E90
	// (set) Token: 0x060000EC RID: 236 RVA: 0x0000AC98 File Offset: 0x00008E98
	public string FogVolumeLayerName
	{
		get
		{
			return this._FogVolumeLayerName;
		}
		set
		{
			if (this._FogVolumeLayerName != value)
			{
				this.SetFogVolumeLayer(value);
			}
		}
	}

	// Token: 0x060000ED RID: 237 RVA: 0x0000ACAF File Offset: 0x00008EAF
	private void SetFogVolumeLayer(string NewFogVolumeLayerName)
	{
		this._FogVolumeLayerName = NewFogVolumeLayerName;
		this.FogVolumeLayer = LayerMask.NameToLayer(this._FogVolumeLayerName);
	}

	// Token: 0x060000EE RID: 238 RVA: 0x0000ACC9 File Offset: 0x00008EC9
	private void OnValidate()
	{
		this.SetFogVolumeLayer(this._FogVolumeLayerName);
	}

	// Token: 0x17000023 RID: 35
	// (get) Token: 0x060000EF RID: 239 RVA: 0x0000ACD7 File Offset: 0x00008ED7
	private Material BlurMaterial
	{
		get
		{
			if (this._BlurMaterial == null)
			{
				this._BlurMaterial = new Material(this._BlurShader);
				this._BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return this._BlurMaterial;
		}
	}

	// Token: 0x17000024 RID: 36
	// (get) Token: 0x060000F0 RID: 240 RVA: 0x0000AD0B File Offset: 0x00008F0B
	private Material fastBloomMaterial
	{
		get
		{
			if (this._fastBloomMaterial == null)
			{
				this._fastBloomMaterial = new Material(this.fastBloomShader);
				this._fastBloomMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return this._fastBloomMaterial;
		}
	}

	// Token: 0x060000F1 RID: 241 RVA: 0x0000AD40 File Offset: 0x00008F40
	private void CreateUniformFogCamera()
	{
		this.UniformFogCameraGO = GameObject.Find("Uniform Fog Volume Camera");
		if (this.UniformFogCameraGO == null)
		{
			this.UniformFogCameraGO = new GameObject();
			this.UniformFogCameraGO.name = "Uniform Fog Volume Camera";
			if (this.UniformFogCamera == null)
			{
				this.UniformFogCamera = this.UniformFogCameraGO.AddComponent<Camera>();
			}
			this.UniformFogCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			this.UniformFogCamera.clearFlags = CameraClearFlags.Color;
			this.UniformFogCamera.renderingPath = RenderingPath.Forward;
			this.UniformFogCamera.enabled = false;
			this.UniformFogCamera.farClipPlane = this.SceneCamera.farClipPlane;
			this.UniformFogCamera.GetComponent<Camera>().allowMSAA = false;
		}
		else
		{
			this.UniformFogCamera = this.UniformFogCameraGO.GetComponent<Camera>();
		}
		this.UniformFogCameraGO.hideFlags = HideFlags.HideInHierarchy;
		this.initFOV = this.SceneCamera.fieldOfView;
	}

	// Token: 0x060000F2 RID: 242 RVA: 0x0000AE48 File Offset: 0x00009048
	private void OnEnable()
	{
		this.SceneCamera = base.gameObject.GetComponent<Camera>();
		this._BlurShader = Shader.Find("Hidden/FogVolumeDensityFilter");
		if (this._BlurShader == null)
		{
			MonoBehaviour.print("Hidden/FogVolumeDensityFilter #SHADER ERROR#");
		}
		this.fastBloomShader = Shader.Find("Hidden/FogVolumeBloom");
		if (this.fastBloomShader == null)
		{
			MonoBehaviour.print("Hidden/FogVolumeBloom #SHADER ERROR#");
		}
		this.CreateUniformFogCamera();
	}

	// Token: 0x060000F3 RID: 243 RVA: 0x0000AEBC File Offset: 0x000090BC
	protected void OnDisable()
	{
		if (this._BlurMaterial)
		{
			Object.DestroyImmediate(this._BlurMaterial);
		}
		if (this._fastBloomMaterial)
		{
			Object.DestroyImmediate(this._fastBloomMaterial);
		}
		if (this.UniformFogCameraGO)
		{
			Object.DestroyImmediate(this.UniformFogCameraGO);
		}
	}

	// Token: 0x060000F4 RID: 244 RVA: 0x0000AF14 File Offset: 0x00009114
	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float num = 0.5f + (float)iteration * this.blurSpread;
		Graphics.BlitMultiTap(source, dest, this.BlurMaterial, new Vector2[]
		{
			new Vector2(-num, -num),
			new Vector2(-num, num),
			new Vector2(num, num),
			new Vector2(num, -num)
		});
	}

	// Token: 0x060000F5 RID: 245 RVA: 0x0000AF80 File Offset: 0x00009180
	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		float num = 1f;
		Graphics.BlitMultiTap(source, dest, this.BlurMaterial, new Vector2[]
		{
			new Vector2(-num, -num),
			new Vector2(-num, num),
			new Vector2(num, num),
			new Vector2(num, -num)
		});
	}

	// Token: 0x060000F6 RID: 246 RVA: 0x0000AFE2 File Offset: 0x000091E2
	public RenderTextureFormat GetRTFormat()
	{
		this.RT_Format = (this.SceneCamera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		return this.RT_Format;
	}

	// Token: 0x060000F7 RID: 247 RVA: 0x0000B002 File Offset: 0x00009202
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x060000F8 RID: 248 RVA: 0x0000B016 File Offset: 0x00009216
	public RenderTextureReadWrite GetRTReadWrite()
	{
		if (!this.SceneCamera.allowHDR)
		{
			return RenderTextureReadWrite.Linear;
		}
		return RenderTextureReadWrite.Default;
	}

	// Token: 0x060000F9 RID: 249 RVA: 0x0000B028 File Offset: 0x00009228
	protected void GetRT(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 0, this.GetRTFormat(), this.GetRTReadWrite());
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Repeat;
	}

	// Token: 0x060000FA RID: 250 RVA: 0x0000B078 File Offset: 0x00009278
	public void ConvolveFogVolume()
	{
		if (this.UniformFogCameraGO == null)
		{
			this.CreateUniformFogCamera();
		}
		int2 @int = new int2(this.screenX, this.screenY);
		this.UniformFogCamera.projectionMatrix = this.SceneCamera.projectionMatrix;
		this.UniformFogCamera.transform.position = this.SceneCamera.transform.position;
		this.UniformFogCamera.transform.rotation = this.SceneCamera.transform.rotation;
		this.GetRT(ref this.RT_FogVolumeConvolution, @int, "RT_FogVolumeConvolution");
		this.UniformFogCamera.targetTexture = this.RT_FogVolumeConvolution;
		this.UniformFogCamera.Render();
		Shader.SetGlobalTexture("RT_FogVolumeConvolution", this.RT_FogVolumeConvolution);
	}

	// Token: 0x17000025 RID: 37
	// (get) Token: 0x060000FB RID: 251 RVA: 0x0000B140 File Offset: 0x00009340
	public static FogVolumeScreen instance
	{
		get
		{
			if (FogVolumeScreen._instance == null)
			{
				FogVolumeScreen._instance = Object.FindObjectOfType<FogVolumeScreen>();
			}
			return FogVolumeScreen._instance;
		}
	}

	// Token: 0x060000FC RID: 252 RVA: 0x0000B160 File Offset: 0x00009360
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		this.ConvolveFogVolume();
		this.GetRT(ref this._source, new int2(Screen.width, Screen.height), "_source");
		Graphics.Blit(source, this._source);
		this.fastBloomMaterial.SetTexture("_source", this._source);
		this.BlurMaterial.SetTexture("_source", this._source);
		this.UniformFogCamera.cullingMask = 1 << FogVolumeScreen.instance.FogVolumeLayer;
		this.FOV_compensation = this.initFOV / this.SceneCamera.fieldOfView;
		Shader.SetGlobalFloat("FOV_compensation", this.FOV_compensation);
		this.fastBloomMaterial.SetFloat("_Falloff", this._Falloff);
		RenderTexture renderTexture = RenderTexture.GetTemporary(this.screenX / this.Downsample, this.screenY / this.Downsample, 0, this.RT_Format);
		this.DownSample4x(source, renderTexture);
		for (int i = 0; i < this.iterations; i++)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(this.screenX / this.Downsample, this.screenY / this.Downsample, 0, this.RT_Format);
			this.FourTapCone(renderTexture, temporary, i);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
		}
		if (this.intensity > 0f)
		{
			Rendering.EnsureKeyword(this.fastBloomMaterial, "BLOOM", true);
			float num = 2f / (float)this._BloomDowsample;
			this.fastBloomMaterial.SetFloat("_Saturation", this._Saturation);
			this.fastBloomMaterial.SetVector("_Parameter", new Vector4(this.blurSize * num, 0f, this.threshold, this.intensity));
			int num2 = source.width / this._BloomDowsample;
			int num3 = source.height / this._BloomDowsample;
			RenderTexture renderTexture2 = RenderTexture.GetTemporary(num2, num3, 0, this.RT_Format);
			renderTexture2.filterMode = FilterMode.Bilinear;
			if (this.SceneBloom)
			{
				Graphics.Blit(source, renderTexture2, this.fastBloomMaterial, 1);
			}
			else
			{
				Graphics.Blit(renderTexture, renderTexture2, this.fastBloomMaterial, 1);
			}
			int num4 = ((this.blurType == FogVolumeScreen.BlurType.Standard) ? 0 : 2);
			for (int j = 1; j < this.blurIterations; j++)
			{
				this.fastBloomMaterial.SetVector("_Parameter", new Vector4(this.blurSize * num + (float)j * 1f, 0f, this.threshold, this.intensity));
				RenderTexture renderTexture3 = RenderTexture.GetTemporary(num2, num3, 0, this.RT_Format);
				renderTexture3.filterMode = FilterMode.Bilinear;
				Graphics.Blit(renderTexture2, renderTexture3, this.fastBloomMaterial, 2 + num4);
				RenderTexture.ReleaseTemporary(renderTexture2);
				renderTexture2 = renderTexture3;
				renderTexture3 = RenderTexture.GetTemporary(num2, num3, 0, this.RT_Format);
				renderTexture3.filterMode = FilterMode.Bilinear;
				Graphics.Blit(renderTexture2, renderTexture3, this.fastBloomMaterial, 3 + num4);
				RenderTexture.ReleaseTemporary(renderTexture2);
				renderTexture2 = renderTexture3;
			}
			this.fastBloomMaterial.SetTexture("_Bloom", renderTexture2);
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
		else
		{
			Rendering.EnsureKeyword(this.fastBloomMaterial, "BLOOM", false);
		}
		Graphics.Blit(renderTexture, destination, this.fastBloomMaterial, 0);
		RenderTexture.ReleaseTemporary(renderTexture);
	}

	// Token: 0x0400017F RID: 383
	[Header("Scene blur")]
	[Range(1f, 8f)]
	public int Downsample = 8;

	// Token: 0x04000180 RID: 384
	[SerializeField]
	[Range(0.001f, 15f)]
	private float _Falloff = 1f;

	// Token: 0x04000181 RID: 385
	private float FOV_compensation;

	// Token: 0x04000182 RID: 386
	private Shader _BlurShader;

	// Token: 0x04000183 RID: 387
	private Camera UniformFogCamera;

	// Token: 0x04000184 RID: 388
	private GameObject UniformFogCameraGO;

	// Token: 0x04000185 RID: 389
	[HideInInspector]
	public Camera SceneCamera;

	// Token: 0x04000186 RID: 390
	private RenderTexture RT_FogVolumeConvolution;

	// Token: 0x04000187 RID: 391
	private RenderTextureFormat RT_Format;

	// Token: 0x04000188 RID: 392
	[HideInInspector]
	public int FogVolumeLayer = -1;

	// Token: 0x04000189 RID: 393
	[SerializeField]
	[HideInInspector]
	private string _FogVolumeLayerName = "FogVolumeUniform";

	// Token: 0x0400018A RID: 394
	private Material _BlurMaterial;

	// Token: 0x0400018B RID: 395
	[Range(0f, 10f)]
	public int iterations = 3;

	// Token: 0x0400018C RID: 396
	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	// Token: 0x0400018D RID: 397
	[Header("Bloom")]
	[Range(1f, 5f)]
	public int _BloomDowsample = 8;

	// Token: 0x0400018E RID: 398
	[Range(0f, 1.5f)]
	public float threshold = 0.35f;

	// Token: 0x0400018F RID: 399
	[Range(0f, 10f)]
	public float intensity = 2.5f;

	// Token: 0x04000190 RID: 400
	[Range(0f, 1f)]
	public float _Saturation = 1f;

	// Token: 0x04000191 RID: 401
	[Range(0f, 5f)]
	public float blurSize = 1f;

	// Token: 0x04000192 RID: 402
	[Range(1f, 10f)]
	public int blurIterations = 4;

	// Token: 0x04000193 RID: 403
	private FogVolumeScreen.BlurType blurType;

	// Token: 0x04000194 RID: 404
	private Shader fastBloomShader;

	// Token: 0x04000195 RID: 405
	private Material _fastBloomMaterial;

	// Token: 0x04000196 RID: 406
	private float initFOV;

	// Token: 0x04000197 RID: 407
	public bool SceneBloom;

	// Token: 0x04000198 RID: 408
	private static FogVolumeScreen _instance;

	// Token: 0x04000199 RID: 409
	private RenderTexture _source;

	// Token: 0x0200022B RID: 555
	public enum BlurType
	{
		// Token: 0x04000E18 RID: 3608
		Standard,
		// Token: 0x04000E19 RID: 3609
		Sgx
	}
}
