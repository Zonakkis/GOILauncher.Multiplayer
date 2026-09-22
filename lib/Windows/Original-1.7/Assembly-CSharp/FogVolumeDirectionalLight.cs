using System;
using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000011 RID: 17
[ExecuteInEditMode]
public class FogVolumeDirectionalLight : MonoBehaviour
{
	// Token: 0x17000012 RID: 18
	// (get) Token: 0x06000077 RID: 119 RVA: 0x00006A90 File Offset: 0x00004C90
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

	// Token: 0x06000078 RID: 120 RVA: 0x00006AAC File Offset: 0x00004CAC
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
		this._GO_Canvas.hideFlags = HideFlags.HideInHierarchy;
		this._GO_Canvas.layer = LayerMask.NameToLayer("UI");
		this._GO_Image.layer = LayerMask.NameToLayer("UI");
		this._Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		this.Initialize();
		if (this._UpdateMode == FogVolumeDirectionalLight.UpdateMode.OnStart)
		{
			this.Render();
		}
	}

	// Token: 0x06000079 RID: 121 RVA: 0x00006CD0 File Offset: 0x00004ED0
	private void CreateMaterial()
	{
		Object.DestroyImmediate(this.quadMaterial);
		this.quadShader = Shader.Find("Hidden/DepthMapQuad");
		this.quadMaterial = new Material(this.quadShader);
		this.quadMaterial.name = "Depth camera quad material";
		this.quadMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	// Token: 0x0600007A RID: 122 RVA: 0x00006D28 File Offset: 0x00004F28
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
			Object.DestroyImmediate(this.Quad.GetComponent<MeshCollider>());
			this.Quad.hideFlags = HideFlags.HideInHierarchy;
			return;
		}
	}

	// Token: 0x0600007B RID: 123 RVA: 0x0000700C File Offset: 0x0000520C
	private void EnableVolumetricShadow(bool b)
	{
		if (this._TargetFogVolumes == null)
		{
			return;
		}
		if (this._TargetFogVolumes.Length != 0)
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
						this.FogVolumeMaterial.SetInt("_VolumetricShadowsEnabled", b ? 1 : 0);
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

	// Token: 0x0600007C RID: 124 RVA: 0x000070D4 File Offset: 0x000052D4
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
			Object.DestroyImmediate(this.depthRT);
			Object.DestroyImmediate(this.GOShadowCamera);
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

	// Token: 0x0600007D RID: 125 RVA: 0x00007310 File Offset: 0x00005510
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
				float num = ((this._ProminentFogVolume != null) ? this._MaxOf(this._ProminentFogVolume.fogVolumeScale.x, this._ProminentFogVolume.fogVolumeScale.y, this._ProminentFogVolume.fogVolumeScale.z) : 0f);
				if (this._MaxOf(fogVolume.fogVolumeScale.x, fogVolume.fogVolumeScale.y, fogVolume.fogVolumeScale.z) > num)
				{
					this._ProminentFogVolume = fogVolume;
				}
			}
		}
	}

	// Token: 0x0600007E RID: 126 RVA: 0x000073EC File Offset: 0x000055EC
	public void Render()
	{
		if (!this.depthRT)
		{
			this.Initialize();
		}
		if (this.depthRT.height != (int)this.Size)
		{
			Object.DestroyImmediate(this.depthRT);
			this.Initialize();
		}
		if (this._Antialiasing != (FogVolumeDirectionalLight.Antialiasing)this.depthRT.antiAliasing)
		{
			Object.DestroyImmediate(this.depthRT);
			this.Initialize();
		}
		if (!this.ShadowCamera)
		{
			this.Initialize();
		}
		switch (this._FocusMode)
		{
		case FogVolumeDirectionalLight.FocusMode.VolumeCenter:
			if (this._ProminentFogVolume != null)
			{
				this.FocusPosition = this._ProminentFogVolume.transform.position;
			}
			else
			{
				this.FocusPosition = Vector3.zero;
			}
			break;
		case FogVolumeDirectionalLight.FocusMode.GameCameraPosition:
			this.FocusPosition = this._GameCamera.transform.position;
			break;
		case FogVolumeDirectionalLight.FocusMode.GameObject:
			if (this._GameObjectFocus)
			{
				this.FocusPosition = this._GameObjectFocus.transform.position;
			}
			break;
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

	// Token: 0x0600007F RID: 127 RVA: 0x000076B7 File Offset: 0x000058B7
	private void OnDisable()
	{
		Object.DestroyImmediate(this.depthRT);
		if (this._GO_Canvas)
		{
			this._GO_Canvas.SetActive(false);
		}
		this.EnableVolumetricShadow(false);
	}

	// Token: 0x06000080 RID: 128 RVA: 0x000076E4 File Offset: 0x000058E4
	private void OnDestroy()
	{
		Object.DestroyImmediate(this.GOShadowCamera);
		Object.DestroyImmediate(this._GO_Canvas);
		Object.DestroyImmediate(this.Quad);
	}

	// Token: 0x06000081 RID: 129 RVA: 0x00007708 File Offset: 0x00005908
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

	// Token: 0x06000082 RID: 130 RVA: 0x00007744 File Offset: 0x00005944
	public void AddAllFogVolumesToThisLight()
	{
		this._ProminentFogVolume = null;
		FogVolume[] array = Object.FindObjectsOfType<FogVolume>();
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

	// Token: 0x06000083 RID: 131 RVA: 0x000077D2 File Offset: 0x000059D2
	public void RemoveAllFogVolumesFromThisLight()
	{
		this._ProminentFogVolume = null;
		this._TargetFogVolumes = null;
	}

	// Token: 0x06000084 RID: 132 RVA: 0x000077E2 File Offset: 0x000059E2
	private float _MaxOf(float _a, float _b)
	{
		if (_a < _b)
		{
			return _b;
		}
		return _a;
	}

	// Token: 0x06000085 RID: 133 RVA: 0x000077EB File Offset: 0x000059EB
	private float _MaxOf(float _a, float _b, float _c)
	{
		return this._MaxOf(this._MaxOf(_a, _b), _c);
	}

	// Token: 0x04000100 RID: 256
	public FogVolume[] _TargetFogVolumes;

	// Token: 0x04000101 RID: 257
	public Vector2 MiniaturePosition = new Vector2(110f, 320f);

	// Token: 0x04000102 RID: 258
	public FogVolume _ProminentFogVolume;

	// Token: 0x04000103 RID: 259
	public Material FogVolumeMaterial;

	// Token: 0x04000104 RID: 260
	public float _CameraVerticalPosition = 500f;

	// Token: 0x04000105 RID: 261
	private RenderTexture depthRT;

	// Token: 0x04000106 RID: 262
	public FogVolumeDirectionalLight.Antialiasing _Antialiasing = FogVolumeDirectionalLight.Antialiasing._1;

	// Token: 0x04000107 RID: 263
	public FogVolumeDirectionalLight.Resolution Size = FogVolumeDirectionalLight.Resolution._512;

	// Token: 0x04000108 RID: 264
	public Camera ShadowCamera;

	// Token: 0x04000109 RID: 265
	public float _FogVolumeShadowMapEdgeSoftness = 0.001f;

	// Token: 0x0400010A RID: 266
	public FogVolumeDirectionalLight.ScaleMode _ScaleMode;

	// Token: 0x0400010B RID: 267
	public LayerMask LayersToRender;

	// Token: 0x0400010C RID: 268
	[HideInInspector]
	public Shader outputDepth;

	// Token: 0x0400010D RID: 269
	[HideInInspector]
	public GameObject GOShadowCamera;

	// Token: 0x0400010E RID: 270
	public bool CameraVisible;

	// Token: 0x0400010F RID: 271
	private Image _CanvasImage;

	// Token: 0x04000110 RID: 272
	public FogVolumeDirectionalLight.UpdateMode _UpdateMode = FogVolumeDirectionalLight.UpdateMode.Interleaved;

	// Token: 0x04000111 RID: 273
	public float Scale = 50f;

	// Token: 0x04000112 RID: 274
	[Range(0f, 100f)]
	public int SkipFrames = 2;

	// Token: 0x04000113 RID: 275
	public bool ShowMiniature;

	// Token: 0x04000114 RID: 276
	private GameObject _GO_Canvas;

	// Token: 0x04000115 RID: 277
	private GameObject _GO_Image;

	// Token: 0x04000116 RID: 278
	private Canvas _Canvas;

	// Token: 0x04000117 RID: 279
	public Material DebugViewMaterial;

	// Token: 0x04000118 RID: 280
	private GameObject Quad;

	// Token: 0x04000119 RID: 281
	private Vector3 FocusPosition;

	// Token: 0x0400011A RID: 282
	private FogVolumeData _FogVolumeData;

	// Token: 0x0400011B RID: 283
	private Camera _GameCamera;

	// Token: 0x0400011C RID: 284
	public Transform _GameObjectFocus;

	// Token: 0x0400011D RID: 285
	public FogVolumeDirectionalLight.FocusMode _FocusMode;

	// Token: 0x0400011E RID: 286
	private Material quadMaterial;

	// Token: 0x0400011F RID: 287
	public Shader quadShader;

	// Token: 0x04000120 RID: 288
	private RenderTextureFormat rt_DepthFormat;

	// Token: 0x02000221 RID: 545
	public enum Resolution
	{
		// Token: 0x04000DEB RID: 3563
		_256 = 256,
		// Token: 0x04000DEC RID: 3564
		_512 = 512,
		// Token: 0x04000DED RID: 3565
		_1024 = 1024,
		// Token: 0x04000DEE RID: 3566
		_2048 = 2048,
		// Token: 0x04000DEF RID: 3567
		_4096 = 4096
	}

	// Token: 0x02000222 RID: 546
	public enum Antialiasing
	{
		// Token: 0x04000DF1 RID: 3569
		_1 = 1,
		// Token: 0x04000DF2 RID: 3570
		_2,
		// Token: 0x04000DF3 RID: 3571
		_4 = 4,
		// Token: 0x04000DF4 RID: 3572
		_8 = 8
	}

	// Token: 0x02000223 RID: 547
	public enum ScaleMode
	{
		// Token: 0x04000DF6 RID: 3574
		VolumeMaxAxis,
		// Token: 0x04000DF7 RID: 3575
		Manual
	}

	// Token: 0x02000224 RID: 548
	public enum UpdateMode
	{
		// Token: 0x04000DF9 RID: 3577
		OnStart,
		// Token: 0x04000DFA RID: 3578
		Interleaved
	}

	// Token: 0x02000225 RID: 549
	public enum FocusMode
	{
		// Token: 0x04000DFC RID: 3580
		VolumeCenter,
		// Token: 0x04000DFD RID: 3581
		GameCameraPosition,
		// Token: 0x04000DFE RID: 3582
		GameObject
	}
}
