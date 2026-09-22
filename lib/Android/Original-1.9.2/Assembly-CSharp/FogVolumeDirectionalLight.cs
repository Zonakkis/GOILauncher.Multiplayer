using System;
using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200000F RID: 15
[ExecuteInEditMode]
public class FogVolumeDirectionalLight : MonoBehaviour
{
	// Token: 0x1700000D RID: 13
	// (get) Token: 0x06000059 RID: 89 RVA: 0x00006CD1 File Offset: 0x000050D1
	public Material QuadMaterial
	{
		get
		{
			if (this.quadMaterial == null)
			{
				this.CreateMaterial();
			}
			return this.quadMaterial;
		}
	}

	// Token: 0x0600005A RID: 90 RVA: 0x00006CF0 File Offset: 0x000050F0
	private void OnEnable()
	{
		this._GO_Canvas = GameObject.Find("FogVolume Debug Canvas");
		if (!this._GO_Canvas)
		{
			this._GO_Canvas = new GameObject("FogVolume Debug Canvas");
		}
		this._GO_Image = GameObject.Find("FogVolume Image");
		if (!this._GO_Image)
		{
			this._GO_Image = new GameObject("FogVolume Image");
			this._CanvasImage = this._GO_Image.AddComponent<Image>();
			this._CanvasImage.material = this.DebugViewMaterial;
			this._CanvasImage.rectTransform.position = new Vector3(this.MiniaturePosition.x, this.MiniaturePosition.y, 0f);
			this._CanvasImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			this._CanvasImage.rectTransform.anchorMax = new Vector2(0f, 0f);
			this._CanvasImage.rectTransform.anchorMin = new Vector2(0f, 0f);
			this._CanvasImage.rectTransform.localScale = new Vector3(2f, 2f, 2f);
		}
		if (!this._CanvasImage)
		{
			this._CanvasImage = this._GO_Image.GetComponent<Image>();
		}
		this._CanvasImage.material = this.DebugViewMaterial;
		this._GO_Image.transform.SetParent(this._GO_Canvas.transform);
		this._GO_Canvas.AddComponent<CanvasScaler>();
		this._GO_Canvas.GetComponent<CanvasScaler>().scaleFactor = 1f;
		this._GO_Canvas.GetComponent<CanvasScaler>().referencePixelsPerUnit = 100f;
		this._Canvas = this._GO_Canvas.GetComponent<Canvas>();
		this._GO_Canvas.hideFlags = HideFlags.None;
		this._GO_Canvas.layer = LayerMask.NameToLayer("UI");
		this._GO_Image.layer = LayerMask.NameToLayer("UI");
		this._Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		this.Initialize();
		if (this._UpdateMode == FogVolumeDirectionalLight.UpdateMode.OnStart)
		{
			this.Render();
		}
	}

	// Token: 0x0600005B RID: 91 RVA: 0x00006F20 File Offset: 0x00005320
	private void CreateMaterial()
	{
		global::UnityEngine.Object.DestroyImmediate(this.quadMaterial);
		this.quadShader = Shader.Find("Hidden/DepthMapQuad");
		this.quadMaterial = new Material(this.quadShader);
		this.quadMaterial.name = "Depth camera quad material";
		this.quadMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	// Token: 0x0600005C RID: 92 RVA: 0x00006F78 File Offset: 0x00005378
	private void Initialize()
	{
		this.CreateMaterial();
		if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat))
		{
			this.rt_DepthFormat = RenderTextureFormat.RGFloat;
		}
		else
		{
			this.rt_DepthFormat = RenderTextureFormat.DefaultHDR;
		}
		GameObject gameObject = GameObject.Find("Fog Volume Data");
		if (gameObject)
		{
			this._FogVolumeData = gameObject.GetComponent<FogVolumeData>();
			this._GameCamera = this._FogVolumeData.GameCamera;
			this.GOShadowCamera = GameObject.Find("FogVolumeShadowCamera");
			if (!this.GOShadowCamera)
			{
				this.GOShadowCamera = new GameObject();
				this.GOShadowCamera.name = "FogVolumeShadowCamera";
			}
			if (!this.GOShadowCamera)
			{
				MonoBehaviour.print("Shadow camera is lost");
			}
			else
			{
				this.ShadowCamera = this.GOShadowCamera.GetComponent<Camera>();
			}
			if (!this.depthRT)
			{
				this.depthRT = new RenderTexture((int)this.Size, (int)this.Size, 16, this.rt_DepthFormat);
				this.depthRT.antiAliasing = (int)this._Antialiasing;
				this.depthRT.filterMode = FilterMode.Bilinear;
				this.depthRT.name = "FogVolumeShadowMap";
				this.depthRT.wrapMode = TextureWrapMode.Clamp;
			}
			if (!this.ShadowCamera)
			{
				this.ShadowCamera = this.GOShadowCamera.AddComponent<Camera>();
			}
			else
			{
				this.ShadowCamera = this.GOShadowCamera.GetComponent<Camera>();
			}
			this.ShadowCamera.clearFlags = CameraClearFlags.Color;
			this.ShadowCamera.backgroundColor = Color.black;
			this.ShadowCamera.orthographic = true;
			this.ShadowCamera.farClipPlane = 10000f;
			this.ShadowCamera.enabled = false;
			this.ShadowCamera.stereoTargetEye = StereoTargetEyeMask.None;
			this.ShadowCamera.targetTexture = this.depthRT;
			this.ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));
			this.ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
			this.ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
			this.ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
			this.ShadowCamera.transform.parent = base.gameObject.transform;
			this.Quad = GameObject.Find("Depth map background");
			if (!this.Quad)
			{
				this.Quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			}
			this.Quad.name = "Depth map background";
			this.Quad.GetComponent<MeshRenderer>().sharedMaterial = this.QuadMaterial;
			this.Quad.transform.parent = this.ShadowCamera.transform;
			global::UnityEngine.Object.DestroyImmediate(this.Quad.GetComponent<MeshCollider>());
			this.Quad.hideFlags = HideFlags.None;
			return;
		}
	}

	// Token: 0x0600005D RID: 93 RVA: 0x0000727C File Offset: 0x0000567C
	private void EnableVolumetricShadow(bool b)
	{
		if (this._TargetFogVolumes == null)
		{
			return;
		}
		if (this._TargetFogVolumes.Length > 0)
		{
			float num = 0f;
			int num2 = 0;
			for (int i = 0; i < this._TargetFogVolumes.Length; i++)
			{
				FogVolume fogVolume = this._TargetFogVolumes[i];
				if (fogVolume != null && fogVolume._FogType == FogVolume.FogType.Textured)
				{
					if (fogVolume.enabled)
					{
						this.FogVolumeMaterial = fogVolume.FogMaterial;
						this.FogVolumeMaterial.SetInt("_VolumetricShadowsEnabled", (!b) ? 0 : 1);
					}
					float num3 = this._MaxOf(fogVolume.fogVolumeScale.x, fogVolume.fogVolumeScale.y, fogVolume.fogVolumeScale.z);
					if (num3 > num)
					{
						num = num3;
						num2 = i;
					}
				}
			}
			this._ProminentFogVolume = this._TargetFogVolumes[num2];
		}
	}

	// Token: 0x0600005E RID: 94 RVA: 0x00007360 File Offset: 0x00005760
	private void Update()
	{
		if (this._CanvasImage.material != null)
		{
			this._CanvasImage.material = this.DebugViewMaterial;
		}
		if (!this.ShadowCamera)
		{
			this.Initialize();
		}
		if (this._TargetFogVolumes != null && this._AtLeastOneFogVolumeInArray())
		{
			this.EnableVolumetricShadow(this.depthRT);
			this.LayersToRender &= ~(1 << LayerMask.NameToLayer("FogVolume"));
			this.LayersToRender &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
			this.LayersToRender &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
			this.LayersToRender &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
			this.ShadowCamera.cullingMask = this.LayersToRender;
			this.Refresh();
			if (this._ScaleMode == FogVolumeDirectionalLight.ScaleMode.VolumeMaxAxis)
			{
				if (this._ProminentFogVolume != null)
				{
					this.ShadowCamera.orthographicSize = this._MaxOf(this._ProminentFogVolume.fogVolumeScale.x, this._ProminentFogVolume.fogVolumeScale.y, this._ProminentFogVolume.fogVolumeScale.z) * 0.5f;
				}
			}
			else
			{
				this.ShadowCamera.orthographicSize = this.Scale;
			}
			if (this.ShadowCamera.cullingMask != 0 && this._ProminentFogVolume != null && this._UpdateMode == FogVolumeDirectionalLight.UpdateMode.Interleaved && ExtensionMethods.TimeSnap(this.SkipFrames))
			{
				this.Render();
			}
		}
		else if (this.depthRT)
		{
			global::UnityEngine.Object.DestroyImmediate(this.depthRT);
			global::UnityEngine.Object.DestroyImmediate(this.GOShadowCamera);
		}
		if (!this.ShowMiniature && this._GO_Canvas.activeInHierarchy)
		{
			this._GO_Canvas.SetActive(this.ShowMiniature);
		}
		if (this.ShowMiniature && !this._GO_Canvas.activeInHierarchy)
		{
			this._GO_Canvas.SetActive(this.ShowMiniature);
		}
	}

	// Token: 0x0600005F RID: 95 RVA: 0x000075C8 File Offset: 0x000059C8
	public void Refresh()
	{
		if (this._TargetFogVolumes == null)
		{
			this._ProminentFogVolume = null;
			return;
		}
		for (int i = 0; i < this._TargetFogVolumes.Length; i++)
		{
			FogVolume fogVolume = this._TargetFogVolumes[i];
			if (fogVolume != null && fogVolume._FogType == FogVolume.FogType.Textured && fogVolume.HasUpdatedBoxMesh)
			{
				float num = ((!(this._ProminentFogVolume != null)) ? 0f : this._MaxOf(this._ProminentFogVolume.fogVolumeScale.x, this._ProminentFogVolume.fogVolumeScale.y, this._ProminentFogVolume.fogVolumeScale.z));
				float num2 = this._MaxOf(fogVolume.fogVolumeScale.x, fogVolume.fogVolumeScale.y, fogVolume.fogVolumeScale.z);
				if (num2 > num)
				{
					this._ProminentFogVolume = fogVolume;
				}
			}
		}
	}

	// Token: 0x06000060 RID: 96 RVA: 0x000076B8 File Offset: 0x00005AB8
	public void Render()
	{
		if (!this.depthRT)
		{
			this.Initialize();
		}
		if (this.depthRT.height != (int)this.Size)
		{
			global::UnityEngine.Object.DestroyImmediate(this.depthRT);
			this.Initialize();
		}
		if (this._Antialiasing != (FogVolumeDirectionalLight.Antialiasing)this.depthRT.antiAliasing)
		{
			global::UnityEngine.Object.DestroyImmediate(this.depthRT);
			this.Initialize();
		}
		if (!this.ShadowCamera)
		{
			this.Initialize();
		}
		FogVolumeDirectionalLight.FocusMode focusMode = this._FocusMode;
		if (focusMode != FogVolumeDirectionalLight.FocusMode.GameCameraPosition)
		{
			if (focusMode != FogVolumeDirectionalLight.FocusMode.VolumeCenter)
			{
				if (focusMode == FogVolumeDirectionalLight.FocusMode.GameObject)
				{
					if (this._GameObjectFocus)
					{
						this.FocusPosition = this._GameObjectFocus.transform.position;
					}
				}
			}
			else if (this._ProminentFogVolume != null)
			{
				this.FocusPosition = this._ProminentFogVolume.transform.position;
			}
			else
			{
				this.FocusPosition = Vector3.zero;
			}
		}
		else
		{
			this.FocusPosition = this._GameCamera.transform.position;
		}
		Vector3 vector = new Vector3(0f, 0f, this.FocusPosition.y - this._CameraVerticalPosition);
		this.ShadowCamera.transform.position = this.FocusPosition;
		this.ShadowCamera.transform.Translate(vector, Space.Self);
		Vector3 vector2 = new Vector3(this.ShadowCamera.orthographicSize * 2f, this.ShadowCamera.orthographicSize * 2f, this.ShadowCamera.orthographicSize * 2f);
		this.Quad.transform.localScale = vector2;
		this.Quad.transform.position = this.ShadowCamera.transform.position;
		Vector3 vector3 = new Vector3(0f, 0f, this.ShadowCamera.farClipPlane - 50f);
		this.Quad.transform.Translate(vector3, Space.Self);
		this.ShadowCamera.transform.rotation = Quaternion.LookRotation(base.transform.forward);
		Shader.SetGlobalVector("_ShadowCameraPosition", this.ShadowCamera.transform.position);
		Shader.SetGlobalMatrix("_ShadowCameraProjection", this.ShadowCamera.worldToCameraMatrix);
		Shader.SetGlobalFloat("_ShadowCameraSize", this.ShadowCamera.orthographicSize);
		Shader.SetGlobalVector("_ShadowLightDir", this.ShadowCamera.transform.forward);
		this.quadShader.maximumLOD = 1;
		Shader.SetGlobalFloat("_FogVolumeShadowMapEdgeSoftness", 20f / this._FogVolumeShadowMapEdgeSoftness);
		this.ShadowCamera.RenderWithShader(this.outputDepth, "RenderType");
		this.quadShader.maximumLOD = 100;
		Shader.SetGlobalTexture("_ShadowTexture", this.depthRT);
	}

	// Token: 0x06000061 RID: 97 RVA: 0x000079A8 File Offset: 0x00005DA8
	private void OnDisable()
	{
		global::UnityEngine.Object.DestroyImmediate(this.depthRT);
		if (this._GO_Canvas)
		{
			this._GO_Canvas.SetActive(false);
		}
		this.EnableVolumetricShadow(false);
	}

	// Token: 0x06000062 RID: 98 RVA: 0x000079D8 File Offset: 0x00005DD8
	private void OnDestroy()
	{
		global::UnityEngine.Object.DestroyImmediate(this.GOShadowCamera);
		global::UnityEngine.Object.DestroyImmediate(this._GO_Canvas);
		global::UnityEngine.Object.DestroyImmediate(this.Quad);
	}

	// Token: 0x06000063 RID: 99 RVA: 0x000079FC File Offset: 0x00005DFC
	private bool _AtLeastOneFogVolumeInArray()
	{
		if (this._TargetFogVolumes != null)
		{
			for (int i = 0; i < this._TargetFogVolumes.Length; i++)
			{
				if (this._TargetFogVolumes[i] != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000064 RID: 100 RVA: 0x00007A44 File Offset: 0x00005E44
	public void AddAllFogVolumesToThisLight()
	{
		this._ProminentFogVolume = null;
		FogVolume[] array = global::UnityEngine.Object.FindObjectsOfType<FogVolume>();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i]._FogType == FogVolume.FogType.Textured)
			{
				num++;
			}
		}
		this._TargetFogVolumes = new FogVolume[num];
		int num2 = 0;
		for (int j = 0; j < array.Length; j++)
		{
			FogVolume fogVolume = array[j];
			if (fogVolume != null && fogVolume._FogType == FogVolume.FogType.Textured)
			{
				this._TargetFogVolumes[num2++] = array[j];
			}
		}
	}

	// Token: 0x06000065 RID: 101 RVA: 0x00007AEA File Offset: 0x00005EEA
	public void RemoveAllFogVolumesFromThisLight()
	{
		this._ProminentFogVolume = null;
		this._TargetFogVolumes = null;
	}

	// Token: 0x06000066 RID: 102 RVA: 0x00007AFA File Offset: 0x00005EFA
	private float _MaxOf(float _a, float _b)
	{
		return (_a < _b) ? _b : _a;
	}

	// Token: 0x06000067 RID: 103 RVA: 0x00007B0A File Offset: 0x00005F0A
	private float _MaxOf(float _a, float _b, float _c)
	{
		return this._MaxOf(this._MaxOf(_a, _b), _c);
	}

	// Token: 0x040000F4 RID: 244
	public FogVolume[] _TargetFogVolumes;

	// Token: 0x040000F5 RID: 245
	public Vector2 MiniaturePosition = new Vector2(110f, 320f);

	// Token: 0x040000F6 RID: 246
	public FogVolume _ProminentFogVolume;

	// Token: 0x040000F7 RID: 247
	public Material FogVolumeMaterial;

	// Token: 0x040000F8 RID: 248
	public float _CameraVerticalPosition = 500f;

	// Token: 0x040000F9 RID: 249
	private RenderTexture depthRT;

	// Token: 0x040000FA RID: 250
	public FogVolumeDirectionalLight.Antialiasing _Antialiasing = FogVolumeDirectionalLight.Antialiasing._1;

	// Token: 0x040000FB RID: 251
	public FogVolumeDirectionalLight.Resolution Size = FogVolumeDirectionalLight.Resolution._512;

	// Token: 0x040000FC RID: 252
	public Camera ShadowCamera;

	// Token: 0x040000FD RID: 253
	public float _FogVolumeShadowMapEdgeSoftness = 0.001f;

	// Token: 0x040000FE RID: 254
	public FogVolumeDirectionalLight.ScaleMode _ScaleMode;

	// Token: 0x040000FF RID: 255
	public LayerMask LayersToRender;

	// Token: 0x04000100 RID: 256
	[HideInInspector]
	public Shader outputDepth;

	// Token: 0x04000101 RID: 257
	[HideInInspector]
	public GameObject GOShadowCamera;

	// Token: 0x04000102 RID: 258
	public bool CameraVisible;

	// Token: 0x04000103 RID: 259
	private Image _CanvasImage;

	// Token: 0x04000104 RID: 260
	public FogVolumeDirectionalLight.UpdateMode _UpdateMode = FogVolumeDirectionalLight.UpdateMode.Interleaved;

	// Token: 0x04000105 RID: 261
	public float Scale = 50f;

	// Token: 0x04000106 RID: 262
	[Range(0f, 100f)]
	public int SkipFrames = 2;

	// Token: 0x04000107 RID: 263
	public bool ShowMiniature;

	// Token: 0x04000108 RID: 264
	private GameObject _GO_Canvas;

	// Token: 0x04000109 RID: 265
	private GameObject _GO_Image;

	// Token: 0x0400010A RID: 266
	private Canvas _Canvas;

	// Token: 0x0400010B RID: 267
	public Material DebugViewMaterial;

	// Token: 0x0400010C RID: 268
	private GameObject Quad;

	// Token: 0x0400010D RID: 269
	private Vector3 FocusPosition;

	// Token: 0x0400010E RID: 270
	private FogVolumeData _FogVolumeData;

	// Token: 0x0400010F RID: 271
	private Camera _GameCamera;

	// Token: 0x04000110 RID: 272
	public Transform _GameObjectFocus;

	// Token: 0x04000111 RID: 273
	public FogVolumeDirectionalLight.FocusMode _FocusMode;

	// Token: 0x04000112 RID: 274
	private Material quadMaterial;

	// Token: 0x04000113 RID: 275
	public Shader quadShader;

	// Token: 0x04000114 RID: 276
	private RenderTextureFormat rt_DepthFormat;

	// Token: 0x02000010 RID: 16
	public enum Resolution
	{
		// Token: 0x04000116 RID: 278
		_256 = 256,
		// Token: 0x04000117 RID: 279
		_512 = 512,
		// Token: 0x04000118 RID: 280
		_1024 = 1024,
		// Token: 0x04000119 RID: 281
		_2048 = 2048,
		// Token: 0x0400011A RID: 282
		_4096 = 4096
	}

	// Token: 0x02000011 RID: 17
	public enum Antialiasing
	{
		// Token: 0x0400011C RID: 284
		_1 = 1,
		// Token: 0x0400011D RID: 285
		_2,
		// Token: 0x0400011E RID: 286
		_4 = 4,
		// Token: 0x0400011F RID: 287
		_8 = 8
	}

	// Token: 0x02000012 RID: 18
	public enum ScaleMode
	{
		// Token: 0x04000121 RID: 289
		VolumeMaxAxis,
		// Token: 0x04000122 RID: 290
		Manual
	}

	// Token: 0x02000013 RID: 19
	public enum UpdateMode
	{
		// Token: 0x04000124 RID: 292
		OnStart,
		// Token: 0x04000125 RID: 293
		Interleaved
	}

	// Token: 0x02000014 RID: 20
	public enum FocusMode
	{
		// Token: 0x04000127 RID: 295
		VolumeCenter,
		// Token: 0x04000128 RID: 296
		GameCameraPosition,
		// Token: 0x04000129 RID: 297
		GameObject
	}
}
