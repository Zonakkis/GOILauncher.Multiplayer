using System;
using FogVolumeUtilities;
using UnityEngine;

// Token: 0x0200001E RID: 30
[ExecuteInEditMode]
public class FogVolumeScreen : MonoBehaviour
{
	// Token: 0x17000026 RID: 38
	// (get) Token: 0x060000D8 RID: 216 RVA: 0x0000A527 File Offset: 0x00008927
	public int screenX
	{
		get
		{
			return this.SceneCamera.pixelWidth;
		}
	}

	// Token: 0x17000027 RID: 39
	// (get) Token: 0x060000D9 RID: 217 RVA: 0x0000A534 File Offset: 0x00008934
	public int screenY
	{
		get
		{
			return this.SceneCamera.pixelHeight;
		}
	}

	// Token: 0x17000028 RID: 40
	// (get) Token: 0x060000DA RID: 218 RVA: 0x0000A541 File Offset: 0x00008941
	// (set) Token: 0x060000DB RID: 219 RVA: 0x0000A549 File Offset: 0x00008949
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

	// Token: 0x060000DC RID: 220 RVA: 0x0000A563 File Offset: 0x00008963
	private void SetFogVolumeLayer(string NewFogVolumeLayerName)
	{
		this._FogVolumeLayerName = NewFogVolumeLayerName;
		this.FogVolumeLayer = LayerMask.NameToLayer(this._FogVolumeLayerName);
	}

	// Token: 0x060000DD RID: 221 RVA: 0x0000A57D File Offset: 0x0000897D
	private void OnValidate()
	{
		this.SetFogVolumeLayer(this._FogVolumeLayerName);
	}

	// Token: 0x17000029 RID: 41
	// (get) Token: 0x060000DE RID: 222 RVA: 0x0000A58B File Offset: 0x0000898B
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

	// Token: 0x1700002A RID: 42
	// (get) Token: 0x060000DF RID: 223 RVA: 0x0000A5C2 File Offset: 0x000089C2
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

	// Token: 0x060000E0 RID: 224 RVA: 0x0000A5FC File Offset: 0x000089FC
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
		this.UniformFogCameraGO.hideFlags = HideFlags.None;
		this.initFOV = this.SceneCamera.fieldOfView;
	}

	// Token: 0x060000E1 RID: 225 RVA: 0x0000A70C File Offset: 0x00008B0C
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

	// Token: 0x060000E2 RID: 226 RVA: 0x0000A788 File Offset: 0x00008B88
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

	// Token: 0x060000E3 RID: 227 RVA: 0x0000A7E8 File Offset: 0x00008BE8
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

	// Token: 0x060000E4 RID: 228 RVA: 0x0000A868 File Offset: 0x00008C68
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

	// Token: 0x060000E5 RID: 229 RVA: 0x0000A8DE File Offset: 0x00008CDE
	public RenderTextureFormat GetRTFormat()
	{
		this.RT_Format = ((!this.SceneCamera.allowHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		return this.RT_Format;
	}

	// Token: 0x060000E6 RID: 230 RVA: 0x0000A904 File Offset: 0x00008D04
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x060000E7 RID: 231 RVA: 0x0000A91B File Offset: 0x00008D1B
	public RenderTextureReadWrite GetRTReadWrite()
	{
		return (!this.SceneCamera.allowHDR) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
	}

	// Token: 0x060000E8 RID: 232 RVA: 0x0000A934 File Offset: 0x00008D34
	protected void GetRT(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 0, this.GetRTFormat(), this.GetRTReadWrite());
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Repeat;
	}

	// Token: 0x060000E9 RID: 233 RVA: 0x0000A984 File Offset: 0x00008D84
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

	// Token: 0x1700002B RID: 43
	// (get) Token: 0x060000EA RID: 234 RVA: 0x0000AA4F File Offset: 0x00008E4F
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

	// Token: 0x060000EB RID: 235 RVA: 0x0000AA70 File Offset: 0x00008E70
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
			int num4 = ((this.blurType != FogVolumeScreen.BlurType.Standard) ? 2 : 0);
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

	// Token: 0x0400018F RID: 399
	[Header("Scene blur")]
	[Range(1f, 8f)]
	public int Downsample = 8;

	// Token: 0x04000190 RID: 400
	[SerializeField]
	[Range(0.001f, 15f)]
	private float _Falloff = 1f;

	// Token: 0x04000191 RID: 401
	private float FOV_compensation;

	// Token: 0x04000192 RID: 402
	private Shader _BlurShader;

	// Token: 0x04000193 RID: 403
	private Camera UniformFogCamera;

	// Token: 0x04000194 RID: 404
	private GameObject UniformFogCameraGO;

	// Token: 0x04000195 RID: 405
	[HideInInspector]
	public Camera SceneCamera;

	// Token: 0x04000196 RID: 406
	private RenderTexture RT_FogVolumeConvolution;

	// Token: 0x04000197 RID: 407
	private RenderTextureFormat RT_Format;

	// Token: 0x04000198 RID: 408
	[HideInInspector]
	public int FogVolumeLayer = -1;

	// Token: 0x04000199 RID: 409
	[SerializeField]
	[HideInInspector]
	private string _FogVolumeLayerName = "FogVolumeUniform";

	// Token: 0x0400019A RID: 410
	private Material _BlurMaterial;

	// Token: 0x0400019B RID: 411
	[Range(0f, 10f)]
	public int iterations = 3;

	// Token: 0x0400019C RID: 412
	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	// Token: 0x0400019D RID: 413
	[Header("Bloom")]
	[Range(1f, 5f)]
	public int _BloomDowsample = 8;

	// Token: 0x0400019E RID: 414
	[Range(0f, 1.5f)]
	public float threshold = 0.35f;

	// Token: 0x0400019F RID: 415
	[Range(0f, 10f)]
	public float intensity = 2.5f;

	// Token: 0x040001A0 RID: 416
	[Range(0f, 1f)]
	public float _Saturation = 1f;

	// Token: 0x040001A1 RID: 417
	[Range(0f, 5f)]
	public float blurSize = 1f;

	// Token: 0x040001A2 RID: 418
	[Range(1f, 10f)]
	public int blurIterations = 4;

	// Token: 0x040001A3 RID: 419
	private FogVolumeScreen.BlurType blurType;

	// Token: 0x040001A4 RID: 420
	private Shader fastBloomShader;

	// Token: 0x040001A5 RID: 421
	private Material _fastBloomMaterial;

	// Token: 0x040001A6 RID: 422
	private float initFOV;

	// Token: 0x040001A7 RID: 423
	public bool SceneBloom;

	// Token: 0x040001A8 RID: 424
	private static FogVolumeScreen _instance;

	// Token: 0x040001A9 RID: 425
	private RenderTexture _source;

	// Token: 0x0200001F RID: 31
	public enum BlurType
	{
		// Token: 0x040001AB RID: 427
		Standard,
		// Token: 0x040001AC RID: 428
		Sgx
	}
}
