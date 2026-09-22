using System;
using FogVolumePlaydeadTAA;
using UnityEngine;

// Token: 0x0200002A RID: 42
[ExecuteInEditMode]
[RequireComponent(typeof(Camera), typeof(FrustumJitter), typeof(VelocityBuffer))]
public class FogVolumeTAA : EffectBase
{
	// Token: 0x06000120 RID: 288 RVA: 0x0001CC94 File Offset: 0x0001AE94
	private void Reset()
	{
		this._FogVolumeCamera = base.GetComponent<Camera>();
		this.reprojectionShader = Shader.Find("Hidden/TAA");
		this._FogVolumeCamera = base.GetComponent<Camera>();
		this._frustumJitter = base.GetComponent<FrustumJitter>();
		this._velocityBuffer = base.GetComponent<VelocityBuffer>();
		this._velocityBuffer.velocityShader = Shader.Find("Hidden/VelocityBuffer");
	}

	// Token: 0x06000121 RID: 289 RVA: 0x000030E2 File Offset: 0x000012E2
	private void Clear()
	{
		base.EnsureArray<int>(ref this.reprojectionIndex, 2, 0);
		this.reprojectionIndex[0] = -1;
		this.reprojectionIndex[1] = -1;
	}

	// Token: 0x06000122 RID: 290 RVA: 0x00003104 File Offset: 0x00001304
	private void Awake()
	{
		this.Reset();
		this.Clear();
	}

	// Token: 0x06000123 RID: 291 RVA: 0x0001CCF8 File Offset: 0x0001AEF8
	private void Resolve(RenderTexture source, RenderTexture destination)
	{
		this._velocityBuffer.GenerateVelocityBuffer();
		base.EnsureArray<RenderTexture>(ref this.reprojectionBuffer, 2, 2, null);
		base.EnsureArray<int>(ref this.reprojectionIndex, 2, -1);
		base.EnsureMaterial(ref this.reprojectionMaterial, this.reprojectionShader);
		if (this.reprojectionMaterial == null)
		{
			Graphics.Blit(source, destination);
			return;
		}
		int num = ((this._FogVolumeCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right) ? 1 : 0);
		int width = source.width;
		int height = source.height;
		if (base.EnsureRenderTarget(ref this.reprojectionBuffer[num, 0], width, height, source.format, FilterMode.Bilinear, 0, source.antiAliasing))
		{
			this.Clear();
		}
		if (base.EnsureRenderTarget(ref this.reprojectionBuffer[num, 1], width, height, source.format, FilterMode.Bilinear, 0, source.antiAliasing))
		{
			this.Clear();
		}
		bool flag = !this._FogVolumeCamera.stereoEnabled;
		base.EnsureKeyword(this.reprojectionMaterial, "CAMERA_PERSPECTIVE", !this._FogVolumeCamera.orthographic);
		base.EnsureKeyword(this.reprojectionMaterial, "CAMERA_ORTHOGRAPHIC", this._FogVolumeCamera.orthographic);
		base.EnsureKeyword(this.reprojectionMaterial, "MINMAX_3X3", this.neighborhood == FogVolumeTAA.Neighborhood.MinMax3x3);
		base.EnsureKeyword(this.reprojectionMaterial, "MINMAX_3X3_ROUNDED", this.neighborhood == FogVolumeTAA.Neighborhood.MinMax3x3Rounded);
		base.EnsureKeyword(this.reprojectionMaterial, "MINMAX_4TAP_VARYING", this.neighborhood == FogVolumeTAA.Neighborhood.MinMax4TapVarying);
		base.EnsureKeyword(this.reprojectionMaterial, "UNJITTER_COLORSAMPLES", this.unjitterColorSamples);
		base.EnsureKeyword(this.reprojectionMaterial, "UNJITTER_NEIGHBORHOOD", this.unjitterNeighborhood);
		base.EnsureKeyword(this.reprojectionMaterial, "UNJITTER_REPROJECTION", this.unjitterReprojection);
		base.EnsureKeyword(this.reprojectionMaterial, "USE_YCOCG", this.useYCoCg);
		base.EnsureKeyword(this.reprojectionMaterial, "USE_CLIPPING", this.useClipping);
		base.EnsureKeyword(this.reprojectionMaterial, "USE_DILATION", this.useDilation);
		base.EnsureKeyword(this.reprojectionMaterial, "USE_MOTION_BLUR", this.useMotionBlur && flag);
		base.EnsureKeyword(this.reprojectionMaterial, "USE_MOTION_BLUR_NEIGHBORMAX", this._velocityBuffer.activeVelocityNeighborMax != null);
		base.EnsureKeyword(this.reprojectionMaterial, "USE_OPTIMIZATIONS", this.useOptimizations);
		if (this.reprojectionIndex[num] == -1)
		{
			this.reprojectionIndex[num] = 0;
			this.reprojectionBuffer[num, this.reprojectionIndex[num]].DiscardContents();
			Graphics.Blit(source, this.reprojectionBuffer[num, this.reprojectionIndex[num]]);
		}
		int num2 = this.reprojectionIndex[num];
		int num3 = (this.reprojectionIndex[num] + 1) % 2;
		Vector4 activeSample = this._frustumJitter.activeSample;
		activeSample.x /= (float)width;
		activeSample.y /= (float)height;
		activeSample.z /= (float)width;
		activeSample.w /= (float)height;
		this.reprojectionMaterial.SetVector("_JitterUV", activeSample);
		this.reprojectionMaterial.SetTexture("_VelocityBuffer", this._velocityBuffer.activeVelocityBuffer);
		this.reprojectionMaterial.SetTexture("_VelocityNeighborMax", this._velocityBuffer.activeVelocityNeighborMax);
		this.reprojectionMaterial.SetTexture("_MainTex", source);
		this.reprojectionMaterial.SetTexture("_PrevTex", this.reprojectionBuffer[num, num2]);
		this.reprojectionMaterial.SetFloat("_FeedbackMin", this.feedbackMin);
		this.reprojectionMaterial.SetFloat("_FeedbackMax", this.feedbackMax);
		FogVolumeTAA.mrt[0] = this.reprojectionBuffer[num, num3].colorBuffer;
		FogVolumeTAA.mrt[1] = destination.colorBuffer;
		Graphics.SetRenderTarget(FogVolumeTAA.mrt, source.depthBuffer);
		this.reprojectionMaterial.SetPass(0);
		this.reprojectionBuffer[num, num3].DiscardContents();
		base.DrawFullscreenQuad();
		this.reprojectionIndex[num] = num3;
	}

	// Token: 0x06000124 RID: 292 RVA: 0x0001D0F4 File Offset: 0x0001B2F4
	public void TAA(ref RenderTexture source)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		this.Resolve(source, temporary);
		Graphics.Blit(temporary, source);
		RenderTexture.ReleaseTemporary(temporary);
	}

	// Token: 0x06000125 RID: 293 RVA: 0x0001D134 File Offset: 0x0001B334
	private void OnDisable()
	{
		if (this.reprojectionBuffer != null)
		{
			base.ReleaseRenderTarget(ref this.reprojectionBuffer[0, 0]);
			base.ReleaseRenderTarget(ref this.reprojectionBuffer[0, 1]);
			base.ReleaseRenderTarget(ref this.reprojectionBuffer[1, 0]);
			base.ReleaseRenderTarget(ref this.reprojectionBuffer[1, 1]);
		}
	}

	// Token: 0x040001E3 RID: 483
	private static RenderBuffer[] mrt = new RenderBuffer[2];

	// Token: 0x040001E4 RID: 484
	private Camera _FogVolumeCamera;

	// Token: 0x040001E5 RID: 485
	private FrustumJitter _frustumJitter;

	// Token: 0x040001E6 RID: 486
	private VelocityBuffer _velocityBuffer;

	// Token: 0x040001E7 RID: 487
	public Shader reprojectionShader;

	// Token: 0x040001E8 RID: 488
	private Material reprojectionMaterial;

	// Token: 0x040001E9 RID: 489
	private RenderTexture[,] reprojectionBuffer;

	// Token: 0x040001EA RID: 490
	private int[] reprojectionIndex = new int[] { -1, -1 };

	// Token: 0x040001EB RID: 491
	public FogVolumeTAA.Neighborhood neighborhood = FogVolumeTAA.Neighborhood.MinMax4TapVarying;

	// Token: 0x040001EC RID: 492
	public bool unjitterColorSamples = true;

	// Token: 0x040001ED RID: 493
	public bool unjitterNeighborhood = true;

	// Token: 0x040001EE RID: 494
	public bool unjitterReprojection = true;

	// Token: 0x040001EF RID: 495
	public bool useYCoCg = true;

	// Token: 0x040001F0 RID: 496
	public bool useClipping = true;

	// Token: 0x040001F1 RID: 497
	public bool useDilation;

	// Token: 0x040001F2 RID: 498
	public bool useMotionBlur;

	// Token: 0x040001F3 RID: 499
	public bool useOptimizations;

	// Token: 0x040001F4 RID: 500
	[Range(0f, 1f)]
	public float feedbackMin = 0.88f;

	// Token: 0x040001F5 RID: 501
	[Range(0f, 1f)]
	public float feedbackMax = 0.97f;

	// Token: 0x040001F6 RID: 502
	public float motionBlurStrength = 1f;

	// Token: 0x040001F7 RID: 503
	public bool motionBlurIgnoreFF;

	// Token: 0x0200002B RID: 43
	public enum Neighborhood
	{
		// Token: 0x040001F9 RID: 505
		MinMax3x3,
		// Token: 0x040001FA RID: 506
		MinMax3x3Rounded,
		// Token: 0x040001FB RID: 507
		MinMax4TapVarying
	}
}
