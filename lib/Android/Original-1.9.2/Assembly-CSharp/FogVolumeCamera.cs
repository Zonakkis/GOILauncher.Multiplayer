using System;
using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.XR;

// Token: 0x0200000B RID: 11
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(FogVolumeRenderManager))]
public class FogVolumeCamera : MonoBehaviour
{
	// Token: 0x17000005 RID: 5
	// (get) Token: 0x0600002F RID: 47 RVA: 0x000059AB File Offset: 0x00003DAB
	public int screenX
	{
		get
		{
			return this.SceneCamera.pixelWidth;
		}
	}

	// Token: 0x17000006 RID: 6
	// (get) Token: 0x06000030 RID: 48 RVA: 0x000059B8 File Offset: 0x00003DB8
	public int screenY
	{
		get
		{
			return this.SceneCamera.pixelHeight;
		}
	}

	// Token: 0x17000007 RID: 7
	// (get) Token: 0x06000032 RID: 50 RVA: 0x000059DA File Offset: 0x00003DDA
	// (set) Token: 0x06000031 RID: 49 RVA: 0x000059C5 File Offset: 0x00003DC5
	public FogVolumeCamera.UpsampleMode upsampleMode
	{
		get
		{
			return this._upsampleMode;
		}
		set
		{
			if (value != this._upsampleMode)
			{
				this.SetUpsampleMode(value);
			}
		}
	}

	// Token: 0x06000033 RID: 51 RVA: 0x000059E2 File Offset: 0x00003DE2
	private void SetUpsampleMode(FogVolumeCamera.UpsampleMode value)
	{
		this._upsampleMode = value;
		this.UpdateBilateralDownsampleModeSwitch();
	}

	// Token: 0x06000034 RID: 52 RVA: 0x000059F1 File Offset: 0x00003DF1
	public static bool BilateralUpsamplingEnabled()
	{
		return SystemInfo.graphicsShaderLevel >= 40;
	}

	// Token: 0x06000035 RID: 53 RVA: 0x00005A00 File Offset: 0x00003E00
	private void ReleaseLowProfileDepthRT()
	{
		if (this.lowProfileDepthRT != null)
		{
			for (int i = 0; i < this.lowProfileDepthRT.Length; i++)
			{
				RenderTexture.ReleaseTemporary(this.lowProfileDepthRT[i]);
			}
			this.lowProfileDepthRT = null;
		}
	}

	// Token: 0x17000008 RID: 8
	// (get) Token: 0x06000036 RID: 54 RVA: 0x00005A45 File Offset: 0x00003E45
	// (set) Token: 0x06000037 RID: 55 RVA: 0x00005A4D File Offset: 0x00003E4D
	public bool useBilateralUpsampling
	{
		get
		{
			return this._useBilateralUpsampling;
		}
		set
		{
			if (this._useBilateralUpsampling != value)
			{
				this.SetUseBilateralUpsampling(value);
			}
		}
	}

	// Token: 0x06000038 RID: 56 RVA: 0x00005A64 File Offset: 0x00003E64
	private void SetUseBilateralUpsampling(bool b)
	{
		this._useBilateralUpsampling = b;
		if (this._useBilateralUpsampling)
		{
			if (this.bilateralMaterial == null)
			{
				this.bilateralMaterial = new Material(Shader.Find("Hidden/Upsample"));
				if (this.bilateralMaterial == null)
				{
					Debug.Log("#ERROR# Hidden/Upsample");
				}
				this.UpdateBilateralDownsampleModeSwitch();
				this.ShowBilateralEdge(this._showBilateralEdge);
			}
		}
		else
		{
			this.bilateralMaterial = null;
		}
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00005AE4 File Offset: 0x00003EE4
	private void UpdateBilateralDownsampleModeSwitch()
	{
		if (this.bilateralMaterial != null)
		{
			switch (this._upsampleMode)
			{
			case FogVolumeCamera.UpsampleMode.DOWNSAMPLE_MIN:
				this.bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				break;
			case FogVolumeCamera.UpsampleMode.DOWNSAMPLE_MAX:
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				this.bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				break;
			case FogVolumeCamera.UpsampleMode.DOWNSAMPLE_CHESSBOARD:
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				this.bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				break;
			}
		}
	}

	// Token: 0x17000009 RID: 9
	// (get) Token: 0x0600003B RID: 59 RVA: 0x00005BD9 File Offset: 0x00003FD9
	// (set) Token: 0x0600003A RID: 58 RVA: 0x00005BC4 File Offset: 0x00003FC4
	public bool showBilateralEdge
	{
		get
		{
			return this._showBilateralEdge;
		}
		set
		{
			if (value != this._showBilateralEdge)
			{
				this.ShowBilateralEdge(value);
			}
		}
	}

	// Token: 0x0600003C RID: 60 RVA: 0x00005BE4 File Offset: 0x00003FE4
	public void ShowBilateralEdge(bool b)
	{
		this._showBilateralEdge = b;
		if (this.bilateralMaterial)
		{
			if (this.showBilateralEdge)
			{
				this.bilateralMaterial.EnableKeyword("VISUALIZE_EDGE");
			}
			else
			{
				this.bilateralMaterial.DisableKeyword("VISUALIZE_EDGE");
			}
		}
	}

	// Token: 0x0600003D RID: 61 RVA: 0x00005C38 File Offset: 0x00004038
	private Texture2D MakeTex(Color col)
	{
		Color[] array = new Color[] { col };
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	// Token: 0x0600003E RID: 62 RVA: 0x00005C6F File Offset: 0x0000406F
	private void ShaderLoad()
	{
		this.depthShader = Shader.Find("Hidden/Fog Volume/Depth");
		if (this.depthShader == null)
		{
			MonoBehaviour.print("Hidden/Fog Volume/Depth #SHADER ERROR#");
		}
	}

	// Token: 0x0600003F RID: 63 RVA: 0x00005C9C File Offset: 0x0000409C
	public RenderTextureReadWrite GetRTReadWrite()
	{
		return (!this.SceneCamera.allowHDR) ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
	}

	// Token: 0x06000040 RID: 64 RVA: 0x00005CB5 File Offset: 0x000040B5
	public RenderTextureFormat GetRTFormat()
	{
		if (!this._FogVolumeRenderer.TAA)
		{
			return (!this.SceneCamera.allowHDR) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;
		}
		return RenderTextureFormat.ARGBHalf;
	}

	// Token: 0x06000041 RID: 65 RVA: 0x00005CE4 File Offset: 0x000040E4
	protected void GetRT(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 0, this.GetRTFormat(), this.GetRTReadWrite());
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00005D33 File Offset: 0x00004133
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x06000043 RID: 67 RVA: 0x00005D4C File Offset: 0x0000414C
	private void OnEnable()
	{
		this.nullTex = this.MakeTex(Color.black);
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
		{
			this.rt_DepthFormat = RenderTextureFormat.RFloat;
		}
		else
		{
			this.rt_DepthFormat = RenderTextureFormat.DefaultHDR;
		}
		this._FogVolumeDataGO = GameObject.FindGameObjectWithTag("FogVolumeData");
		if (this._FogVolumeDataGO)
		{
			this._FogVolumeData = this._FogVolumeDataGO.GetComponent<FogVolumeData>();
		}
		if (this._FogVolumeData)
		{
			this.SceneCamera = this._FogVolumeData.GetFogVolumeCamera;
		}
		if (this.SceneCamera == null)
		{
			Debug.Log("FogVolumeCamera.cs can't get a valid camera from 'Fog Volume Data'\n Assigning BackgroundCam - M@ Edited this");
			this.SceneCamera = GameObject.FindGameObjectWithTag("BackgroundCamera").GetComponent<Camera>();
		}
		this.ShaderLoad();
		this.SetUpsampleMode(this._upsampleMode);
		this.ShowBilateralEdge(this._showBilateralEdge);
		this._FogVolumeRenderer = this.SceneCamera.GetComponent<FogVolumeRenderer>();
		if (this._FogVolumeRenderer == null && !this._FogVolumeData.ForceNoRenderer)
		{
			FogVolumeRenderer fogVolumeRenderer = this.SceneCamera.gameObject.AddComponent<FogVolumeRenderer>();
			fogVolumeRenderer.enabled = true;
		}
		this.ThisCamera = base.GetComponent<Camera>();
		this.ThisCamera.enabled = false;
		this.ThisCamera.clearFlags = CameraClearFlags.Color;
		this.ThisCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		this.ThisCamera.farClipPlane = this.SceneCamera.farClipPlane;
		this._CameraRender = base.GetComponent<FogVolumeRenderManager>();
		if (this._CameraRender == null)
		{
			this._CameraRender = base.gameObject.AddComponent<FogVolumeRenderManager>();
		}
		this._CameraRender.SceneCamera = this.SceneCamera;
		this._CameraRender.SecondaryCamera = this.ThisCamera;
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00005F24 File Offset: 0x00004324
	private void CameraUpdate()
	{
		if (this.SceneCamera)
		{
			this.ThisCamera.aspect = this.SceneCamera.aspect;
			this.ThisCamera.farClipPlane = this.SceneCamera.farClipPlane;
			this.ThisCamera.nearClipPlane = this.SceneCamera.nearClipPlane;
			this.ThisCamera.allowHDR = this.SceneCamera.allowHDR;
			this.ThisCamera.projectionMatrix = this.SceneCamera.projectionMatrix;
		}
	}

	// Token: 0x06000045 RID: 69 RVA: 0x00005FAF File Offset: 0x000043AF
	protected void Get_RT_Depth(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 16, this.rt_DepthFormat);
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
	}

	// Token: 0x06000046 RID: 70 RVA: 0x00005FF0 File Offset: 0x000043F0
	public void RenderDepth()
	{
		if (this._FogVolumeRenderer.GenerateDepth)
		{
			this.ThisCamera.cullingMask = this._FogVolumeRenderer.DepthLayer2;
			if (this.SceneCamera.stereoEnabled)
			{
				Shader.EnableKeyword("FOG_VOLUME_STEREO_ON");
				if (this.SceneCamera.stereoTargetEye == StereoTargetEyeMask.Both || this.SceneCamera.stereoTargetEye == StereoTargetEyeMask.Left)
				{
					Vector3 vector = this.SceneCamera.transform.parent.TransformPoint(InputTracking.GetLocalPosition(0));
					Quaternion rotation = this.SceneCamera.transform.rotation;
					Matrix4x4 stereoProjectionMatrix = this.SceneCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
					this.Get_RT_Depth(ref this.RT_Depth, new int2(this.screenX, this.screenY), "RT_DepthLeft");
					this._CameraRender.RenderEye(this.RT_Depth, vector, rotation, stereoProjectionMatrix, this.depthShader);
					Shader.SetGlobalTexture("RT_Depth", this.RT_Depth);
				}
				if (this.SceneCamera.stereoTargetEye == StereoTargetEyeMask.Both || this.SceneCamera.stereoTargetEye == StereoTargetEyeMask.Right)
				{
					Vector3 vector2 = this.SceneCamera.transform.parent.TransformPoint(InputTracking.GetLocalPosition(1));
					Quaternion rotation2 = this.SceneCamera.transform.rotation;
					Matrix4x4 stereoProjectionMatrix2 = this.SceneCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
					this.Get_RT_Depth(ref this.RT_DepthR, new int2(this.screenX, this.screenY), "RT_DepthRight");
					this._CameraRender.RenderEye(this.RT_DepthR, vector2, rotation2, stereoProjectionMatrix2, this.depthShader);
					Shader.SetGlobalTexture("RT_DepthR", this.RT_DepthR);
				}
			}
			else
			{
				Shader.DisableKeyword("FOG_VOLUME_STEREO_ON");
				this.Get_RT_Depth(ref this.RT_Depth, new int2(this.screenX, this.screenY), "RT_Depth");
				this._CameraRender.RenderEye(this.RT_Depth, this.SceneCamera.transform.position, this.SceneCamera.transform.rotation, this.SceneCamera.projectionMatrix, this.depthShader);
				Shader.SetGlobalTexture("RT_Depth", this.RT_Depth);
			}
		}
		else
		{
			Shader.SetGlobalTexture("RT_Depth", this.nullTex);
			Shader.SetGlobalTexture("RT_DepthR", this.nullTex);
		}
	}

	// Token: 0x06000047 RID: 71 RVA: 0x0000623C File Offset: 0x0000463C
	public void Render()
	{
		this.CameraUpdate();
		if (this._Downsample > 0)
		{
			this.RenderDepth();
			this.ThisCamera.cullingMask = 1 << LayerMask.NameToLayer("FogVolume");
			this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
			int2 @int = new int2(this.screenX / this._Downsample, this.screenY / this._Downsample);
			if (this.SceneCamera.stereoEnabled)
			{
				Shader.EnableKeyword("FOG_VOLUME_STEREO_ON");
				if (this.SceneCamera.stereoTargetEye == StereoTargetEyeMask.Both || this.SceneCamera.stereoTargetEye == StereoTargetEyeMask.Left)
				{
					Vector3 vector = this.SceneCamera.transform.parent.TransformPoint(InputTracking.GetLocalPosition(0));
					Quaternion rotation = this.SceneCamera.transform.rotation;
					Matrix4x4 stereoProjectionMatrix = this.SceneCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
					this.GetRT(ref this.RT_FogVolume, @int, "RT_FogVolumeLeft");
					this._CameraRender.RenderEye(this.RT_FogVolume, vector, rotation, stereoProjectionMatrix, null);
				}
				if (this.SceneCamera.stereoTargetEye == StereoTargetEyeMask.Both || this.SceneCamera.stereoTargetEye == StereoTargetEyeMask.Right)
				{
					Vector3 vector2 = this.SceneCamera.transform.parent.TransformPoint(InputTracking.GetLocalPosition(1));
					Quaternion rotation2 = this.SceneCamera.transform.rotation;
					Matrix4x4 stereoProjectionMatrix2 = this.SceneCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
					this.GetRT(ref this.RT_FogVolumeR, @int, "RT_FogVolumeRight");
					this._CameraRender.RenderEye(this.RT_FogVolumeR, vector2, rotation2, stereoProjectionMatrix2, null);
				}
			}
			else
			{
				Shader.DisableKeyword("FOG_VOLUME_STEREO_ON");
				this.GetRT(ref this.RT_FogVolume, @int, "RT_FogVolume");
				this._CameraRender.RenderEye(this.RT_FogVolume, this.SceneCamera.transform.position, this.SceneCamera.transform.rotation, this.SceneCamera.projectionMatrix, null);
			}
			if (this.RT_FogVolume)
			{
				if (this._FogVolumeRenderer.TAA && Application.isPlaying)
				{
					this._FogVolumeRenderer._TAA.TAA(ref this.RT_FogVolume);
				}
				Shader.SetGlobalTexture("RT_FogVolume", this.RT_FogVolume);
			}
			if (this.RT_FogVolumeR)
			{
				if (this._FogVolumeRenderer.TAA && Application.isPlaying)
				{
					this._FogVolumeRenderer._TAA.TAA(ref this.RT_FogVolumeR);
				}
				Shader.SetGlobalTexture("RT_FogVolumeR", this.RT_FogVolumeR);
			}
			if (this.useBilateralUpsampling && this._FogVolumeRenderer.GenerateDepth)
			{
				if (this.bilateralMaterial)
				{
					this.ReleaseLowProfileDepthRT();
					this.lowProfileDepthRT = new RenderTexture[this._Downsample];
					for (int i = 0; i < this._Downsample; i++)
					{
						int num = this.screenX / (i + 1);
						int num2 = this.screenY / (i + 1);
						int num3 = this.screenX / Mathf.Max(i, 1);
						int num4 = this.screenY / Mathf.Max(i, 1);
						Vector4 vector3 = new Vector4(1f / (float)num3, 1f / (float)num4, 0f, 0f);
						this.bilateralMaterial.SetFloat("_UpsampleDepthThreshold", this.upsampleDepthThreshold);
						this.bilateralMaterial.SetVector("_TexelSize", vector3);
						this.bilateralMaterial.SetTexture("_HiResDepthBuffer", this.RT_Depth);
						this.lowProfileDepthRT[i] = RenderTexture.GetTemporary(num, num2, 0, this.rt_DepthFormat, this.GetRTReadWrite());
						this.lowProfileDepthRT[i].name = "lowProfileDepthRT_" + i;
						Graphics.Blit(null, this.lowProfileDepthRT[i], this.bilateralMaterial, 0);
					}
					Shader.SetGlobalTexture("RT_Depth", this.lowProfileDepthRT[this.lowProfileDepthRT.Length - 1]);
				}
				if (this.bilateralMaterial)
				{
					for (int j = this._Downsample - 1; j >= 0; j--)
					{
						int num5 = this.screenX / Mathf.Max(j, 1);
						int num6 = this.screenY / Mathf.Max(j, 1);
						int num7 = this.screenX / (j + 1);
						int num8 = this.screenY / (j + 1);
						Vector4 vector4 = new Vector4(1f / (float)num7, 1f / (float)num8, 0f, 0f);
						this.bilateralMaterial.SetVector("_TexelSize", vector4);
						this.bilateralMaterial.SetVector("_InvdUV", new Vector4((float)this.RT_FogVolume.width, (float)this.RT_FogVolume.height, 0f, 0f));
						this.bilateralMaterial.SetTexture("_HiResDepthBuffer", this.RT_Depth);
						this.bilateralMaterial.SetTexture("_LowResDepthBuffer", this.lowProfileDepthRT[j]);
						this.bilateralMaterial.SetTexture("_LowResColor", this.RT_FogVolume);
						RenderTexture temporary = RenderTexture.GetTemporary(num5, num6, 0, this.GetRTFormat(), this.GetRTReadWrite());
						temporary.filterMode = FilterMode.Bilinear;
						Graphics.Blit(null, temporary, this.bilateralMaterial, 1);
						RenderTexture rt_FogVolume = this.RT_FogVolume;
						this.RT_FogVolume = temporary;
						RenderTexture.ReleaseTemporary(rt_FogVolume);
					}
				}
				this.ReleaseLowProfileDepthRT();
			}
			Shader.SetGlobalTexture("RT_FogVolume", this.RT_FogVolume);
		}
		else
		{
			this.ReleaseRT(this.RT_FogVolume);
		}
	}

	// Token: 0x06000048 RID: 72 RVA: 0x000067D4 File Offset: 0x00004BD4
	private void OnDisable()
	{
		if (this.ThisCamera)
		{
			this.ThisCamera.targetTexture = null;
		}
		global::UnityEngine.Object.DestroyImmediate(this.RT_FogVolume);
		this.ReleaseRT(this.RT_FogVolume);
		global::UnityEngine.Object.DestroyImmediate(this.RT_FogVolumeR);
		this.ReleaseRT(this.RT_FogVolumeR);
		this.ReleaseRT(this.RT_Depth);
		global::UnityEngine.Object.DestroyImmediate(this.RT_Depth);
		this.ReleaseRT(this.RT_DepthR);
		global::UnityEngine.Object.DestroyImmediate(this.RT_DepthR);
	}

	// Token: 0x040000D3 RID: 211
	[HideInInspector]
	public string FogVolumeResolution;

	// Token: 0x040000D4 RID: 212
	public RenderTexture RT_FogVolume;

	// Token: 0x040000D5 RID: 213
	public RenderTexture RT_FogVolumeR;

	// Token: 0x040000D6 RID: 214
	[HideInInspector]
	public int _Downsample;

	// Token: 0x040000D7 RID: 215
	private FogVolumeRenderer _FogVolumeRenderer;

	// Token: 0x040000D8 RID: 216
	[HideInInspector]
	public Camera ThisCamera;

	// Token: 0x040000D9 RID: 217
	[HideInInspector]
	public Camera SceneCamera;

	// Token: 0x040000DA RID: 218
	public LayerMask DepthMask = 0;

	// Token: 0x040000DB RID: 219
	public RenderTexture RT_Depth;

	// Token: 0x040000DC RID: 220
	public RenderTexture RT_DepthR;

	// Token: 0x040000DD RID: 221
	private Shader depthShader;

	// Token: 0x040000DE RID: 222
	[HideInInspector]
	public float upsampleDepthThreshold = 0.02f;

	// Token: 0x040000DF RID: 223
	private FogVolumeCamera.UpsampleMode _upsampleMode;

	// Token: 0x040000E0 RID: 224
	public RenderTexture[] lowProfileDepthRT;

	// Token: 0x040000E1 RID: 225
	private bool _useBilateralUpsampling;

	// Token: 0x040000E2 RID: 226
	private Material bilateralMaterial;

	// Token: 0x040000E3 RID: 227
	private bool _showBilateralEdge;

	// Token: 0x040000E4 RID: 228
	private FogVolumeRenderManager _CameraRender;

	// Token: 0x040000E5 RID: 229
	private RenderTextureFormat rt_DepthFormat;

	// Token: 0x040000E6 RID: 230
	private FogVolumeData _FogVolumeData;

	// Token: 0x040000E7 RID: 231
	private GameObject _FogVolumeDataGO;

	// Token: 0x040000E8 RID: 232
	private Texture2D nullTex;

	// Token: 0x0200000C RID: 12
	[Serializable]
	public enum UpsampleMode
	{
		// Token: 0x040000EA RID: 234
		DOWNSAMPLE_MIN,
		// Token: 0x040000EB RID: 235
		DOWNSAMPLE_MAX,
		// Token: 0x040000EC RID: 236
		DOWNSAMPLE_CHESSBOARD
	}

	// Token: 0x0200000D RID: 13
	public enum UpsampleMaterialPass
	{
		// Token: 0x040000EE RID: 238
		DEPTH_DOWNSAMPLE,
		// Token: 0x040000EF RID: 239
		BILATERAL_UPSAMPLE
	}
}
