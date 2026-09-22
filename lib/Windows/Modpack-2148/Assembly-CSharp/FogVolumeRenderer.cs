using System;
using FogVolumePlaydeadTAA;
using FogVolumeUtilities;
using UnityEngine;

// Token: 0x02000024 RID: 36
[ExecuteInEditMode]
public class FogVolumeRenderer : MonoBehaviour
{
	// Token: 0x060000EB RID: 235 RVA: 0x00002E89 File Offset: 0x00001089
	public void setDownsample(int val)
	{
		this._Downsample = val;
	}

	// Token: 0x1700002B RID: 43
	// (get) Token: 0x060000ED RID: 237 RVA: 0x00002EA4 File Offset: 0x000010A4
	// (set) Token: 0x060000EC RID: 236 RVA: 0x00002E92 File Offset: 0x00001092
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

	// Token: 0x060000EE RID: 238 RVA: 0x00002EAC File Offset: 0x000010AC
	public void ShowBilateralEdge(bool b)
	{
		this._showBilateralEdge = b;
		if (this.bilateralMaterial)
		{
			if (this.showBilateralEdge)
			{
				this.bilateralMaterial.EnableKeyword("VISUALIZE_EDGE");
				return;
			}
			this.bilateralMaterial.DisableKeyword("VISUALIZE_EDGE");
		}
	}

	// Token: 0x1700002C RID: 44
	// (get) Token: 0x060000EF RID: 239 RVA: 0x00002EEB File Offset: 0x000010EB
	// (set) Token: 0x060000F0 RID: 240 RVA: 0x00002EF3 File Offset: 0x000010F3
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

	// Token: 0x060000F1 RID: 241 RVA: 0x0001AFC8 File Offset: 0x000191C8
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
				return;
			}
		}
		else
		{
			this.bilateralMaterial = null;
		}
	}

	// Token: 0x1700002D RID: 45
	// (get) Token: 0x060000F3 RID: 243 RVA: 0x00002F17 File Offset: 0x00001117
	// (set) Token: 0x060000F2 RID: 242 RVA: 0x00002F05 File Offset: 0x00001105
	public FogVolumeRenderer.UpsampleMode upsampleMode
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

	// Token: 0x060000F4 RID: 244 RVA: 0x0001B03C File Offset: 0x0001923C
	private void UpdateBilateralDownsampleModeSwitch()
	{
		if (this.bilateralMaterial != null)
		{
			switch (this._upsampleMode)
			{
			case FogVolumeRenderer.UpsampleMode.DOWNSAMPLE_MIN:
				this.bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				return;
			case FogVolumeRenderer.UpsampleMode.DOWNSAMPLE_MAX:
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				this.bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				return;
			case FogVolumeRenderer.UpsampleMode.DOWNSAMPLE_CHESSBOARD:
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
				this.bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
				this.bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
				break;
			default:
				return;
			}
		}
	}

	// Token: 0x060000F5 RID: 245 RVA: 0x00002F1F File Offset: 0x0000111F
	private void SetUpsampleMode(FogVolumeRenderer.UpsampleMode value)
	{
		this._upsampleMode = value;
		this.UpdateBilateralDownsampleModeSwitch();
	}

	// Token: 0x060000F6 RID: 246 RVA: 0x00002F2E File Offset: 0x0000112E
	public RenderTextureReadWrite GetRTReadWrite()
	{
		if (!this.ThisCamera.allowHDR)
		{
			return RenderTextureReadWrite.Linear;
		}
		return RenderTextureReadWrite.Default;
	}

	// Token: 0x060000F7 RID: 247 RVA: 0x00002F40 File Offset: 0x00001140
	public RenderTextureFormat GetRTFormat()
	{
		if (!this.ThisCamera.allowHDR)
		{
			return RenderTextureFormat.Default;
		}
		return RenderTextureFormat.DefaultHDR;
	}

	// Token: 0x060000F8 RID: 248 RVA: 0x0001B108 File Offset: 0x00019308
	protected void GetRT(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 0, this.GetRTFormat(), this.GetRTReadWrite());
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
	}

	// Token: 0x060000F9 RID: 249 RVA: 0x00002F53 File Offset: 0x00001153
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x060000FA RID: 250 RVA: 0x00002F67 File Offset: 0x00001167
	protected void Get_RT_Depth(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 24, this.rt_DepthFormat);
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
	}

	// Token: 0x060000FB RID: 251 RVA: 0x0001B158 File Offset: 0x00019358
	private void RenderDepth()
	{
		if (this.GenerateDepth && this._FogVolumeCamera)
		{
			if (this._TAAjitter)
			{
				this._TAAjitter.patternScale = 0f;
			}
			this._FogVolumeCamera.cullingMask = this.DepthLayer2;
			if (this.ThisCamera.stereoEnabled)
			{
				Shader.EnableKeyword("FOG_VOLUME_STEREO_ON");
				if (this.ThisCamera.stereoTargetEye == StereoTargetEyeMask.Both || this.ThisCamera.stereoTargetEye == StereoTargetEyeMask.Left)
				{
					this._FogVolumeCamera.worldToCameraMatrix = this.ThisCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
					this._FogVolumeCamera.projectionMatrix = this.ThisCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
					this.Get_RT_Depth(ref this.RT_Depth, new int2(this.m_screenWidth, this.m_screenHeight), "RT_DepthLeft");
					this._FogVolumeCamera.targetTexture = this.RT_Depth;
					this._FogVolumeCamera.RenderWithShader(this.depthShader, "RenderType");
				}
				if (this.ThisCamera.stereoTargetEye == StereoTargetEyeMask.Both || this.ThisCamera.stereoTargetEye == StereoTargetEyeMask.Right)
				{
					this._FogVolumeCamera.worldToCameraMatrix = this.ThisCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
					this._FogVolumeCamera.projectionMatrix = this.ThisCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
					this.Get_RT_Depth(ref this.RT_DepthR, new int2(this.m_screenWidth, this.m_screenHeight), "RT_DepthRight");
					this._FogVolumeCamera.targetTexture = this.RT_DepthR;
					this._FogVolumeCamera.RenderWithShader(this.depthShader, "RenderType");
					Shader.SetGlobalTexture("RT_DepthR", this.RT_DepthR);
				}
			}
			else
			{
				this._FogVolumeCamera.projectionMatrix = this.ThisCamera.projectionMatrix;
				Shader.DisableKeyword("FOG_VOLUME_STEREO_ON");
				this.Get_RT_Depth(ref this.RT_Depth, new int2(this.m_screenWidth, this.m_screenHeight), "RT_Depth");
				this._FogVolumeCamera.targetTexture = this.RT_Depth;
				this._FogVolumeCamera.RenderWithShader(this.depthShader, "RenderType");
			}
			Shader.SetGlobalTexture("RT_Depth", this.RT_Depth);
		}
	}

	// Token: 0x060000FC RID: 252 RVA: 0x0001B37C File Offset: 0x0001957C
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

	// Token: 0x060000FD RID: 253 RVA: 0x0001B3B8 File Offset: 0x000195B8
	private void ReleaseLowProfileDepthRRT()
	{
		if (this.lowProfileDepthRRT != null)
		{
			for (int i = 0; i < this.lowProfileDepthRRT.Length; i++)
			{
				RenderTexture.ReleaseTemporary(this.lowProfileDepthRRT[i]);
			}
			this.lowProfileDepthRRT = null;
		}
	}

	// Token: 0x060000FE RID: 254 RVA: 0x0001B3F4 File Offset: 0x000195F4
	private void RenderColor()
	{
		if (this._TAA && this._TAAjitter)
		{
			this._TAA.enabled = this.TAA;
			this._TAAvelocity.enabled = false;
			this._TAAjitter.enabled = this.TAA;
			this._TAAjitter.patternScale = 0.2f;
		}
		this._FogVolumeCamera.cullingMask = 1 << LayerMask.NameToLayer("FogVolume");
		this._FogVolumeCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
		int2 @int = new int2(this.m_screenWidth / this._Downsample, this.m_screenHeight / this._Downsample);
		this.FogVolumeResolution = @int.x.ToString() + " X " + @int.y.ToString();
		if (this.ThisCamera.stereoEnabled)
		{
			Shader.EnableKeyword("FOG_VOLUME_STEREO_ON");
			if (this.ThisCamera.stereoTargetEye == StereoTargetEyeMask.Both || this.ThisCamera.stereoTargetEye == StereoTargetEyeMask.Left)
			{
				this._FogVolumeCamera.projectionMatrix = this.ThisCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
				this._FogVolumeCamera.worldToCameraMatrix = this.ThisCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
				this.GetRT(ref this.RT_FogVolume, @int, "RT_FogVolumeLeft");
				this._FogVolumeCamera.targetTexture = this.RT_FogVolume;
				this._FogVolumeCamera.Render();
			}
			if (this.ThisCamera.stereoTargetEye == StereoTargetEyeMask.Both || this.ThisCamera.stereoTargetEye == StereoTargetEyeMask.Right)
			{
				this._FogVolumeCamera.projectionMatrix = this.ThisCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
				this._FogVolumeCamera.worldToCameraMatrix = this.ThisCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
				this.GetRT(ref this.RT_FogVolumeR, @int, "RT_FogVolumeRight");
				this._FogVolumeCamera.targetTexture = this.RT_FogVolumeR;
				this._FogVolumeCamera.Render();
			}
		}
		else
		{
			Shader.DisableKeyword("FOG_VOLUME_STEREO_ON");
			this._FogVolumeCamera.projectionMatrix = this.ThisCamera.projectionMatrix;
			this.GetRT(ref this.RT_FogVolume, @int, "RT_FogVolume");
			this._FogVolumeCamera.targetTexture = this.RT_FogVolume;
			this._FogVolumeCamera.Render();
		}
		if (this.TAA)
		{
			this._TAA.TAA(ref this.RT_FogVolume);
		}
		if (this.ThisCamera.stereoEnabled && this.TAA)
		{
			this._TAA.TAA(ref this.RT_FogVolumeR);
		}
		if (this.useBilateralUpsampling && this.GenerateDepth)
		{
			if (this.bilateralMaterial)
			{
				this.bilateralMaterial.SetInt("RightSide", 0);
				this.ReleaseLowProfileDepthRT();
				this.lowProfileDepthRT = new RenderTexture[this._Downsample];
				for (int i = 0; i < this._Downsample; i++)
				{
					int num = this.m_screenWidth / (i + 1);
					int num2 = this.m_screenHeight / (i + 1);
					int num3 = this.m_screenWidth / Mathf.Max(i, 1);
					int num4 = this.m_screenHeight / Mathf.Max(i, 1);
					Vector4 vector = new Vector4(1f / (float)num3, 1f / (float)num4, 0f, 0f);
					this.bilateralMaterial.SetFloat("_UpsampleDepthThreshold", this.upsampleDepthThreshold);
					this.bilateralMaterial.SetVector("_TexelSize", vector);
					this.bilateralMaterial.SetTexture("_HiResDepthBuffer", this.RT_Depth);
					this.lowProfileDepthRT[i] = RenderTexture.GetTemporary(num, num2, 0, this.rt_DepthFormat, this.GetRTReadWrite());
					this.lowProfileDepthRT[i].name = "lowProfileDepthRT_" + i.ToString();
					Graphics.Blit(null, this.lowProfileDepthRT[i], this.bilateralMaterial, 0);
				}
				Shader.SetGlobalTexture("RT_Depth", this.lowProfileDepthRT[this.lowProfileDepthRT.Length - 1]);
			}
			if (this.bilateralMaterial)
			{
				for (int j = this._Downsample - 1; j >= 0; j--)
				{
					int num5 = this.m_screenWidth / Mathf.Max(j, 1);
					int num6 = this.m_screenHeight / Mathf.Max(j, 1);
					int num7 = this.m_screenWidth / (j + 1);
					int num8 = this.m_screenHeight / (j + 1);
					Vector4 vector2 = new Vector4(1f / (float)num7, 1f / (float)num8, 0f, 0f);
					this.bilateralMaterial.SetVector("_TexelSize", vector2);
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
			if (this.ThisCamera.stereoEnabled)
			{
				if (this.bilateralMaterial)
				{
					this.bilateralMaterial.EnableKeyword("FOG_VOLUME_STEREO_ON");
					this.bilateralMaterial.SetInt("RightSide", 1);
					this.ReleaseLowProfileDepthRRT();
					this.lowProfileDepthRRT = new RenderTexture[this._Downsample];
					for (int k = 0; k < this._Downsample; k++)
					{
						int num9 = this.m_screenWidth / (k + 1);
						int num10 = this.m_screenHeight / (k + 1);
						int num11 = this.m_screenWidth / Mathf.Max(k, 1);
						int num12 = this.m_screenHeight / Mathf.Max(k, 1);
						Vector4 vector3 = new Vector4(1f / (float)num11, 1f / (float)num12, 0f, 0f);
						this.bilateralMaterial.SetFloat("_UpsampleDepthThreshold", this.upsampleDepthThreshold);
						this.bilateralMaterial.SetVector("_TexelSize", vector3);
						this.bilateralMaterial.SetTexture("_HiResDepthBufferR", this.RT_DepthR);
						this.lowProfileDepthRRT[k] = RenderTexture.GetTemporary(num9, num10, 0, this.rt_DepthFormat, this.GetRTReadWrite());
						this.lowProfileDepthRRT[k].name = "lowProfileDepthRRT_" + k.ToString();
						Graphics.Blit(null, this.lowProfileDepthRRT[k], this.bilateralMaterial, 0);
					}
					Shader.SetGlobalTexture("RT_DepthR", this.lowProfileDepthRRT[this.lowProfileDepthRRT.Length - 1]);
				}
				if (this.bilateralMaterial)
				{
					for (int l = this._Downsample - 1; l >= 0; l--)
					{
						int num13 = this.m_screenWidth / Mathf.Max(l, 1);
						int num14 = this.m_screenHeight / Mathf.Max(l, 1);
						int num15 = this.m_screenWidth / (l + 1);
						int num16 = this.m_screenHeight / (l + 1);
						Vector4 vector4 = new Vector4(1f / (float)num15, 1f / (float)num16, 0f, 0f);
						this.bilateralMaterial.SetVector("_TexelSize", vector4);
						this.bilateralMaterial.SetVector("_InvdUV", new Vector4((float)this.RT_FogVolumeR.width, (float)this.RT_FogVolumeR.height, 0f, 0f));
						this.bilateralMaterial.SetTexture("_HiResDepthBufferR", this.RT_DepthR);
						this.bilateralMaterial.SetTexture("_LowResDepthBufferR", this.lowProfileDepthRRT[l]);
						this.bilateralMaterial.SetTexture("_LowResColorR", this.RT_FogVolumeR);
						RenderTexture temporary2 = RenderTexture.GetTemporary(num13, num14, 0, this.GetRTFormat(), this.GetRTReadWrite());
						temporary2.filterMode = FilterMode.Bilinear;
						Graphics.Blit(null, temporary2, this.bilateralMaterial, 1);
						RenderTexture rt_FogVolumeR = this.RT_FogVolumeR;
						this.RT_FogVolumeR = temporary2;
						RenderTexture.ReleaseTemporary(rt_FogVolumeR);
					}
				}
				this.ReleaseLowProfileDepthRRT();
			}
			else
			{
				this.bilateralMaterial.DisableKeyword("FOG_VOLUME_STEREO_ON");
			}
		}
		if (this.ThisCamera.stereoEnabled)
		{
			Shader.SetGlobalTexture("RT_FogVolumeR", this.RT_FogVolumeR);
		}
		Shader.SetGlobalTexture("RT_FogVolume", this.RT_FogVolume);
	}

	// Token: 0x060000FF RID: 255 RVA: 0x0001BC5C File Offset: 0x00019E5C
	private void CameraUpdateSharedProperties()
	{
		if (this._FogVolumeCamera)
		{
			this._FogVolumeCamera.farClipPlane = this.ThisCamera.farClipPlane;
			this._FogVolumeCamera.nearClipPlane = this.ThisCamera.nearClipPlane;
			this._FogVolumeCamera.allowHDR = this.ThisCamera.allowHDR;
			if (!this._FogVolumeCamera.stereoEnabled)
			{
				this._FogVolumeCamera.fieldOfView = this.ThisCamera.fieldOfView;
				this._FogVolumeCamera.projectionMatrix = this.ThisCamera.projectionMatrix;
			}
		}
	}

	// Token: 0x06000100 RID: 256 RVA: 0x0001BCF4 File Offset: 0x00019EF4
	private void TAASetup()
	{
		if (this._Downsample > 1 && this.TAA)
		{
			if (this._FogVolumeCameraGO.GetComponent<FogVolumeTAA>() == null)
			{
				this._FogVolumeCameraGO.AddComponent<FogVolumeTAA>();
			}
			this._TAA = this._FogVolumeCameraGO.GetComponent<FogVolumeTAA>();
			this._TAAvelocity = this._FogVolumeCameraGO.GetComponent<VelocityBuffer>();
			this._TAAjitter = this._FogVolumeCameraGO.GetComponent<FrustumJitter>();
		}
	}

	// Token: 0x06000101 RID: 257 RVA: 0x0001BD64 File Offset: 0x00019F64
	private void CreateFogCamera()
	{
		this._FogVolumeCameraGO = new GameObject();
		this._FogVolumeCameraGO.transform.parent = base.gameObject.transform;
		this._FogVolumeCameraGO.transform.localEulerAngles = Vector3.zero;
		this._FogVolumeCameraGO.transform.localPosition = Vector3.zero;
		this._FogVolumeCameraGO.name = "FogVolumeCamera";
		this._FogVolumeCamera = this._FogVolumeCameraGO.AddComponent<Camera>();
		this._FogVolumeCamera.depth = -666f;
		this._FogVolumeCamera.clearFlags = CameraClearFlags.Color;
		this._FogVolumeCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		this._FogVolumeCamera.enabled = false;
		this._FogVolumeCamera.renderingPath = RenderingPath.Forward;
		this._FogVolumeCamera.allowMSAA = false;
	}

	// Token: 0x06000102 RID: 258 RVA: 0x0001BE48 File Offset: 0x0001A048
	private void FindFogCamera()
	{
		this._FogVolumeCameraGO = GameObject.Find("FogVolumeCamera");
		if (this._FogVolumeCameraGO)
		{
			this._FogVolumeCamera = this._FogVolumeCameraGO.GetComponent<Camera>();
		}
		if (this._FogVolumeCameraGO == null)
		{
			this.CreateFogCamera();
		}
		this.TAASetup();
	}

	// Token: 0x06000103 RID: 259 RVA: 0x0001BEA0 File Offset: 0x0001A0A0
	private void TexelUpdate()
	{
		this.TexelSize.x = 1f / (float)this.ThisCamera.pixelWidth;
		this.TexelSize.y = 1f / (float)this.ThisCamera.pixelHeight;
		this.TexelSize.z = (float)this.ThisCamera.pixelWidth;
		this.TexelSize.w = (float)this.ThisCamera.pixelHeight;
		Shader.SetGlobalVector("RT_FogVolume_TexelSize", this.TexelSize);
	}

	// Token: 0x06000104 RID: 260 RVA: 0x0001BF28 File Offset: 0x0001A128
	private void OnEnable()
	{
		this.SetUseBilateralUpsampling(this._useBilateralUpsampling);
		this.SetUpsampleMode(this._upsampleMode);
		this.ShowBilateralEdge(this._showBilateralEdge);
		this.ThisCamera = base.gameObject.GetComponent<Camera>();
		this.depthShader = Shader.Find("Hidden/Fog Volume/Depth");
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
		{
			this.rt_DepthFormat = RenderTextureFormat.RFloat;
		}
		else
		{
			this.rt_DepthFormat = RenderTextureFormat.DefaultHDR;
		}
		if (this.depthShader == null)
		{
			MonoBehaviour.print("Hidden/Fog Volume/Depth #SHADER ERROR#");
		}
		this.FindFogCamera();
		Component[] components = this._FogVolumeCameraGO.GetComponents<Component>();
		if (this._FogVolumeCamera.GetComponent("FogVolumeCamera"))
		{
			MonoBehaviour.print("Destroyed Old Camera");
			this.SafeDestroy(this._FogVolumeCameraGO);
			this.CreateFogCamera();
		}
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] == null)
			{
				MonoBehaviour.print("Destroyed Old Camera");
				this.SafeDestroy(this._FogVolumeCameraGO);
				this.CreateFogCamera();
				break;
			}
		}
		if (base.GetComponent<FogVolume>())
		{
			MonoBehaviour.print("Don't add FogVolume here. Create a new one using the menu buttons and follow the instructions");
			global::UnityEngine.Object.DestroyImmediate(base.GetComponent<FogVolume>());
		}
		if (this.ThisCamera.GetComponent<MeshFilter>())
		{
			global::UnityEngine.Object.DestroyImmediate(this.ThisCamera.GetComponent<MeshFilter>());
		}
		if (this.ThisCamera.GetComponent<MeshRenderer>())
		{
			global::UnityEngine.Object.DestroyImmediate(this.ThisCamera.GetComponent<MeshRenderer>());
		}
		this.SurrogateMaterial = (Material)Resources.Load("Fog Volume Surrogate");
		if (this.DepthLayer2 == 0)
		{
			this.DepthLayer2 = 1;
		}
	}

	// Token: 0x06000105 RID: 261 RVA: 0x0001C0B4 File Offset: 0x0001A2B4
	private void UpdateParams()
	{
		if (this.useBilateralUpsampling)
		{
			bool generateDepth = this.GenerateDepth;
		}
		if (this.GenerateDepth)
		{
			this.SurrogateMaterial.SetInt("_ztest", 8);
			this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolume"));
			this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
			this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
			this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
			this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("UI"));
		}
		else
		{
			this.SurrogateMaterial.SetInt("_ztest", 4);
		}
		if (!this._TAA)
		{
			this.TAASetup();
		}
		this.HDR = this.ThisCamera.allowHDR;
	}

	// Token: 0x06000106 RID: 262 RVA: 0x0001C1B4 File Offset: 0x0001A3B4
	private void OnPreRender()
	{
		this.m_screenWidth = this.ThisCamera.pixelWidth;
		this.m_screenHeight = this.ThisCamera.pixelHeight;
		if (this.ThisCamera == null)
		{
			this.ThisCamera = base.gameObject.GetComponent<Camera>();
			Debug.Log("No camera found");
		}
		if (this._FogVolumeCamera == null && this._Downsample > 1)
		{
			this.FindFogCamera();
		}
		if (this._Downsample == 1)
		{
			this.SafeDestroy(this._FogVolumeCameraGO);
		}
		this.CameraUpdateSharedProperties();
		if (this._Downsample > 1 && this._FogVolumeCameraGO && base.isActiveAndEnabled)
		{
			this.ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));
			this.ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
			this.FogVolumeResolution = this.m_screenWidth.ToString() + " X " + this.m_screenHeight.ToString();
			this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeSurrogate");
		}
		else
		{
			this.ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
			this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolume");
			this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
		}
		int pixelLightCount = QualitySettings.pixelLightCount;
		ShadowQuality shadows = QualitySettings.shadows;
		QualitySettings.pixelLightCount = 0;
		QualitySettings.shadows = ShadowQuality.Disable;
		if (this.ThisCamera == null)
		{
			this.ThisCamera = base.gameObject.GetComponent<Camera>();
		}
		this.UpdateParams();
		this.SurrogateMaterial.SetInt("_SrcBlend", (int)this._BlendMode);
		if (this._Downsample > 1 && this._FogVolumeCamera)
		{
			Shader.EnableKeyword("_FOG_LOWRES_RENDERER");
			this.RenderDepth();
			this.RenderColor();
			Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		}
		else
		{
			Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		}
		QualitySettings.pixelLightCount = pixelLightCount;
		QualitySettings.shadows = shadows;
	}

	// Token: 0x06000107 RID: 263 RVA: 0x00002FA4 File Offset: 0x000011A4
	private void Update()
	{
		this.ShowCamerasBack = this.ShowCamera;
	}

	// Token: 0x06000108 RID: 264 RVA: 0x00002FB2 File Offset: 0x000011B2
	private void SafeDestroy(global::UnityEngine.Object obj)
	{
		if (obj != null)
		{
			obj = null;
			global::UnityEngine.Object.DestroyImmediate(obj);
		}
		obj = null;
	}

	// Token: 0x06000109 RID: 265 RVA: 0x0001C3FC File Offset: 0x0001A5FC
	private void OnDisable()
	{
		Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolume");
		this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
		this.ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
		this.SafeDestroy(this._TAA);
		this.SafeDestroy(this._TAAvelocity);
		this.SafeDestroy(this._TAAjitter);
		this.SafeDestroy(this._FogVolumeCameraGO);
		this.SafeDestroy(this.RT_FogVolume);
		this.SafeDestroy(this.RT_FogVolumeR);
		this.SafeDestroy(this.RT_Depth);
		this.SafeDestroy(this.RT_DepthR);
	}

	// Token: 0x0400019A RID: 410
	private bool ShowCamerasBack = true;

	// Token: 0x0400019B RID: 411
	public bool ShowCamera;

	// Token: 0x0400019C RID: 412
	private int m_screenWidth;

	// Token: 0x0400019D RID: 413
	private int m_screenHeight;

	// Token: 0x0400019E RID: 414
	public string FogVolumeResolution;

	// Token: 0x0400019F RID: 415
	private RenderTextureFormat rt_DepthFormat;

	// Token: 0x040001A0 RID: 416
	public FogVolumeRenderer.BlendMode _BlendMode = FogVolumeRenderer.BlendMode.PremultipliedTransparency;

	// Token: 0x040001A1 RID: 417
	public bool GenerateDepth = true;

	// Token: 0x040001A2 RID: 418
	private RenderTexture RT_FogVolume;

	// Token: 0x040001A3 RID: 419
	private RenderTexture RT_FogVolumeR;

	// Token: 0x040001A4 RID: 420
	[SerializeField]
	[Range(0f, 8f)]
	public int _Downsample = 1;

	// Token: 0x040001A5 RID: 421
	public bool _showBilateralEdge;

	// Token: 0x040001A6 RID: 422
	private Material bilateralMaterial;

	// Token: 0x040001A7 RID: 423
	public bool _useBilateralUpsampling = true;

	// Token: 0x040001A8 RID: 424
	public FogVolumeRenderer.UpsampleMode _upsampleMode = FogVolumeRenderer.UpsampleMode.DOWNSAMPLE_MAX;

	// Token: 0x040001A9 RID: 425
	private Camera ThisCamera;

	// Token: 0x040001AA RID: 426
	private RenderTexture RT_Depth;

	// Token: 0x040001AB RID: 427
	private RenderTexture RT_DepthR;

	// Token: 0x040001AC RID: 428
	private Shader depthShader;

	// Token: 0x040001AD RID: 429
	[HideInInspector]
	private Camera _FogVolumeCamera;

	// Token: 0x040001AE RID: 430
	private GameObject _FogVolumeCameraGO;

	// Token: 0x040001AF RID: 431
	[SerializeField]
	[Range(0f, 0.01f)]
	public float upsampleDepthThreshold = 0.00187f;

	// Token: 0x040001B0 RID: 432
	public bool HDR;

	// Token: 0x040001B1 RID: 433
	public bool TAA;

	// Token: 0x040001B2 RID: 434
	public FogVolumeTAA _TAA;

	// Token: 0x040001B3 RID: 435
	private VelocityBuffer _TAAvelocity;

	// Token: 0x040001B4 RID: 436
	private FrustumJitter _TAAjitter;

	// Token: 0x040001B5 RID: 437
	[SerializeField]
	public int DepthLayer2;

	// Token: 0x040001B6 RID: 438
	public RenderTexture[] lowProfileDepthRT;

	// Token: 0x040001B7 RID: 439
	public RenderTexture[] lowProfileDepthRRT;

	// Token: 0x040001B8 RID: 440
	private Vector4 TexelSize = Vector4.zero;

	// Token: 0x040001B9 RID: 441
	private Material SurrogateMaterial;

	// Token: 0x040001BA RID: 442
	public bool SceneBlur = true;

	// Token: 0x02000025 RID: 37
	public enum BlendMode
	{
		// Token: 0x040001BC RID: 444
		PremultipliedTransparency = 1,
		// Token: 0x040001BD RID: 445
		TraditionalTransparency = 5
	}

	// Token: 0x02000026 RID: 38
	[SerializeField]
	[Serializable]
	public enum UpsampleMode
	{
		// Token: 0x040001BF RID: 447
		DOWNSAMPLE_MIN,
		// Token: 0x040001C0 RID: 448
		DOWNSAMPLE_MAX,
		// Token: 0x040001C1 RID: 449
		DOWNSAMPLE_CHESSBOARD
	}

	// Token: 0x02000027 RID: 39
	public enum UpsampleMaterialPass
	{
		// Token: 0x040001C3 RID: 451
		DEPTH_DOWNSAMPLE,
		// Token: 0x040001C4 RID: 452
		BILATERAL_UPSAMPLE
	}
}
