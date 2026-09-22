using System;
using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000018 RID: 24
[ExecuteInEditMode]
public class FogVolumeDirectionalLight : MonoBehaviour
{
	// Token: 0x17000012 RID: 18
	// (get) Token: 0x0600007A RID: 122 RVA: 0x00002B49 File Offset: 0x00000D49
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

	// Token: 0x0600007B RID: 123 RVA: 0x000185F8 File Offset: 0x000167F8
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

	// Token: 0x0600007C RID: 124 RVA: 0x0001881C File Offset: 0x00016A1C
	private void CreateMaterial()
	{
		global::UnityEngine.Object.DestroyImmediate(this.quadMaterial);
		this.quadShader = Shader.Find("Hidden/DepthMapQuad");
		this.quadMaterial = new Material(this.quadShader);
		this.quadMaterial.name = "Depth camera quad material";
		this.quadMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	// Token: 0x0600007D RID: 125 RVA: 0x00018874 File Offset: 0x00016A74
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
			this.Quad.hideFlags = HideFlags.HideInHierarchy;
			return;
		}
	}

	// Token: 0x0600007E RID: 126 RVA: 0x00018B58 File Offset: 0x00016D58
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

	// Token: 0x0600007F RID: 127 RVA: 0x00018C20 File Offset: 0x00016E20
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

	// Token: 0x06000080 RID: 128 RVA: 0x00018E5C File Offset: 0x0001705C
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

	// Token: 0x06000081 RID: 129 RVA: 0x00018F38 File Offset: 0x00017138
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

	// Token: 0x06000082 RID: 130 RVA: 0x00002B65 File Offset: 0x00000D65
	private void OnDisable()
	{
		global::UnityEngine.Object.DestroyImmediate(this.depthRT);
		if (this._GO_Canvas)
		{
			this._GO_Canvas.SetActive(false);
		}
		this.EnableVolumetricShadow(false);
	}

	// Token: 0x06000083 RID: 131 RVA: 0x00002B92 File Offset: 0x00000D92
	private void OnDestroy()
	{
		global::UnityEngine.Object.DestroyImmediate(this.GOShadowCamera);
		global::UnityEngine.Object.DestroyImmediate(this._GO_Canvas);
		global::UnityEngine.Object.DestroyImmediate(this.Quad);
	}

	// Token: 0x06000084 RID: 132 RVA: 0x00019204 File Offset: 0x00017404
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

	// Token: 0x06000085 RID: 133 RVA: 0x00019240 File Offset: 0x00017440
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

	// Token: 0x06000086 RID: 134 RVA: 0x00002BB5 File Offset: 0x00000DB5
	public void RemoveAllFogVolumesFromThisLight()
	{
		this._ProminentFogVolume = null;
		this._TargetFogVolumes = null;
	}

	// Token: 0x06000087 RID: 135 RVA: 0x00002BC5 File Offset: 0x00000DC5
	private float _MaxOf(float _a, float _b)
	{
		if (_a < _b)
		{
			return _b;
		}
		return _a;
	}

	// Token: 0x06000088 RID: 136 RVA: 0x00002BCE File Offset: 0x00000DCE
	private float _MaxOf(float _a, float _b, float _c)
	{
		return this._MaxOf(this._MaxOf(_a, _b), _c);
	}

	// Token: 0x04000119 RID: 281
	public FogVolume[] _TargetFogVolumes;

	// Token: 0x0400011A RID: 282
	public Vector2 MiniaturePosition = new Vector2(110f, 320f);

	// Token: 0x0400011B RID: 283
	public FogVolume _ProminentFogVolume;

	// Token: 0x0400011C RID: 284
	public Material FogVolumeMaterial;

	// Token: 0x0400011D RID: 285
	public float _CameraVerticalPosition = 500f;

	// Token: 0x0400011E RID: 286
	private RenderTexture depthRT;

	// Token: 0x0400011F RID: 287
	public FogVolumeDirectionalLight.Antialiasing _Antialiasing = FogVolumeDirectionalLight.Antialiasing._1;

	// Token: 0x04000120 RID: 288
	public FogVolumeDirectionalLight.Resolution Size = FogVolumeDirectionalLight.Resolution._512;

	// Token: 0x04000121 RID: 289
	public Camera ShadowCamera;

	// Token: 0x04000122 RID: 290
	public float _FogVolumeShadowMapEdgeSoftness = 0.001f;

	// Token: 0x04000123 RID: 291
	public FogVolumeDirectionalLight.ScaleMode _ScaleMode;

	// Token: 0x04000124 RID: 292
	public LayerMask LayersToRender;

	// Token: 0x04000125 RID: 293
	[HideInInspector]
	public Shader outputDepth;

	// Token: 0x04000126 RID: 294
	[HideInInspector]
	public GameObject GOShadowCamera;

	// Token: 0x04000127 RID: 295
	public bool CameraVisible;

	// Token: 0x04000128 RID: 296
	private Image _CanvasImage;

	// Token: 0x04000129 RID: 297
	public FogVolumeDirectionalLight.UpdateMode _UpdateMode = FogVolumeDirectionalLight.UpdateMode.Interleaved;

	// Token: 0x0400012A RID: 298
	public float Scale = 50f;

	// Token: 0x0400012B RID: 299
	[Range(0f, 100f)]
	public int SkipFrames = 2;

	// Token: 0x0400012C RID: 300
	public bool ShowMiniature;

	// Token: 0x0400012D RID: 301
	private GameObject _GO_Canvas;

	// Token: 0x0400012E RID: 302
	private GameObject _GO_Image;

	// Token: 0x0400012F RID: 303
	private Canvas _Canvas;

	// Token: 0x04000130 RID: 304
	public Material DebugViewMaterial;

	// Token: 0x04000131 RID: 305
	private GameObject Quad;

	// Token: 0x04000132 RID: 306
	private Vector3 FocusPosition;

	// Token: 0x04000133 RID: 307
	private FogVolumeData _FogVolumeData;

	// Token: 0x04000134 RID: 308
	private Camera _GameCamera;

	// Token: 0x04000135 RID: 309
	public Transform _GameObjectFocus;

	// Token: 0x04000136 RID: 310
	public FogVolumeDirectionalLight.FocusMode _FocusMode;

	// Token: 0x04000137 RID: 311
	private Material quadMaterial;

	// Token: 0x04000138 RID: 312
	public Shader quadShader;

	// Token: 0x04000139 RID: 313
	private RenderTextureFormat rt_DepthFormat;

	// Token: 0x02000019 RID: 25
	public enum Resolution
	{
		// Token: 0x0400013B RID: 315
		_256 = 256,
		// Token: 0x0400013C RID: 316
		_512 = 512,
		// Token: 0x0400013D RID: 317
		_1024 = 1024,
		// Token: 0x0400013E RID: 318
		_2048 = 2048,
		// Token: 0x0400013F RID: 319
		_4096 = 4096
	}

	// Token: 0x0200001A RID: 26
	public enum Antialiasing
	{
		// Token: 0x04000141 RID: 321
		_1 = 1,
		// Token: 0x04000142 RID: 322
		_2,
		// Token: 0x04000143 RID: 323
		_4 = 4,
		// Token: 0x04000144 RID: 324
		_8 = 8
	}

	// Token: 0x0200001B RID: 27
	public enum ScaleMode
	{
		// Token: 0x04000146 RID: 326
		VolumeMaxAxis,
		// Token: 0x04000147 RID: 327
		Manual
	}

	// Token: 0x0200001C RID: 28
	public enum UpdateMode
	{
		// Token: 0x04000149 RID: 329
		OnStart,
		// Token: 0x0400014A RID: 330
		Interleaved
	}

	// Token: 0x0200001D RID: 29
	public enum FocusMode
	{
		// Token: 0x0400014C RID: 332
		VolumeCenter,
		// Token: 0x0400014D RID: 333
		GameCameraPosition,
		// Token: 0x0400014E RID: 334
		GameObject
	}
}
