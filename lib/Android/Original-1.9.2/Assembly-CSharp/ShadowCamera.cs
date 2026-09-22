using System;
using FogVolumeUtilities;
using UnityEngine;

// Token: 0x02000033 RID: 51
[ExecuteInEditMode]
public class ShadowCamera : MonoBehaviour
{
	// Token: 0x0600013E RID: 318 RVA: 0x0000D9CC File Offset: 0x0000BDCC
	public RenderTexture GetOpacityRT()
	{
		return this.RT_Opacity;
	}

	// Token: 0x0600013F RID: 319 RVA: 0x0000D9D4 File Offset: 0x0000BDD4
	public RenderTexture GetOpacityBlurRT()
	{
		return this.RT_OpacityBlur;
	}

	// Token: 0x1700002E RID: 46
	// (get) Token: 0x06000141 RID: 321 RVA: 0x0000D9F1 File Offset: 0x0000BDF1
	// (set) Token: 0x06000140 RID: 320 RVA: 0x0000D9DC File Offset: 0x0000BDDC
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

	// Token: 0x06000142 RID: 322 RVA: 0x0000D9F9 File Offset: 0x0000BDF9
	private void SetQuality(ShadowCamera.TextureSize value)
	{
		this.textureSize = value;
	}

	// Token: 0x1700002F RID: 47
	// (get) Token: 0x06000143 RID: 323 RVA: 0x0000DA02 File Offset: 0x0000BE02
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

	// Token: 0x17000030 RID: 48
	// (get) Token: 0x06000144 RID: 324 RVA: 0x0000DA39 File Offset: 0x0000BE39
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

	// Token: 0x06000145 RID: 325 RVA: 0x0000DA70 File Offset: 0x0000BE70
	protected void GetRT(ref RenderTexture rt, int size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Repeat;
	}

	// Token: 0x06000146 RID: 326 RVA: 0x0000DA9F File Offset: 0x0000BE9F
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x06000147 RID: 327 RVA: 0x0000DAB8 File Offset: 0x0000BEB8
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

	// Token: 0x06000148 RID: 328 RVA: 0x0000DB38 File Offset: 0x0000BF38
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

	// Token: 0x06000149 RID: 329 RVA: 0x0000DBB0 File Offset: 0x0000BFB0
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

	// Token: 0x0600014A RID: 330 RVA: 0x0000DC40 File Offset: 0x0000C040
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

	// Token: 0x0600014B RID: 331 RVA: 0x0000DDB4 File Offset: 0x0000C1B4
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

	// Token: 0x0600014C RID: 332 RVA: 0x0000DE18 File Offset: 0x0000C218
	private void OnEnable()
	{
		this.ShaderLoad();
		this.Dad = base.transform.parent.gameObject;
		this.Fog = this.Dad.GetComponent<FogVolume>();
		this.ThisCamera = base.gameObject.GetComponent<Camera>();
		this.CameraTransform();
	}

	// Token: 0x0600014D RID: 333 RVA: 0x0000DE6C File Offset: 0x0000C26C
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

	// Token: 0x0600014E RID: 334 RVA: 0x0000DFC0 File Offset: 0x0000C3C0
	private void Update()
	{
		if (this.Fog.IsVisible && this.Fog.CastShadows && ExtensionMethods.TimeSnap(this.Fog.ShadowCameraSkippedFrames))
		{
			this.RenderShadowMap();
		}
	}

	// Token: 0x0600014F RID: 335 RVA: 0x0000E000 File Offset: 0x0000C400
	private void OnDisable()
	{
		RenderTexture.active = null;
		this.ThisCamera.targetTexture = null;
		if (this.RT_Opacity)
		{
			global::UnityEngine.Object.DestroyImmediate(this.RT_Opacity);
		}
		if (this.RT_OpacityBlur)
		{
			global::UnityEngine.Object.DestroyImmediate(this.RT_OpacityBlur);
		}
		if (this.RT_PostProcess)
		{
			global::UnityEngine.Object.DestroyImmediate(this.RT_PostProcess);
		}
		if (this.blurMaterial)
		{
			global::UnityEngine.Object.DestroyImmediate(this.blurMaterial);
		}
		if (this.postProcessMaterial)
		{
			global::UnityEngine.Object.DestroyImmediate(this.postProcessMaterial);
		}
	}

	// Token: 0x0400023E RID: 574
	private Camera ThisCamera;

	// Token: 0x0400023F RID: 575
	private GameObject Dad;

	// Token: 0x04000240 RID: 576
	private FogVolume Fog;

	// Token: 0x04000241 RID: 577
	public RenderTexture RT_Opacity;

	// Token: 0x04000242 RID: 578
	public RenderTexture RT_OpacityBlur;

	// Token: 0x04000243 RID: 579
	public RenderTexture RT_PostProcess;

	// Token: 0x04000244 RID: 580
	public ShadowCamera.TextureSize textureSize = ShadowCamera.TextureSize._128;

	// Token: 0x04000245 RID: 581
	[Range(0f, 10f)]
	public int iterations = 3;

	// Token: 0x04000246 RID: 582
	[Range(0f, 1f)]
	public float blurSpread = 0.6f;

	// Token: 0x04000247 RID: 583
	public int Downsampling = 2;

	// Token: 0x04000248 RID: 584
	private Shader blurShader;

	// Token: 0x04000249 RID: 585
	private Shader PostProcessShader;

	// Token: 0x0400024A RID: 586
	private Material blurMaterial;

	// Token: 0x0400024B RID: 587
	private Material postProcessMaterial;

	// Token: 0x02000034 RID: 52
	[Serializable]
	public enum TextureSize
	{
		// Token: 0x0400024D RID: 589
		_64 = 64,
		// Token: 0x0400024E RID: 590
		_128 = 128,
		// Token: 0x0400024F RID: 591
		_256 = 256,
		// Token: 0x04000250 RID: 592
		_512 = 512,
		// Token: 0x04000251 RID: 593
		_1024 = 1024
	}
}
