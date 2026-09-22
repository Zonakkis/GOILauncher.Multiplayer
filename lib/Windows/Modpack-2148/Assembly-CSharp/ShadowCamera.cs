using System;
using FogVolumeUtilities;
using UnityEngine;

// Token: 0x02000032 RID: 50
[ExecuteInEditMode]
public class ShadowCamera : MonoBehaviour
{
	// Token: 0x0600013E RID: 318 RVA: 0x000031FF File Offset: 0x000013FF
	public RenderTexture GetOpacityRT()
	{
		return this.RT_Opacity;
	}

	// Token: 0x0600013F RID: 319 RVA: 0x00003207 File Offset: 0x00001407
	public RenderTexture GetOpacityBlurRT()
	{
		return this.RT_OpacityBlur;
	}

	// Token: 0x17000034 RID: 52
	// (get) Token: 0x06000141 RID: 321 RVA: 0x00003221 File Offset: 0x00001421
	// (set) Token: 0x06000140 RID: 320 RVA: 0x0000320F File Offset: 0x0000140F
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

	// Token: 0x06000142 RID: 322 RVA: 0x00003229 File Offset: 0x00001429
	private void SetQuality(ShadowCamera.TextureSize value)
	{
		this.textureSize = value;
	}

	// Token: 0x17000035 RID: 53
	// (get) Token: 0x06000143 RID: 323 RVA: 0x00003232 File Offset: 0x00001432
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

	// Token: 0x17000036 RID: 54
	// (get) Token: 0x06000144 RID: 324 RVA: 0x00003266 File Offset: 0x00001466
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

	// Token: 0x06000145 RID: 325 RVA: 0x0000329A File Offset: 0x0000149A
	protected void GetRT(ref RenderTexture rt, int size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Repeat;
	}

	// Token: 0x06000146 RID: 326 RVA: 0x00002F53 File Offset: 0x00001153
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x06000147 RID: 327 RVA: 0x0001DF38 File Offset: 0x0001C138
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

	// Token: 0x06000148 RID: 328 RVA: 0x0001DFA4 File Offset: 0x0001C1A4
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

	// Token: 0x06000149 RID: 329 RVA: 0x0001E008 File Offset: 0x0001C208
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

	// Token: 0x0600014A RID: 330 RVA: 0x0001E094 File Offset: 0x0001C294
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

	// Token: 0x0600014B RID: 331 RVA: 0x0001E204 File Offset: 0x0001C404
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

	// Token: 0x0600014C RID: 332 RVA: 0x0001E264 File Offset: 0x0001C464
	private void OnEnable()
	{
		this.ShaderLoad();
		this.Dad = base.transform.parent.gameObject;
		this.Fog = this.Dad.GetComponent<FogVolume>();
		this.ThisCamera = base.gameObject.GetComponent<Camera>();
		this.ThisCamera.depthTextureMode = DepthTextureMode.Depth;
		this.CameraTransform();
	}

	// Token: 0x0600014D RID: 333 RVA: 0x0001E2C4 File Offset: 0x0001C4C4
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

	// Token: 0x0600014E RID: 334 RVA: 0x000032C9 File Offset: 0x000014C9
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

	// Token: 0x0600014F RID: 335 RVA: 0x00003303 File Offset: 0x00001503
	private void SafeDestroy(global::UnityEngine.Object obj)
	{
		obj = null;
		global::UnityEngine.Object.DestroyImmediate(obj);
	}

	// Token: 0x06000150 RID: 336 RVA: 0x0001E418 File Offset: 0x0001C618
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

	// Token: 0x04000231 RID: 561
	private Camera ThisCamera;

	// Token: 0x04000232 RID: 562
	private GameObject Dad;

	// Token: 0x04000233 RID: 563
	private FogVolume Fog;

	// Token: 0x04000234 RID: 564
	public RenderTexture RT_Opacity;

	// Token: 0x04000235 RID: 565
	public RenderTexture RT_OpacityBlur;

	// Token: 0x04000236 RID: 566
	public RenderTexture RT_PostProcess;

	// Token: 0x04000237 RID: 567
	public ShadowCamera.TextureSize textureSize = ShadowCamera.TextureSize._128;

	// Token: 0x04000238 RID: 568
	[Range(0f, 10f)]
	public int iterations = 3;

	// Token: 0x04000239 RID: 569
	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	// Token: 0x0400023A RID: 570
	public int Downsampling;

	// Token: 0x0400023B RID: 571
	private Shader blurShader;

	// Token: 0x0400023C RID: 572
	private Shader PostProcessShader;

	// Token: 0x0400023D RID: 573
	private Material blurMaterial;

	// Token: 0x0400023E RID: 574
	private Material postProcessMaterial;

	// Token: 0x02000033 RID: 51
	[Serializable]
	public enum TextureSize
	{
		// Token: 0x04000240 RID: 576
		_64 = 64,
		// Token: 0x04000241 RID: 577
		_128 = 128,
		// Token: 0x04000242 RID: 578
		_256 = 256,
		// Token: 0x04000243 RID: 579
		_512 = 512,
		// Token: 0x04000244 RID: 580
		_1024 = 1024
	}
}
