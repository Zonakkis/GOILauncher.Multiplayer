using System;
using FogVolumePlaydeadTAA;
using UnityEngine;

// Token: 0x0200001B RID: 27
[ExecuteInEditMode]
public class FogVolumeRenderer : MonoBehaviour
{
	// Token: 0x060000CA RID: 202 RVA: 0x00009CD0 File Offset: 0x000080D0
	public void setDownsample(int val)
	{
		this._Downsample = val;
	}

	// Token: 0x060000CB RID: 203 RVA: 0x00009CDC File Offset: 0x000080DC
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
		}
	}

	// Token: 0x060000CC RID: 204 RVA: 0x00009D44 File Offset: 0x00008144
	private void CreateFogCamera()
	{
		if (this._Downsample > 1)
		{
			this._FogVolumeCameraGO = new GameObject();
			this._FogVolumeCameraGO.name = "FogVolumeCamera";
			this._FogVolumeCamera = this._FogVolumeCameraGO.AddComponent<FogVolumeCamera>();
			this._FogVolumeCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.Color;
			this._FogVolumeCamera.GetComponent<Camera>().backgroundColor = new Color(0f, 0f, 0f, 0f);
			this._FogVolumeCameraGO.hideFlags = HideFlags.None;
			this._FogVolumeCamera.GetComponent<Camera>().renderingPath = RenderingPath.Forward;
			this._FogVolumeCamera.GetComponent<Camera>().allowMSAA = false;
		}
	}

	// Token: 0x060000CD RID: 205 RVA: 0x00009DF1 File Offset: 0x000081F1
	private void FindFogCamera()
	{
		this._FogVolumeCameraGO = GameObject.Find("FogVolumeCamera");
		if (this._FogVolumeCameraGO != null)
		{
			this._FogVolumeCameraGO.SetActive(true);
		}
		else
		{
			this.CreateFogCamera();
		}
	}

	// Token: 0x060000CE RID: 206 RVA: 0x00009E2C File Offset: 0x0000822C
	private void TexelUpdate()
	{
		if (this._FogVolumeCamera.RT_FogVolume)
		{
			this.TexelSize.x = 1f / (float)this._FogVolumeCamera.RT_FogVolume.width;
			this.TexelSize.y = 1f / (float)this._FogVolumeCamera.RT_FogVolume.height;
			this.TexelSize.z = (float)this._FogVolumeCamera.RT_FogVolume.width;
			this.TexelSize.w = (float)this._FogVolumeCamera.RT_FogVolume.height;
			Shader.SetGlobalVector("RT_FogVolume_TexelSize", this.TexelSize);
		}
	}

	// Token: 0x060000CF RID: 207 RVA: 0x00009EDC File Offset: 0x000082DC
	private void OnEnable()
	{
		this.ThisCamera = base.gameObject.GetComponent<Camera>();
		this.FindFogCamera();
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
		this.UpdateParams();
		if (this.DepthLayer2 == 0)
		{
			this.DepthLayer2 = 1;
		}
	}

	// Token: 0x060000D0 RID: 208 RVA: 0x00009F9C File Offset: 0x0000839C
	private void UpdateParams()
	{
		if (this._FogVolumeCamera && this._Downsample > 1)
		{
			this._FogVolumeCamera.useBilateralUpsampling = this.BilateralUpsampling;
			if (this.BilateralUpsampling && this.GenerateDepth)
			{
				this._FogVolumeCamera.upsampleMode = this.USMode;
				this._FogVolumeCamera.showBilateralEdge = this.ShowBilateralEdge;
				this._FogVolumeCamera.upsampleDepthThreshold = this.upsampleDepthThreshold;
			}
			if (this.GenerateDepth)
			{
				this.SurrogateMaterial.SetInt("_ztest", 8);
				this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolume"));
				this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
				this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
				this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
				this.DepthLayer2 &= ~(1 << LayerMask.NameToLayer("UI"));
				this._FogVolumeCamera.DepthMask = this.DepthLayer2;
			}
			else
			{
				this.SurrogateMaterial.SetInt("_ztest", 4);
			}
			if (!this._TAA)
			{
				this.TAASetup();
			}
			if (this._TAA && this._TAA.enabled != this.TAA)
			{
				this._TAA.enabled = this.TAA;
				this._TAAvelocity.enabled = this.TAA;
			}
			this.HDR = this.ThisCamera.allowHDR;
		}
	}

	// Token: 0x060000D1 RID: 209 RVA: 0x0000A168 File Offset: 0x00008568
	private void OnPreRender()
	{
		int pixelLightCount = QualitySettings.pixelLightCount;
		ShadowQuality shadows = QualitySettings.shadows;
		QualitySettings.pixelLightCount = 0;
		QualitySettings.shadows = ShadowQuality.Disable;
		if (this._Downsample > 1 && this._FogVolumeCamera)
		{
			this.SurrogateMaterial.SetInt("_SrcBlend", (int)this._BlendMode);
			Shader.EnableKeyword("_FOG_LOWRES_RENDERER");
			this._FogVolumeCamera.Render();
			Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		}
		else
		{
			Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		}
		QualitySettings.pixelLightCount = pixelLightCount;
		QualitySettings.shadows = shadows;
	}

	// Token: 0x060000D2 RID: 210 RVA: 0x0000A1FC File Offset: 0x000085FC
	private void Update()
	{
		if (this._FogVolumeCamera == null && this._Downsample > 1)
		{
			this.FindFogCamera();
		}
		if (this._Downsample > 1 && this._FogVolumeCameraGO && base.isActiveAndEnabled)
		{
			this.ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));
			this.ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
			this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeSurrogate");
		}
		else
		{
			this.ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
			this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolume");
			this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
			if (Screen.width != this.m_screenWidth || Screen.height != this.m_screenHeight)
			{
				this.m_screenWidth = Screen.width;
				this.m_screenHeight = Screen.height;
			}
		}
		if (this._FogVolumeCameraGO)
		{
			this._FogVolumeCamera._Downsample = this._Downsample;
		}
		else
		{
			this.CreateFogCamera();
		}
	}

	// Token: 0x060000D3 RID: 211 RVA: 0x0000A383 File Offset: 0x00008783
	private void DestroyFogCamera()
	{
		if (this._FogVolumeCameraGO)
		{
			global::UnityEngine.Object.DestroyImmediate(this._FogVolumeCameraGO);
		}
	}

	// Token: 0x060000D4 RID: 212 RVA: 0x0000A3A0 File Offset: 0x000087A0
	private void OnDisable()
	{
		Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
		this.DestroyFogCamera();
		this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolume");
		this.ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
		this.ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
	}

	// Token: 0x04000175 RID: 373
	public string FogVolumeResolution;

	// Token: 0x04000176 RID: 374
	public FogVolumeRenderer.BlendMode _BlendMode = FogVolumeRenderer.BlendMode.PremultipliedTransparency;

	// Token: 0x04000177 RID: 375
	public bool GenerateDepth = true;

	// Token: 0x04000178 RID: 376
	[SerializeField]
	[Range(0f, 8f)]
	public int _Downsample = 1;

	// Token: 0x04000179 RID: 377
	public bool BilateralUpsampling;

	// Token: 0x0400017A RID: 378
	public bool ShowBilateralEdge;

	// Token: 0x0400017B RID: 379
	[SerializeField]
	public FogVolumeCamera.UpsampleMode USMode = FogVolumeCamera.UpsampleMode.DOWNSAMPLE_CHESSBOARD;

	// Token: 0x0400017C RID: 380
	public Camera ThisCamera;

	// Token: 0x0400017D RID: 381
	[HideInInspector]
	public FogVolumeCamera _FogVolumeCamera;

	// Token: 0x0400017E RID: 382
	private GameObject _FogVolumeCameraGO;

	// Token: 0x0400017F RID: 383
	[SerializeField]
	[Range(0f, 0.01f)]
	public float upsampleDepthThreshold = 0.00187f;

	// Token: 0x04000180 RID: 384
	public bool HDR;

	// Token: 0x04000181 RID: 385
	public bool TAA;

	// Token: 0x04000182 RID: 386
	public FogVolumeTAA _TAA;

	// Token: 0x04000183 RID: 387
	private VelocityBuffer _TAAvelocity;

	// Token: 0x04000184 RID: 388
	[SerializeField]
	public int DepthLayer2;

	// Token: 0x04000185 RID: 389
	private Vector4 TexelSize = Vector4.zero;

	// Token: 0x04000186 RID: 390
	private Material SurrogateMaterial;

	// Token: 0x04000187 RID: 391
	public bool SceneBlur = true;

	// Token: 0x04000188 RID: 392
	private int m_screenWidth;

	// Token: 0x04000189 RID: 393
	private int m_screenHeight;

	// Token: 0x0200001C RID: 28
	public enum BlendMode
	{
		// Token: 0x0400018B RID: 395
		PremultipliedTransparency = 1,
		// Token: 0x0400018C RID: 396
		TraditionalTransparency = 5
	}
}
