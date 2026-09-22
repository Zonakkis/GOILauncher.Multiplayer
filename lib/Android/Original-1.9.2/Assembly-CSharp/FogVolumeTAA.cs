using System;
using FogVolumePlaydeadTAA;
using UnityEngine;

// Token: 0x02000020 RID: 32
[ExecuteInEditMode]
[RequireComponent(typeof(Camera), typeof(FrustumJitter), typeof(VelocityBuffer))]
public class FogVolumeTAA : EffectBase
{
	// Token: 0x060000ED RID: 237 RVA: 0x0000B0B4 File Offset: 0x000094B4
	private void Reset()
	{
		this._camera = base.GetComponent<Camera>();
		this._FogVolumeCamera = base.gameObject.GetComponent<FogVolumeCamera>();
		this.reprojectionShader = Shader.Find("Hidden/TAA");
		this._camera = base.GetComponent<Camera>();
		this._frustumJitter = base.GetComponent<FrustumJitter>();
		this._frustumJitter.enabled = false;
		this._velocityBuffer = base.GetComponent<VelocityBuffer>();
		this._velocityBuffer.velocityShader = Shader.Find("Hidden/VelocityBuffer");
	}

	// Token: 0x060000EE RID: 238 RVA: 0x0000B133 File Offset: 0x00009533
	private void Clear()
	{
		base.EnsureArray<int>(ref this.reprojectionIndex, 2, 0);
		this.reprojectionIndex[0] = -1;
		this.reprojectionIndex[1] = -1;
	}

	// Token: 0x060000EF RID: 239 RVA: 0x0000B155 File Offset: 0x00009555
	private void Awake()
	{
		this.Reset();
		this.Clear();
	}

	// Token: 0x060000F0 RID: 240 RVA: 0x0000B164 File Offset: 0x00009564
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
		int num = ((this._camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right) ? 0 : 1);
		int width = source.width;
		int height = source.height;
		ref RenderTexture ptr = ref this.reprojectionBuffer[num, 0];
		int num2 = width;
		int num3 = height;
		RenderTextureFormat renderTextureFormat = this._FogVolumeCamera.GetRTFormat();
		FilterMode filterMode = FilterMode.Bilinear;
		int num4 = source.antiAliasing;
		if (base.EnsureRenderTarget(ref ptr, num2, num3, renderTextureFormat, filterMode, 0, num4))
		{
			this.Clear();
		}
		ptr = ref this.reprojectionBuffer[num, 1];
		num4 = width;
		num3 = height;
		renderTextureFormat = this._FogVolumeCamera.GetRTFormat();
		filterMode = FilterMode.Bilinear;
		num2 = source.antiAliasing;
		if (base.EnsureRenderTarget(ref ptr, num4, num3, renderTextureFormat, filterMode, 0, num2))
		{
			this.Clear();
		}
		bool stereoEnabled = this._camera.stereoEnabled;
		bool flag = !stereoEnabled;
		base.EnsureKeyword(this.reprojectionMaterial, "CAMERA_PERSPECTIVE", !this._camera.orthographic);
		base.EnsureKeyword(this.reprojectionMaterial, "CAMERA_ORTHOGRAPHIC", this._camera.orthographic);
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
		int num5 = this.reprojectionIndex[num];
		int num6 = (this.reprojectionIndex[num] + 1) % 2;
		Vector4 activeSample = this._frustumJitter.activeSample;
		activeSample.x /= (float)width;
		activeSample.y /= (float)height;
		activeSample.z /= (float)width;
		activeSample.w /= (float)height;
		this.reprojectionMaterial.SetVector("_JitterUV", activeSample);
		this.reprojectionMaterial.SetTexture("_VelocityBuffer", this._velocityBuffer.activeVelocityBuffer);
		this.reprojectionMaterial.SetTexture("_VelocityNeighborMax", this._velocityBuffer.activeVelocityNeighborMax);
		this.reprojectionMaterial.SetTexture("_MainTex", source);
		this.reprojectionMaterial.SetTexture("_PrevTex", this.reprojectionBuffer[num, num5]);
		this.reprojectionMaterial.SetFloat("_FeedbackMin", this.feedbackMin);
		this.reprojectionMaterial.SetFloat("_FeedbackMax", this.feedbackMax);
		FogVolumeTAA.mrt[0] = this.reprojectionBuffer[num, num6].colorBuffer;
		FogVolumeTAA.mrt[1] = destination.colorBuffer;
		Graphics.SetRenderTarget(FogVolumeTAA.mrt, source.depthBuffer);
		this.reprojectionMaterial.SetPass(0);
		this.reprojectionBuffer[num, num6].DiscardContents();
		base.DrawFullscreenQuad();
		this.reprojectionIndex[num] = num6;
	}

	// Token: 0x060000F1 RID: 241 RVA: 0x0000B5CC File Offset: 0x000099CC
	public void TAA(ref RenderTexture source)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, this._FogVolumeCamera.GetRTFormat(), RenderTextureReadWrite.Default, source.antiAliasing);
		this.Resolve(source, temporary);
		Graphics.Blit(temporary, source);
		RenderTexture.ReleaseTemporary(temporary);
	}

	// Token: 0x060000F2 RID: 242 RVA: 0x0000B618 File Offset: 0x00009A18
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

	// Token: 0x040001AD RID: 429
	private static RenderBuffer[] mrt = new RenderBuffer[2];

	// Token: 0x040001AE RID: 430
	private Camera _camera;

	// Token: 0x040001AF RID: 431
	private FrustumJitter _frustumJitter;

	// Token: 0x040001B0 RID: 432
	private VelocityBuffer _velocityBuffer;

	// Token: 0x040001B1 RID: 433
	public Shader reprojectionShader;

	// Token: 0x040001B2 RID: 434
	private Material reprojectionMaterial;

	// Token: 0x040001B3 RID: 435
	private RenderTexture[,] reprojectionBuffer;

	// Token: 0x040001B4 RID: 436
	private int[] reprojectionIndex = new int[] { -1, -1 };

	// Token: 0x040001B5 RID: 437
	public FogVolumeTAA.Neighborhood neighborhood;

	// Token: 0x040001B6 RID: 438
	public bool unjitterColorSamples;

	// Token: 0x040001B7 RID: 439
	public bool unjitterNeighborhood;

	// Token: 0x040001B8 RID: 440
	public bool unjitterReprojection;

	// Token: 0x040001B9 RID: 441
	public bool useYCoCg;

	// Token: 0x040001BA RID: 442
	public bool useClipping;

	// Token: 0x040001BB RID: 443
	public bool useDilation;

	// Token: 0x040001BC RID: 444
	public bool useMotionBlur;

	// Token: 0x040001BD RID: 445
	public bool useOptimizations = true;

	// Token: 0x040001BE RID: 446
	[Range(0f, 1f)]
	public float feedbackMin = 0.88f;

	// Token: 0x040001BF RID: 447
	[Range(0f, 1f)]
	public float feedbackMax = 0.97f;

	// Token: 0x040001C0 RID: 448
	public float motionBlurStrength = 1f;

	// Token: 0x040001C1 RID: 449
	public bool motionBlurIgnoreFF;

	// Token: 0x040001C2 RID: 450
	private FogVolumeCamera _FogVolumeCamera;

	// Token: 0x02000021 RID: 33
	public enum Neighborhood
	{
		// Token: 0x040001C4 RID: 452
		MinMax3x3,
		// Token: 0x040001C5 RID: 453
		MinMax3x3Rounded,
		// Token: 0x040001C6 RID: 454
		MinMax4TapVarying
	}
}
