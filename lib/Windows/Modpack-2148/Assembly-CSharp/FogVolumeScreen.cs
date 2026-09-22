using System;
using FogVolumeUtilities;
using UnityEngine;

// Token: 0x02000028 RID: 40
[ExecuteInEditMode]
public class FogVolumeScreen : MonoBehaviour
{
	// Token: 0x1700002E RID: 46
	// (get) Token: 0x0600010B RID: 267 RVA: 0x00002FC9 File Offset: 0x000011C9
	public int screenX
	{
		get
		{
			return this.SceneCamera.pixelWidth;
		}
	}

	// Token: 0x1700002F RID: 47
	// (get) Token: 0x0600010C RID: 268 RVA: 0x00002FD6 File Offset: 0x000011D6
	public int screenY
	{
		get
		{
			return this.SceneCamera.pixelHeight;
		}
	}

	// Token: 0x17000030 RID: 48
	// (get) Token: 0x0600010D RID: 269 RVA: 0x00002FE3 File Offset: 0x000011E3
	// (set) Token: 0x0600010E RID: 270 RVA: 0x00002FEB File Offset: 0x000011EB
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

	// Token: 0x0600010F RID: 271 RVA: 0x00003002 File Offset: 0x00001202
	private void SetFogVolumeLayer(string NewFogVolumeLayerName)
	{
		this._FogVolumeLayerName = NewFogVolumeLayerName;
		this.FogVolumeLayer = LayerMask.NameToLayer(this._FogVolumeLayerName);
	}

	// Token: 0x06000110 RID: 272 RVA: 0x0000301C File Offset: 0x0000121C
	private void OnValidate()
	{
		this.SetFogVolumeLayer(this._FogVolumeLayerName);
	}

	// Token: 0x17000031 RID: 49
	// (get) Token: 0x06000111 RID: 273 RVA: 0x0000302A File Offset: 0x0000122A
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

	// Token: 0x17000032 RID: 50
	// (get) Token: 0x06000112 RID: 274 RVA: 0x0000305E File Offset: 0x0000125E
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

	// Token: 0x06000113 RID: 275 RVA: 0x0001C534 File Offset: 0x0001A734
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

	// Token: 0x06000114 RID: 276 RVA: 0x0001C63C File Offset: 0x0001A83C
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

	// Token: 0x06000115 RID: 277 RVA: 0x0001C6B0 File Offset: 0x0001A8B0
	protected void OnDisable()
	{
		if (this._BlurMaterial)
		{
			global::UnityEngine.Object.DestroyImmediate(this._BlurMaterial);
		}
		if (this._fastBloomMaterial)
		{
			global::UnityEngine.Object.DestroyImmediate(this._fastBloomMaterial);
		}
		if (this.UniformFogCameraGO)
		{
			global::UnityEngine.Object.DestroyImmediate(this.UniformFogCameraGO);
		}
	}

	// Token: 0x06000116 RID: 278 RVA: 0x0001C708 File Offset: 0x0001A908
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

	// Token: 0x06000117 RID: 279 RVA: 0x0001C774 File Offset: 0x0001A974
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

	// Token: 0x06000118 RID: 280 RVA: 0x00003092 File Offset: 0x00001292
	public RenderTextureFormat GetRTFormat()
	{
		this.RT_Format = (this.SceneCamera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default);
		return this.RT_Format;
	}

	// Token: 0x06000119 RID: 281 RVA: 0x00002F53 File Offset: 0x00001153
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x0600011A RID: 282 RVA: 0x000030B2 File Offset: 0x000012B2
	public RenderTextureReadWrite GetRTReadWrite()
	{
		if (!this.SceneCamera.allowHDR)
		{
			return RenderTextureReadWrite.Linear;
		}
		return RenderTextureReadWrite.Default;
	}

	// Token: 0x0600011B RID: 283 RVA: 0x0001C7D8 File Offset: 0x0001A9D8
	protected void GetRT(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 0, this.GetRTFormat(), this.GetRTReadWrite());
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Repeat;
	}

	// Token: 0x0600011C RID: 284 RVA: 0x0001C828 File Offset: 0x0001AA28
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

	// Token: 0x17000033 RID: 51
	// (get) Token: 0x0600011D RID: 285 RVA: 0x000030C4 File Offset: 0x000012C4
	public static FogVolumeScreen instance
	{
		get
		{
			if (FogVolumeScreen._instance == null)
			{
				FogVolumeScreen._instance = global::UnityEngine.Object.FindObjectOfType<FogVolumeScreen>();
			}
			return FogVolumeScreen._instance;
		}
	}

	// Token: 0x0600011E RID: 286 RVA: 0x0001C8F0 File Offset: 0x0001AAF0
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

	// Token: 0x040001C5 RID: 453
	[Header("Scene blur")]
	[Range(1f, 8f)]
	public int Downsample = 8;

	// Token: 0x040001C6 RID: 454
	[SerializeField]
	[Range(0.001f, 15f)]
	private float _Falloff = 1f;

	// Token: 0x040001C7 RID: 455
	private float FOV_compensation;

	// Token: 0x040001C8 RID: 456
	private Shader _BlurShader;

	// Token: 0x040001C9 RID: 457
	private Camera UniformFogCamera;

	// Token: 0x040001CA RID: 458
	private GameObject UniformFogCameraGO;

	// Token: 0x040001CB RID: 459
	[HideInInspector]
	public Camera SceneCamera;

	// Token: 0x040001CC RID: 460
	private RenderTexture RT_FogVolumeConvolution;

	// Token: 0x040001CD RID: 461
	private RenderTextureFormat RT_Format;

	// Token: 0x040001CE RID: 462
	[HideInInspector]
	public int FogVolumeLayer = -1;

	// Token: 0x040001CF RID: 463
	[SerializeField]
	[HideInInspector]
	private string _FogVolumeLayerName = "FogVolumeUniform";

	// Token: 0x040001D0 RID: 464
	private Material _BlurMaterial;

	// Token: 0x040001D1 RID: 465
	[Range(0f, 10f)]
	public int iterations = 3;

	// Token: 0x040001D2 RID: 466
	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	// Token: 0x040001D3 RID: 467
	[Header("Bloom")]
	[Range(1f, 5f)]
	public int _BloomDowsample = 8;

	// Token: 0x040001D4 RID: 468
	[Range(0f, 1.5f)]
	public float threshold = 0.35f;

	// Token: 0x040001D5 RID: 469
	[Range(0f, 10f)]
	public float intensity = 2.5f;

	// Token: 0x040001D6 RID: 470
	[Range(0f, 1f)]
	public float _Saturation = 1f;

	// Token: 0x040001D7 RID: 471
	[Range(0f, 5f)]
	public float blurSize = 1f;

	// Token: 0x040001D8 RID: 472
	[Range(1f, 10f)]
	public int blurIterations = 4;

	// Token: 0x040001D9 RID: 473
	private FogVolumeScreen.BlurType blurType;

	// Token: 0x040001DA RID: 474
	private Shader fastBloomShader;

	// Token: 0x040001DB RID: 475
	private Material _fastBloomMaterial;

	// Token: 0x040001DC RID: 476
	private float initFOV;

	// Token: 0x040001DD RID: 477
	public bool SceneBloom;

	// Token: 0x040001DE RID: 478
	private static FogVolumeScreen _instance;

	// Token: 0x040001DF RID: 479
	private RenderTexture _source;

	// Token: 0x02000029 RID: 41
	public enum BlurType
	{
		// Token: 0x040001E1 RID: 481
		Standard,
		// Token: 0x040001E2 RID: 482
		Sgx
	}
}
