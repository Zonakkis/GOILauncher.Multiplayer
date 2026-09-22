using System;
using FogVolumeUtilities;
using UnityEngine;

// Token: 0x0200001F RID: 31
[ExecuteInEditMode]
public class ShadowCamera : MonoBehaviour
{
	// Token: 0x0600011C RID: 284 RVA: 0x0000C8C6 File Offset: 0x0000AAC6
	public RenderTexture GetOpacityRT()
	{
		return this.RT_Opacity;
	}

	// Token: 0x0600011D RID: 285 RVA: 0x0000C8CE File Offset: 0x0000AACE
	public RenderTexture GetOpacityBlurRT()
	{
		return this.RT_OpacityBlur;
	}

	// Token: 0x17000026 RID: 38
	// (get) Token: 0x0600011F RID: 287 RVA: 0x0000C8E8 File Offset: 0x0000AAE8
	// (set) Token: 0x0600011E RID: 286 RVA: 0x0000C8D6 File Offset: 0x0000AAD6
	public ShadowCamera.TextureSize SetTextureSize
	{
		get
		{
			return this.textureSize;
		}
		set
		{
			if (value != this.textureSize)
			{
				this.SetQuality(value);
			}
		}
	}

	// Token: 0x06000120 RID: 288 RVA: 0x0000C8F0 File Offset: 0x0000AAF0
	private void SetQuality(ShadowCamera.TextureSize value)
	{
		this.textureSize = value;
	}

	// Token: 0x17000027 RID: 39
	// (get) Token: 0x06000121 RID: 289 RVA: 0x0000C8F9 File Offset: 0x0000AAF9
	protected Material BlurMaterial
	{
		get
		{
			if (this.blurMaterial == null)
			{
				this.blurMaterial = new Material(this.blurShader);
				this.blurMaterial.hideFlags = HideFlags.DontSave;
			}
			return this.blurMaterial;
		}
	}

	// Token: 0x17000028 RID: 40
	// (get) Token: 0x06000122 RID: 290 RVA: 0x0000C92D File Offset: 0x0000AB2D
	protected Material PostProcessMaterial
	{
		get
		{
			if (this.postProcessMaterial == null)
			{
				this.postProcessMaterial = new Material(this.PostProcessShader);
				this.postProcessMaterial.hideFlags = HideFlags.DontSave;
			}
			return this.postProcessMaterial;
		}
	}

	// Token: 0x06000123 RID: 291 RVA: 0x0000C961 File Offset: 0x0000AB61
	protected void GetRT(ref RenderTexture rt, int size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Repeat;
	}

	// Token: 0x06000124 RID: 292 RVA: 0x0000C990 File Offset: 0x0000AB90
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x06000125 RID: 293 RVA: 0x0000C9A4 File Offset: 0x0000ABA4
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

	// Token: 0x06000126 RID: 294 RVA: 0x0000CA10 File Offset: 0x0000AC10
	private void DownSample(RenderTexture source, RenderTexture dest)
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

	// Token: 0x06000127 RID: 295 RVA: 0x0000CA74 File Offset: 0x0000AC74
	private void Blur(RenderTexture Input, int BlurRTSize)
	{
		RenderTexture renderTexture = null;
		this.GetRT(ref this.RT_OpacityBlur, BlurRTSize, "Shadow blurred");
		this.GetRT(ref renderTexture, BlurRTSize, "Shadow blurred");
		this.DownSample(Input, this.RT_OpacityBlur);
		for (int i = 0; i < this.iterations; i++)
		{
			this.FourTapCone(this.RT_OpacityBlur, renderTexture, i);
			ExtensionMethods.Swap<RenderTexture>(ref this.RT_OpacityBlur, ref renderTexture);
		}
		Shader.SetGlobalTexture("RT_OpacityBlur", this.RT_OpacityBlur);
		this.Fog.RT_OpacityBlur = this.RT_OpacityBlur;
	}

	// Token: 0x06000128 RID: 296 RVA: 0x0000CB00 File Offset: 0x0000AD00
	private void RenderShadowMap()
	{
		this.Fog.FogVolumeShader.maximumLOD = 100;
		this.SetQuality(this.textureSize);
		this.GetRT(ref this.RT_Opacity, (int)this.textureSize, "Opacity");
		this.ThisCamera.targetTexture = this.RT_Opacity;
		this.ThisCamera.Render();
		this.Fog.RT_Opacity = this.RT_Opacity;
		if (this.RT_Opacity != null)
		{
			this.GetRT(ref this.RT_PostProcess, (int)this.textureSize, "Shadow PostProcess");
			this.PostProcessMaterial.SetFloat("ShadowColor", this.Fog.ShadowColor.a);
			Graphics.Blit(this.RT_Opacity, this.RT_PostProcess, this.PostProcessMaterial);
			Graphics.Blit(this.RT_PostProcess, this.RT_Opacity);
			if (this.iterations > 0)
			{
				this.Blur(this.RT_Opacity, (int)(this.textureSize >> (this.Downsampling & 31)));
			}
			else
			{
				Shader.SetGlobalTexture("RT_OpacityBlur", this.RT_Opacity);
				this.Fog.RT_OpacityBlur = this.RT_Opacity;
			}
			this.Fog.RT_Opacity = this.RT_Opacity;
		}
		this.BlurMaterial.SetFloat("ShadowColor", this.Fog.ShadowColor.a);
		this.Fog.FogVolumeShader.maximumLOD = 600;
	}

	// Token: 0x06000129 RID: 297 RVA: 0x0000CC70 File Offset: 0x0000AE70
	private void ShaderLoad()
	{
		this.blurShader = Shader.Find("Hidden/Fog Volume/BlurEffectConeTap");
		if (this.blurShader == null)
		{
			MonoBehaviour.print("Hidden / Fog Volume / BlurEffectConeTap #SHADER ERROR#");
		}
		this.PostProcessShader = Shader.Find("Hidden/Fog Volume/Shadow Postprocess");
		if (this.PostProcessShader == null)
		{
			MonoBehaviour.print("Hidden/Fog Volume/Shadow Postprocess #SHADER ERROR#");
		}
	}

	// Token: 0x0600012A RID: 298 RVA: 0x0000CCD0 File Offset: 0x0000AED0
	private void OnEnable()
	{
		this.ShaderLoad();
		this.Dad = base.transform.parent.gameObject;
		this.Fog = this.Dad.GetComponent<FogVolume>();
		this.ThisCamera = base.gameObject.GetComponent<Camera>();
		this.ThisCamera.depthTextureMode = DepthTextureMode.Depth;
		this.CameraTransform();
	}

	// Token: 0x0600012B RID: 299 RVA: 0x0000CD30 File Offset: 0x0000AF30
	public void CameraTransform()
	{
		if (this.ThisCamera != null)
		{
			this.ThisCamera.orthographicSize = this.Dad.GetComponent<FogVolume>().fogVolumeScale.x / 2f;
			this.ThisCamera.transform.position = this.Dad.transform.position;
			this.ThisCamera.farClipPlane = this.Fog.fogVolumeScale.y + (float)this.Fog.shadowCameraPosition;
			Vector3 vector = new Vector3(0f, 0f, this.Fog.fogVolumeScale.y / 2f - (float)this.Fog.shadowCameraPosition);
			this.ThisCamera.transform.Translate(vector, Space.Self);
			Quaternion quaternion = Quaternion.Euler(90f, 0f, 0f);
			this.ThisCamera.transform.rotation = this.Dad.transform.rotation * quaternion;
			this.ThisCamera.enabled = false;
			if (this.Fog.SunAttached)
			{
				this.Fog.Sun.transform.rotation = this.Dad.transform.rotation * quaternion;
			}
		}
	}

	// Token: 0x0600012C RID: 300 RVA: 0x0000CE81 File Offset: 0x0000B081
	private void Update()
	{
		if (this.Fog.IsVisible && this.Fog.CastShadows)
		{
			if (ExtensionMethods.TimeSnap(this.Fog.ShadowCameraSkippedFrames))
			{
				this.RenderShadowMap();
			}
			this.CameraTransform();
		}
	}

	// Token: 0x0600012D RID: 301 RVA: 0x0000CEBB File Offset: 0x0000B0BB
	private void SafeDestroy(Object obj)
	{
		obj = null;
		Object.DestroyImmediate(obj);
	}

	// Token: 0x0600012E RID: 302 RVA: 0x0000CEC8 File Offset: 0x0000B0C8
	private void OnDisable()
	{
		RenderTexture.active = null;
		this.ThisCamera.targetTexture = null;
		if (this.RT_Opacity)
		{
			this.SafeDestroy(this.RT_Opacity);
		}
		if (this.RT_OpacityBlur)
		{
			this.SafeDestroy(this.RT_OpacityBlur);
		}
		if (this.RT_PostProcess)
		{
			this.SafeDestroy(this.RT_PostProcess);
		}
		if (this.blurMaterial)
		{
			this.SafeDestroy(this.blurMaterial);
		}
		if (this.postProcessMaterial)
		{
			this.SafeDestroy(this.postProcessMaterial);
		}
	}

	// Token: 0x040001E4 RID: 484
	private Camera ThisCamera;

	// Token: 0x040001E5 RID: 485
	private GameObject Dad;

	// Token: 0x040001E6 RID: 486
	private FogVolume Fog;

	// Token: 0x040001E7 RID: 487
	public RenderTexture RT_Opacity;

	// Token: 0x040001E8 RID: 488
	public RenderTexture RT_OpacityBlur;

	// Token: 0x040001E9 RID: 489
	public RenderTexture RT_PostProcess;

	// Token: 0x040001EA RID: 490
	public ShadowCamera.TextureSize textureSize = ShadowCamera.TextureSize._128;

	// Token: 0x040001EB RID: 491
	[Range(0f, 10f)]
	public int iterations = 3;

	// Token: 0x040001EC RID: 492
	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	// Token: 0x040001ED RID: 493
	public int Downsampling;

	// Token: 0x040001EE RID: 494
	private Shader blurShader;

	// Token: 0x040001EF RID: 495
	private Shader PostProcessShader;

	// Token: 0x040001F0 RID: 496
	private Material blurMaterial;

	// Token: 0x040001F1 RID: 497
	private Material postProcessMaterial;

	// Token: 0x0200022D RID: 557
	[Serializable]
	public enum TextureSize
	{
		// Token: 0x04000E1F RID: 3615
		_64 = 64,
		// Token: 0x04000E20 RID: 3616
		_128 = 128,
		// Token: 0x04000E21 RID: 3617
		_256 = 256,
		// Token: 0x04000E22 RID: 3618
		_512 = 512,
		// Token: 0x04000E23 RID: 3619
		_1024 = 1024
	}
}
