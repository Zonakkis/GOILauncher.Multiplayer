using System;
using FogVolumePlaydeadTAA;
using UnityEngine;

// Token: 0x02000018 RID: 24
[ExecuteInEditMode]
[RequireComponent(typeof(Camera), typeof(FrustumJitter), typeof(VelocityBuffer))]
public class FogVolumeTAA : EffectBase
{
	// Token: 0x060000FE RID: 254 RVA: 0x0000B504 File Offset: 0x00009704
	private void Reset()
	{
		this._FogVolumeCamera = base.GetComponent<Camera>();
		this.reprojectionShader = Shader.Find("Hidden/TAA");
		this._FogVolumeCamera = base.GetComponent<Camera>();
		this._frustumJitter = base.GetComponent<FrustumJitter>();
		this._velocityBuffer = base.GetComponent<VelocityBuffer>();
		this._velocityBuffer.velocityShader = Shader.Find("Hidden/VelocityBuffer");
	}

	// Token: 0x060000FF RID: 255 RVA: 0x0000B566 File Offset: 0x00009766
	private void Clear()
	{
		base.EnsureArray<int>(ref this.reprojectionIndex, 2, 0);
		this.reprojectionIndex[0] = -1;
		this.reprojectionIndex[1] = -1;
	}

	// Token: 0x06000100 RID: 256 RVA: 0x0000B588 File Offset: 0x00009788
	private void Awake()
	{
		this.Reset();
		this.Clear();
	}

	// Token: 0x06000101 RID: 257 RVA: 0x0000B598 File Offset: 0x00009798
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

	// Token: 0x06000102 RID: 258 RVA: 0x0000B994 File Offset: 0x00009B94
	public void TAA(ref RenderTexture source)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
		this.Resolve(source, temporary);
		Graphics.Blit(temporary, source);
		RenderTexture.ReleaseTemporary(temporary);
	}

	// Token: 0x06000103 RID: 259 RVA: 0x0000B9D4 File Offset: 0x00009BD4
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

	// Token: 0x0400019A RID: 410
	private static RenderBuffer[] mrt = new RenderBuffer[2];

	// Token: 0x0400019B RID: 411
	private Camera _FogVolumeCamera;

	// Token: 0x0400019C RID: 412
	private FrustumJitter _frustumJitter;

	// Token: 0x0400019D RID: 413
	private VelocityBuffer _velocityBuffer;

	// Token: 0x0400019E RID: 414
	public Shader reprojectionShader;

	// Token: 0x0400019F RID: 415
	private Material reprojectionMaterial;

	// Token: 0x040001A0 RID: 416
	private RenderTexture[,] reprojectionBuffer;

	// Token: 0x040001A1 RID: 417
	private int[] reprojectionIndex = new int[] { -1, -1 };

	// Token: 0x040001A2 RID: 418
	public FogVolumeTAA.Neighborhood neighborhood = FogVolumeTAA.Neighborhood.MinMax4TapVarying;

	// Token: 0x040001A3 RID: 419
	public bool unjitterColorSamples = true;

	// Token: 0x040001A4 RID: 420
	public bool unjitterNeighborhood = true;

	// Token: 0x040001A5 RID: 421
	public bool unjitterReprojection = true;

	// Token: 0x040001A6 RID: 422
	public bool useYCoCg = true;

	// Token: 0x040001A7 RID: 423
	public bool useClipping = true;

	// Token: 0x040001A8 RID: 424
	public bool useDilation;

	// Token: 0x040001A9 RID: 425
	public bool useMotionBlur;

	// Token: 0x040001AA RID: 426
	public bool useOptimizations;

	// Token: 0x040001AB RID: 427
	[Range(0f, 1f)]
	public float feedbackMin = 0.88f;

	// Token: 0x040001AC RID: 428
	[Range(0f, 1f)]
	public float feedbackMax = 0.97f;

	// Token: 0x040001AD RID: 429
	public float motionBlurStrength = 1f;

	// Token: 0x040001AE RID: 430
	public bool motionBlurIgnoreFF;

	// Token: 0x0200022C RID: 556
	public enum Neighborhood
	{
		// Token: 0x04000E1B RID: 3611
		MinMax3x3,
		// Token: 0x04000E1C RID: 3612
		MinMax3x3Rounded,
		// Token: 0x04000E1D RID: 3613
		MinMax4TapVarying
	}
}
