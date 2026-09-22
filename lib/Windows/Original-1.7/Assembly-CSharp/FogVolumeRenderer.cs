using System;
using FogVolumePlaydeadTAA;
using FogVolumeUtilities;
using UnityEngine;

// Token: 0x02000016 RID: 22
[ExecuteInEditMode]
public class FogVolumeRenderer : MonoBehaviour
{
	// Token: 0x060000C9 RID: 201 RVA: 0x000095CE File Offset: 0x000077CE
	public void setDownsample(int val)
	{
		this._Downsample = val;
	}

	// Token: 0x1700001D RID: 29
	// (get) Token: 0x060000CB RID: 203 RVA: 0x000095E9 File Offset: 0x000077E9
	// (set) Token: 0x060000CA RID: 202 RVA: 0x000095D7 File Offset: 0x000077D7
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

	// Token: 0x060000CC RID: 204 RVA: 0x000095F1 File Offset: 0x000077F1
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

	// Token: 0x1700001E RID: 30
	// (get) Token: 0x060000CD RID: 205 RVA: 0x00009630 File Offset: 0x00007830
	// (set) Token: 0x060000CE RID: 206 RVA: 0x00009638 File Offset: 0x00007838
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

	// Token: 0x060000CF RID: 207 RVA: 0x0000964C File Offset: 0x0000784C
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

	// Token: 0x1700001F RID: 31
	// (get) Token: 0x060000D1 RID: 209 RVA: 0x000096CF File Offset: 0x000078CF
	// (set) Token: 0x060000D0 RID: 208 RVA: 0x000096BD File Offset: 0x000078BD
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

	// Token: 0x060000D2 RID: 210 RVA: 0x000096D8 File Offset: 0x000078D8
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

	// Token: 0x060000D3 RID: 211 RVA: 0x000097A2 File Offset: 0x000079A2
	private void SetUpsampleMode(FogVolumeRenderer.UpsampleMode value)
	{
		this._upsampleMode = value;
		this.UpdateBilateralDownsampleModeSwitch();
	}

	// Token: 0x060000D4 RID: 212 RVA: 0x000097B1 File Offset: 0x000079B1
	public RenderTextureReadWrite GetRTReadWrite()
	{
		if (!this.ThisCamera.allowHDR)
		{
			return RenderTextureReadWrite.Linear;
		}
		return RenderTextureReadWrite.Default;
	}

	// Token: 0x060000D5 RID: 213 RVA: 0x000097C3 File Offset: 0x000079C3
	public RenderTextureFormat GetRTFormat()
	{
		if (!this.ThisCamera.allowHDR)
		{
			return RenderTextureFormat.Default;
		}
		return RenderTextureFormat.DefaultHDR;
	}

	// Token: 0x060000D6 RID: 214 RVA: 0x000097D8 File Offset: 0x000079D8
	protected void GetRT(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 0, this.GetRTFormat(), this.GetRTReadWrite());
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
	}

	// Token: 0x060000D7 RID: 215 RVA: 0x00009825 File Offset: 0x00007A25
	public void ReleaseRT(RenderTexture rt)
	{
		if (rt != null)
		{
			RenderTexture.ReleaseTemporary(rt);
			rt = null;
		}
	}

	// Token: 0x060000D8 RID: 216 RVA: 0x00009839 File Offset: 0x00007A39
	protected void Get_RT_Depth(ref RenderTexture rt, int2 size, string name)
	{
		this.ReleaseRT(rt);
		rt = RenderTexture.GetTemporary(size.x, size.y, 24, this.rt_DepthFormat);
		rt.filterMode = FilterMode.Bilinear;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
	}

	// Token: 0x060000D9 RID: 217 RVA: 0x00009878 File Offset: 0x00007A78
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

	// Token: 0x060000DA RID: 218 RVA: 0x00009A9C File Offset: 0x00007C9C
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

	// Token: 0x060000DB RID: 219 RVA: 0x00009AD8 File Offset: 0x00007CD8
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

	// Token: 0x060000DC RID: 220 RVA: 0x00009B14 File Offset: 0x00007D14
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

	// Token: 0x060000DD RID: 221 RVA: 0x0000A37C File Offset: 0x0000857C
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

	// Token: 0x060000DE RID: 222 RVA: 0x0000A414 File Offset: 0x00008614
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

	// Token: 0x060000DF RID: 223 RVA: 0x0000A484 File Offset: 0x00008684
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

	// Token: 0x060000E0 RID: 224 RVA: 0x0000A568 File Offset: 0x00008768
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

	// Token: 0x060000E1 RID: 225 RVA: 0x0000A5C0 File Offset: 0x000087C0
	private void TexelUpdate()
	{
		this.TexelSize.x = 1f / (float)this.ThisCamera.pixelWidth;
		this.TexelSize.y = 1f / (float)this.ThisCamera.pixelHeight;
		this.TexelSize.z = (float)this.ThisCamera.pixelWidth;
		this.TexelSize.w = (float)this.ThisCamera.pixelHeight;
		Shader.SetGlobalVector("RT_FogVolume_TexelSize", this.TexelSize);
	}

	// Token: 0x060000E2 RID: 226 RVA: 0x0000A648 File Offset: 0x00008848
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
			Object.DestroyImmediate(base.GetComponent<FogVolume>());
		}
		if (this.ThisCamera.GetComponent<MeshFilter>())
		{
			Object.DestroyImmediate(this.ThisCamera.GetComponent<MeshFilter>());
		}
		if (this.ThisCamera.GetComponent<MeshRenderer>())
		{
			Object.DestroyImmediate(this.ThisCamera.GetComponent<MeshRenderer>());
		}
		this.SurrogateMaterial = (Material)Resources.Load("Fog Volume Surrogate");
		if (this.DepthLayer2 == 0)
		{
			this.DepthLayer2 = 1;
		}
	}

	// Token: 0x060000E3 RID: 227 RVA: 0x0000A7D4 File Offset: 0x000089D4
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

	// Token: 0x060000E4 RID: 228 RVA: 0x0000A8D4 File Offset: 0x00008AD4
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

	// Token: 0x060000E5 RID: 229 RVA: 0x0000AB1B File Offset: 0x00008D1B
	private void Update()
	{
		this.ShowCamerasBack = this.ShowCamera;
	}

	// Token: 0x060000E6 RID: 230 RVA: 0x0000AB29 File Offset: 0x00008D29
	private void SafeDestroy(Object obj)
	{
		if (obj != null)
		{
			obj = null;
			Object.DestroyImmediate(obj);
		}
		obj = null;
	}

	// Token: 0x060000E7 RID: 231 RVA: 0x0000AB40 File Offset: 0x00008D40
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

	// Token: 0x0400015E RID: 350
	private bool ShowCamerasBack = true;

	// Token: 0x0400015F RID: 351
	public bool ShowCamera;

	// Token: 0x04000160 RID: 352
	private int m_screenWidth;

	// Token: 0x04000161 RID: 353
	private int m_screenHeight;

	// Token: 0x04000162 RID: 354
	public string FogVolumeResolution;

	// Token: 0x04000163 RID: 355
	private RenderTextureFormat rt_DepthFormat;

	// Token: 0x04000164 RID: 356
	public FogVolumeRenderer.BlendMode _BlendMode = FogVolumeRenderer.BlendMode.PremultipliedTransparency;

	// Token: 0x04000165 RID: 357
	public bool GenerateDepth = true;

	// Token: 0x04000166 RID: 358
	private RenderTexture RT_FogVolume;

	// Token: 0x04000167 RID: 359
	private RenderTexture RT_FogVolumeR;

	// Token: 0x04000168 RID: 360
	[SerializeField]
	[Range(0f, 8f)]
	public int _Downsample = 1;

	// Token: 0x04000169 RID: 361
	public bool _showBilateralEdge;

	// Token: 0x0400016A RID: 362
	private Material bilateralMaterial;

	// Token: 0x0400016B RID: 363
	public bool _useBilateralUpsampling = true;

	// Token: 0x0400016C RID: 364
	public FogVolumeRenderer.UpsampleMode _upsampleMode = FogVolumeRenderer.UpsampleMode.DOWNSAMPLE_MAX;

	// Token: 0x0400016D RID: 365
	private Camera ThisCamera;

	// Token: 0x0400016E RID: 366
	private RenderTexture RT_Depth;

	// Token: 0x0400016F RID: 367
	private RenderTexture RT_DepthR;

	// Token: 0x04000170 RID: 368
	private Shader depthShader;

	// Token: 0x04000171 RID: 369
	[HideInInspector]
	private Camera _FogVolumeCamera;

	// Token: 0x04000172 RID: 370
	private GameObject _FogVolumeCameraGO;

	// Token: 0x04000173 RID: 371
	[SerializeField]
	[Range(0f, 0.01f)]
	public float upsampleDepthThreshold = 0.00187f;

	// Token: 0x04000174 RID: 372
	public bool HDR;

	// Token: 0x04000175 RID: 373
	public bool TAA;

	// Token: 0x04000176 RID: 374
	public FogVolumeTAA _TAA;

	// Token: 0x04000177 RID: 375
	private VelocityBuffer _TAAvelocity;

	// Token: 0x04000178 RID: 376
	private FrustumJitter _TAAjitter;

	// Token: 0x04000179 RID: 377
	[SerializeField]
	public int DepthLayer2;

	// Token: 0x0400017A RID: 378
	public RenderTexture[] lowProfileDepthRT;

	// Token: 0x0400017B RID: 379
	public RenderTexture[] lowProfileDepthRRT;

	// Token: 0x0400017C RID: 380
	private Vector4 TexelSize = Vector4.zero;

	// Token: 0x0400017D RID: 381
	private Material SurrogateMaterial;

	// Token: 0x0400017E RID: 382
	public bool SceneBlur = true;

	// Token: 0x02000228 RID: 552
	public enum BlendMode
	{
		// Token: 0x04000E0E RID: 3598
		PremultipliedTransparency = 1,
		// Token: 0x04000E0F RID: 3599
		TraditionalTransparency = 5
	}

	// Token: 0x02000229 RID: 553
	[SerializeField]
	[Serializable]
	public enum UpsampleMode
	{
		// Token: 0x04000E11 RID: 3601
		DOWNSAMPLE_MIN,
		// Token: 0x04000E12 RID: 3602
		DOWNSAMPLE_MAX,
		// Token: 0x04000E13 RID: 3603
		DOWNSAMPLE_CHESSBOARD
	}

	// Token: 0x0200022A RID: 554
	public enum UpsampleMaterialPass
	{
		// Token: 0x04000E15 RID: 3605
		DEPTH_DOWNSAMPLE,
		// Token: 0x04000E16 RID: 3606
		BILATERAL_UPSAMPLE
	}
}
