using System;
using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x0200000F RID: 15
[ExecuteInEditMode]
public class FogVolume : MonoBehaviour
{
	// Token: 0x0600003E RID: 62 RVA: 0x000033DF File Offset: 0x000015DF
	public void setNoiseIntensity(float value)
	{
		this.NoiseIntensity = value;
	}

	// Token: 0x1700000C RID: 12
	// (get) Token: 0x06000040 RID: 64 RVA: 0x00003405 File Offset: 0x00001605
	// (set) Token: 0x0600003F RID: 63 RVA: 0x000033E8 File Offset: 0x000015E8
	public int ShadowCameraPosition
	{
		get
		{
			return this.shadowCameraPosition;
		}
		set
		{
			if (value != this.shadowCameraPosition)
			{
				this.shadowCameraPosition = value;
				this._ShadowCamera.CameraTransform();
			}
		}
	}

	// Token: 0x06000041 RID: 65 RVA: 0x0000340D File Offset: 0x0000160D
	public float GetVisibility()
	{
		return this.Visibility;
	}

	// Token: 0x1700000D RID: 13
	// (get) Token: 0x06000042 RID: 66 RVA: 0x00003415 File Offset: 0x00001615
	public Material FogMaterial
	{
		get
		{
			if (this.fogMaterial == null)
			{
				this.CreateMaterial();
			}
			return this.fogMaterial;
		}
	}

	// Token: 0x06000043 RID: 67 RVA: 0x00003431 File Offset: 0x00001631
	private void RemoveMaterial()
	{
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00003434 File Offset: 0x00001634
	private void CreateMaterial()
	{
		if (this.SaveMaterials)
		{
			this.FogVolumeShader = Shader.Find("Hidden/FogVolume");
			this.fogMaterial = new Material(this.FogVolumeShader);
			return;
		}
		Object.DestroyImmediate(this.fogMaterial);
		this.FogVolumeShader = Shader.Find("Hidden/FogVolume");
		this.fogMaterial = new Material(this.FogVolumeShader);
		try
		{
			this.fogMaterial.name = this.FogVolumeGameObject.name + " Material";
		}
		catch
		{
			MonoBehaviour.print(base.name);
		}
		this.fogMaterial.hideFlags = HideFlags.HideAndDontSave;
	}

	// Token: 0x06000045 RID: 69 RVA: 0x000034E4 File Offset: 0x000016E4
	private void ShadowProjectorLock()
	{
		if (this.ShadowProjector)
		{
			this.ShadowProjector.transform.position = new Vector3(this.FogVolumeGameObject.transform.position.x, this.ShadowProjector.transform.position.y, this.FogVolumeGameObject.transform.position.z);
			Vector3 vector = new Vector3(this.fogVolumeScale.x, this.ShadowProjector.transform.localScale.y, this.fogVolumeScale.z);
			this.ShadowProjector.transform.localScale = vector;
			this.ShadowProjector.transform.localRotation = default(Quaternion);
		}
	}

	// Token: 0x06000046 RID: 70 RVA: 0x000035B4 File Offset: 0x000017B4
	private void ShadowMapSetup()
	{
		if (this.ShadowCameraGO)
		{
			this.ShadowCameraGO.GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
			this.ShadowCameraGO.GetComponent<Camera>().renderingPath = RenderingPath.Forward;
			this._ShadowCamera.CameraTransform();
		}
		if (this.CastShadows)
		{
			this.fogVolumeScale.z = this.fogVolumeScale.x;
		}
		if (this.CastShadows)
		{
			this.ShadowProjector = GameObject.Find(this.FogVolumeGameObject.name + " Shadow Projector");
			if (this.ShadowProjector == null)
			{
				this.ShadowProjector = new GameObject();
				this.ShadowProjector.AddComponent<MeshFilter>();
				this.ShadowProjector.AddComponent<MeshRenderer>();
				this.ShadowProjector.transform.parent = this.FogVolumeGameObject.transform;
				this.ShadowProjector.transform.position = this.FogVolumeGameObject.transform.position;
				this.ShadowProjector.transform.rotation = this.FogVolumeGameObject.transform.rotation;
				this.ShadowProjector.transform.localScale = this.fogVolumeScale;
				this.ShadowProjector.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
				this.ShadowProjector.GetComponent<MeshRenderer>().receiveShadows = false;
				this.ShadowProjector.GetComponent<MeshRenderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;
				this.ShadowProjector.GetComponent<MeshRenderer>().lightProbeUsage = LightProbeUsage.Off;
			}
			this.ShadowProjectorMeshFilter = this.ShadowProjector.GetComponent<MeshFilter>();
			this.ShadowProjectorMeshFilter.mesh = this.ShadowProjectorMesh;
			if (this.ShadowProjectorMesh == null)
			{
				GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
				this.ShadowProjectorMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
				Object.DestroyImmediate(gameObject, true);
			}
			if (this.ShadowProjector.GetComponent<MeshFilter>().sharedMesh == null)
			{
				this.ShadowProjector.GetComponent<MeshFilter>().mesh = this.ShadowProjectorMesh;
			}
			if (this.ShadowProjectorMesh == null)
			{
				MonoBehaviour.print("Missing mesh");
			}
			this.ShadowProjectorRenderer = this.ShadowProjector.GetComponent<MeshRenderer>();
			this.ShadowProjectorRenderer.lightProbeUsage = LightProbeUsage.Off;
			this.ShadowProjectorRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			this.ShadowProjectorRenderer.shadowCastingMode = ShadowCastingMode.Off;
			this.ShadowProjectorRenderer.receiveShadows = false;
			this.ShadowProjectorMaterial = this.ShadowProjectorRenderer.sharedMaterial;
			this.ShadowProjector.name = this.FogVolumeGameObject.name + " Shadow Projector";
			if (this.ShadowProjectorMaterial == null)
			{
				this.ShadowProjectorMaterial = new Material(Shader.Find("Fog Volume/Shadow Projector"));
				this.ShadowProjectorMaterial.name = "Shadow Projector Material";
			}
			this.ShadowProjectorRenderer.sharedMaterial = this.ShadowProjectorMaterial;
		}
	}

	// Token: 0x06000047 RID: 71 RVA: 0x00003888 File Offset: 0x00001A88
	private void SetShadowProyectorLayer()
	{
		if (this.ShadowProjector)
		{
			if (!this.RenderableInSceneView)
			{
				if (this.ShadowProjector.layer == LayerMask.NameToLayer("Default"))
				{
					this.ShadowProjector.layer = LayerMask.NameToLayer("UI");
					return;
				}
			}
			else if (this.ShadowProjector.layer == LayerMask.NameToLayer("UI"))
			{
				this.ShadowProjector.layer = LayerMask.NameToLayer("Default");
			}
		}
	}

	// Token: 0x06000048 RID: 72 RVA: 0x00003904 File Offset: 0x00001B04
	private void FindDirectionalLight()
	{
		Light[] array = Object.FindObjectsOfType<Light>();
		if (!this.Sun)
		{
			this.Sun = RenderSettings.sun;
		}
		if (!this.Sun)
		{
			foreach (Light light in array)
			{
				if (light.type == LightType.Directional)
				{
					this.Sun = light;
					break;
				}
			}
		}
		if (this.Sun == null)
		{
			Debug.LogError("Fog Volume: No valid light found\nDirectional light is required. Light component can be disabled.");
			return;
		}
		this.Sun.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
		this.Sun.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));
		this.Sun.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
	}

	// Token: 0x06000049 RID: 73 RVA: 0x000039E4 File Offset: 0x00001BE4
	public void FindFogVolumeData()
	{
		if (!(this._FogVolumeDataGO == null))
		{
			this._FogVolumeData = this._FogVolumeDataGO.GetComponent<FogVolumeData>();
			return;
		}
		FogVolumeData[] array = Object.FindObjectsOfType<FogVolumeData>();
		if (array.Length == 0)
		{
			this._FogVolumeDataGO = new GameObject();
			this._FogVolumeData = this._FogVolumeDataGO.AddComponent<FogVolumeData>();
			this._FogVolumeDataGO.name = "Fog Volume Data";
			return;
		}
		this._FogVolumeDataGO = array[0].gameObject;
		this._FogVolumeData = array[0];
	}

	// Token: 0x0600004A RID: 74 RVA: 0x00003A60 File Offset: 0x00001C60
	private void MoveToLayer()
	{
		if (!this.CastShadows && !this.ExcludeFromLowRes)
		{
			if (this._FogType == FogVolume.FogType.Textured)
			{
				if (this.FogVolumeGameObject.layer != LayerMask.NameToLayer("FogVolume"))
				{
					this.FogVolumeGameObject.layer = LayerMask.NameToLayer("FogVolume");
					return;
				}
			}
			else if (this.FogVolumeGameObject.layer != LayerMask.NameToLayer("FogVolumeUniform"))
			{
				this.FogVolumeGameObject.layer = LayerMask.NameToLayer("FogVolumeUniform");
			}
		}
	}

	// Token: 0x0600004B RID: 75 RVA: 0x00003AE0 File Offset: 0x00001CE0
	public void AssignCamera()
	{
		this.FindFogVolumeData();
		if (this._FogVolumeDataGO == null)
		{
			return;
		}
		if (this._FogVolumeData.GetFogVolumeCamera != null)
		{
			this.GameCameraGO = this._FogVolumeData.GetFogVolumeCamera.gameObject;
		}
		if (this.GameCameraGO == null)
		{
			base.enabled = false;
			return;
		}
		base.enabled = true;
		this.GameCamera = this.GameCameraGO.GetComponent<Camera>();
		this._FogVolumeRenderer = this.GameCameraGO.GetComponent<FogVolumeRenderer>();
		if (this._FogVolumeRenderer == null)
		{
			if (!this._FogVolumeData.ForceNoRenderer)
			{
				this._FogVolumeRenderer = this.GameCameraGO.AddComponent<FogVolumeRenderer>();
				this._FogVolumeRenderer.enabled = true;
				return;
			}
		}
		else if (!this._FogVolumeData.ForceNoRenderer)
		{
			this._FogVolumeRenderer.enabled = true;
		}
	}

	// Token: 0x0600004C RID: 76 RVA: 0x00003BBC File Offset: 0x00001DBC
	private void SetIcon()
	{
	}

	// Token: 0x0600004D RID: 77 RVA: 0x00003BC0 File Offset: 0x00001DC0
	private void OnEnable()
	{
		this.m_lightManager = null;
		this.m_primitiveManager = null;
		this.SetIcon();
		this.AssignCamera();
		this.SurrogateMaterial = Resources.Load("Fog Volume Surrogate", typeof(Material)) as Material;
		this.FindDirectionalLight();
		this._BoxCollider = base.GetComponent<BoxCollider>();
		if (this._BoxCollider == null)
		{
			this._BoxCollider = base.gameObject.AddComponent<BoxCollider>();
		}
		this._BoxCollider.hideFlags = HideFlags.HideAndDontSave;
		this._BoxCollider.isTrigger = true;
		this.FogVolumeGameObject = base.gameObject;
		this.FogRenderer = this.FogVolumeGameObject.GetComponent<MeshRenderer>();
		if (this.FogRenderer == null)
		{
			this.FogRenderer = this.FogVolumeGameObject.AddComponent<MeshRenderer>();
		}
		this.FogRenderer.sharedMaterial = this.FogMaterial;
		this.ToggleKeyword();
		if (this.FogRenderer.shadowCastingMode == ShadowCastingMode.On)
		{
			string text = this.FogVolumeGameObject.name + " Shadow Camera";
			this.ShadowCameraGO = GameObject.Find(text);
			if (this.ShadowCameraGO == null)
			{
				this.ShadowCameraGO = new GameObject(text);
				this.ShadowCameraGO.transform.parent = this.FogVolumeGameObject.transform;
				this.ShadowCameraGO.AddComponent<Camera>();
				Camera component = this.ShadowCameraGO.GetComponent<Camera>();
				component.orthographic = true;
				component.depth = -6f;
				component.clearFlags = CameraClearFlags.Color;
				component.backgroundColor = new Color(0f, 0f, 0f, 0f);
				component.allowHDR = false;
				component.allowMSAA = false;
				this._ShadowCamera = this.ShadowCameraGO.AddComponent<ShadowCamera>();
			}
			this.ShadowCameraGO.hideFlags = HideFlags.HideInHierarchy;
			this._ShadowCamera = this.ShadowCameraGO.GetComponent<ShadowCamera>();
			this.ShadowMapSetup();
		}
		if (this.filter == null)
		{
			this.filter = base.gameObject.GetComponent<MeshFilter>();
			this.CreateBoxMesh(base.transform.localScale);
			base.transform.localScale = Vector3.one;
		}
		this.filter.hideFlags = HideFlags.HideInInspector;
		this.UpdateBoxMesh();
		this._PerformanceLUT = Resources.Load("PerformanceLUT") as Texture2D;
		this.GetShadowMap();
		this._FogVolumeData.FindFogVolumes();
		if (this.FrustumPlanes == null)
		{
			this.FrustumPlanes = new Plane[6];
		}
		this._InitializeLightManagerIfNeccessary();
		this._InitializePrimitiveManagerIfNeccessary();
	}

	// Token: 0x0600004E RID: 78 RVA: 0x00003E38 File Offset: 0x00002038
	private float GetPointLightDistance2Camera(Vector3 lightPosition)
	{
		this.PointLightsCameraGO = Camera.current.gameObject;
		return (lightPosition - this.PointLightsCameraGO.transform.position).magnitude;
	}

	// Token: 0x0600004F RID: 79 RVA: 0x00003E74 File Offset: 0x00002074
	private bool PointIsVisible(Vector3 point)
	{
		this.PointLightsCamera = this.PointLightsCameraGO.GetComponent<Camera>();
		float num = 0f - this.PointLightScreenMargin;
		float num2 = 1f + this.PointLightScreenMargin;
		Vector3 vector = this.PointLightsCamera.WorldToViewportPoint(point);
		return vector.z > num && vector.x > num && vector.x < num2 && vector.y > num && vector.y < num2;
	}

	// Token: 0x06000050 RID: 80 RVA: 0x00003EF0 File Offset: 0x000020F0
	public bool PointIsInsideVolume(Vector3 PointPosition)
	{
		bool flag = false;
		float num = base.gameObject.transform.position.x + this.fogVolumeScale.x / 2f;
		float num2 = base.gameObject.transform.position.x - this.fogVolumeScale.x / 2f;
		float num3 = base.gameObject.transform.position.y + this.fogVolumeScale.y / 2f;
		float num4 = base.gameObject.transform.position.y - this.fogVolumeScale.y / 2f;
		float num5 = base.gameObject.transform.position.z + this.fogVolumeScale.z / 2f;
		float num6 = base.gameObject.transform.position.z - this.fogVolumeScale.z / 2f;
		if (num > PointPosition.x && num2 < PointPosition.x && num3 > PointPosition.y && num4 < PointPosition.y && num5 > PointPosition.z && num6 < PointPosition.z)
		{
			flag = true;
		}
		return flag;
	}

	// Token: 0x06000051 RID: 81 RVA: 0x0000402A File Offset: 0x0000222A
	private Vector3 LocalDirectionalSunLight(Light Sun)
	{
		return base.transform.InverseTransformVector(-Sun.transform.forward);
	}

	// Token: 0x06000052 RID: 82 RVA: 0x00004047 File Offset: 0x00002247
	private void OnDisable()
	{
		this.m_lightManager = null;
		this.m_primitiveManager = null;
	}

	// Token: 0x06000053 RID: 83 RVA: 0x00004057 File Offset: 0x00002257
	public static void Wireframe(GameObject obj, bool Enable)
	{
	}

	// Token: 0x06000054 RID: 84 RVA: 0x00004059 File Offset: 0x00002259
	private void OnBecameVisible()
	{
	}

	// Token: 0x06000055 RID: 85 RVA: 0x0000405C File Offset: 0x0000225C
	private void GetShadowMap()
	{
		if (!this.FogRenderer)
		{
			this.FogRenderer = this.FogVolumeGameObject.GetComponent<MeshRenderer>();
		}
		if (this.IsVisible)
		{
			if (this.FogRenderer != null && this.FogRenderer.shadowCastingMode == ShadowCastingMode.Off)
			{
				this.CastShadows = false;
			}
			if (this.FogRenderer.shadowCastingMode == ShadowCastingMode.On)
			{
				if (this._FogType == FogVolume.FogType.Textured)
				{
					this.CastShadows = true;
				}
				else
				{
					this.FogRenderer.shadowCastingMode = ShadowCastingMode.Off;
				}
			}
			if (this.CastShadows && this._ShadowCamera == null)
			{
				this.ShadowMapSetup();
				this.RT_OpacityBlur = this._ShadowCamera.GetOpacityBlurRT();
			}
			if (this._ShadowCamera)
			{
				this.ShadowCameraGO.SetActive(this.CastShadows);
			}
			if (this.CastShadows)
			{
				this.RT_Opacity = this._ShadowCamera.GetOpacityRT();
			}
			else if (this.ShadowCaster && this.FogRenderer.receiveShadows)
			{
				this.RT_Opacity = this.ShadowCaster.RT_Opacity;
				this.RT_OpacityBlur = this.ShadowCaster.RT_OpacityBlur;
				this.fogVolumeScale.x = this.ShadowCaster.fogVolumeScale.x;
				this.fogVolumeScale.z = this.fogVolumeScale.x;
			}
			if (this.CastShadows)
			{
				this.FogVolumeGameObject.layer = LayerMask.NameToLayer("FogVolumeShadowCaster");
			}
			if (this.CastShadows && this.ShadowCaster)
			{
				this.FogMaterial.DisableKeyword("VOLUME_SHADOWS");
			}
			if ((this.ShadowCaster && this.ShadowCaster.CastShadows) || this.FogRenderer.receiveShadows)
			{
				this.FogMaterial.EnableKeyword("VOLUME_SHADOWS");
			}
			if ((this.ShadowCaster && !this.ShadowCaster.CastShadows) || !this.FogRenderer.receiveShadows)
			{
				this.FogMaterial.DisableKeyword("VOLUME_SHADOWS");
			}
		}
	}

	// Token: 0x06000056 RID: 86 RVA: 0x00004264 File Offset: 0x00002464
	private void Update()
	{
		if (this.InjectCustomDepthBuffer && !this.FogMaterial.IsKeywordEnabled("ExternalDepth") && this.ExcludeFromLowRes)
		{
			this.FogMaterial.EnableKeyword("ExternalDepth");
		}
		if (this.FogMaterial.IsKeywordEnabled("ExternalDepth") && this.ExcludeFromLowRes && !this.InjectCustomDepthBuffer)
		{
			this.FogMaterial.DisableKeyword("ExternalDepth");
		}
		if (this.GameCamera == null)
		{
			this.AssignCamera();
		}
		if (this.PointLightCullSizeMultiplier < 1f)
		{
			this.PointLightCullSizeMultiplier = 1f;
		}
		if (this.m_lightManager != null)
		{
			this.m_lightManager.DrawDebugData = this.ShowDebugGizmos;
			this.m_lightManager.SetPointLightCullSizeMultiplier(this.PointLightCullSizeMultiplier);
		}
		if (this.m_primitiveManager != null)
		{
			this.m_primitiveManager.SetVisibility(this.ShowPrimitives);
		}
		if (this.GameCamera != null)
		{
			this.FrustumPlanes = GeometryUtility.CalculateFrustumPlanes(this.GameCamera);
			if (Application.isPlaying)
			{
				if (GeometryUtility.TestPlanesAABB(this.FrustumPlanes, this._BoxCollider.bounds))
				{
					this.IsVisible = true;
				}
				else
				{
					this.IsVisible = false;
				}
			}
			else
			{
				this.IsVisible = true;
			}
		}
		if (this.EnableNoise || this.EnableGradient)
		{
			this._FogType = FogVolume.FogType.Textured;
		}
		else
		{
			this._FogType = FogVolume.FogType.Uniform;
		}
		this.UpdateBoxMesh();
		this.ShadowProjectorLock();
		this.RenderSurrogate();
		if (this.ShadowProjector != null && this.CastShadows)
		{
			this.ShadowProjectorMaterial.SetColor("_ShadowColor", this.ShadowColor);
		}
		if ((this.FogMaterial.GetTexture("_NoiseVolume") == null && this._NoiseVolume != null) || this.FogMaterial.GetTexture("_NoiseVolume") != this._NoiseVolume)
		{
			this.FogMaterial.SetTexture("_NoiseVolume", this._NoiseVolume);
		}
		if ((this.FogMaterial.GetTexture("_NoiseVolume2") == null && this._NoiseVolume2 != null) || this.FogMaterial.GetTexture("_NoiseVolume2") != this._NoiseVolume2)
		{
			this.FogMaterial.SetTexture("_NoiseVolume2", this._NoiseVolume2);
		}
		if ((this.FogMaterial.GetTexture("CoverageTex") == null && this.CoverageTex != null) || this.FogMaterial.GetTexture("CoverageTex") != this.CoverageTex)
		{
			this.FogMaterial.SetTexture("CoverageTex", this.CoverageTex);
		}
	}

	// Token: 0x06000057 RID: 87 RVA: 0x00004510 File Offset: 0x00002710
	public void RenderSurrogate()
	{
		if (this.IsVisible)
		{
			if (this.GameCameraGO == null)
			{
				this.AssignCamera();
			}
			FogVolumeRenderer fogVolumeRenderer = this.GameCameraGO.GetComponent<FogVolumeRenderer>();
			if (fogVolumeRenderer == null && !this._FogVolumeData.ForceNoRenderer)
			{
				fogVolumeRenderer = this.GameCameraGO.AddComponent<FogVolumeRenderer>();
				fogVolumeRenderer.enabled = true;
			}
			if (!this._FogVolumeData.ForceNoRenderer && fogVolumeRenderer._Downsample > 0 && this.CreateSurrogate && this.mesh != null && this._FogType == FogVolume.FogType.Textured)
			{
				Graphics.DrawMesh(this.mesh, base.transform.position, base.transform.rotation, this.SurrogateMaterial, LayerMask.NameToLayer("FogVolumeSurrogate"), null, 0, null, false, false, false);
			}
		}
	}

	// Token: 0x06000058 RID: 88 RVA: 0x000045E0 File Offset: 0x000027E0
	private void UpdateParams()
	{
		if (this.IsVisible)
		{
			if (this._PerformanceLUT && this._DebugMode == FogVolume.DebugMode.Iterations)
			{
				this.FogMaterial.SetTexture("_PerformanceLUT", this._PerformanceLUT);
			}
			if (this._DebugMode != FogVolume.DebugMode.Lit)
			{
				this.FogMaterial.SetInt("_DebugMode", (int)this._DebugMode);
			}
			if (FogVolume.FogType.Textured == this._FogType)
			{
				this.FogMaterial.SetInt("_SrcBlend", (int)this._BlendMode);
			}
			else
			{
				this.FogMaterial.SetInt("_SrcBlend", 5);
			}
			this.FogMaterial.SetInt("_ztest", (int)this._ztest);
			if (this.m_lightManager != null && this.m_lightManager.CurrentLightCount > 0 && this.PointLightsActive && SystemInfo.graphicsShaderLevel > 30)
			{
				this.FogMaterial.SetVectorArray("_LightPositions", this.m_lightManager.GetLightPositionArray());
				this.FogMaterial.SetVectorArray("_LightRotations", this.m_lightManager.GetLightRotationArray());
				this.FogMaterial.SetColorArray("_LightColors", this.m_lightManager.GetLightColorArray());
				this.FogMaterial.SetVectorArray("_LightData", this.m_lightManager.GetLightData());
				this.FogMaterial.SetFloat("PointLightingDistance", 1f / this.PointLightingDistance);
				this.FogMaterial.SetFloat("PointLightingDistance2Camera", 1f / this.PointLightingDistance2Camera);
			}
			if (this.m_primitiveManager != null && this.m_primitiveManager.CurrentPrimitiveCount > 0 && this.EnableDistanceFields)
			{
				this.FogMaterial.SetFloat("Constrain", this.Constrain);
				this.FogMaterial.SetVectorArray("_PrimitivePosition", this.m_primitiveManager.GetPrimitivePositionArray());
				this.FogMaterial.SetVectorArray("_PrimitiveScale", this.m_primitiveManager.GetPrimitiveScaleArray());
				this.FogMaterial.SetInt("_PrimitiveCount", this.m_primitiveManager.VisiblePrimitiveCount);
				this.FogMaterial.SetMatrixArray("_PrimitivesTransform", this.m_primitiveManager.GetPrimitiveTransformArray());
				this.FogMaterial.SetFloat("_PrimitiveEdgeSoftener", 1f / this._PrimitiveEdgeSoftener);
				this.FogMaterial.SetFloat("_PrimitiveCutout", this._PrimitiveCutout);
				this.FogMaterial.SetVectorArray("_PrimitiveData", this.m_primitiveManager.GetPrimitiveDataArray());
			}
			if (this.Sun && this.Sun.enabled)
			{
				this.FogMaterial.SetFloat("_LightExposure", this._LightExposure);
				if (this.LightHalo && this._LightHaloTexture)
				{
					this.FogMaterial.SetTexture("_LightHaloTexture", this._LightHaloTexture);
					this.FogMaterial.SetFloat("_HaloOpticalDispersion", this._HaloOpticalDispersion);
					this.FogMaterial.SetFloat("_HaloWidth", this._HaloWidth.Remap(0f, 1f, 10f, 1f));
					this.FogMaterial.SetFloat("_HaloIntensity", this._HaloIntensity * 2000f);
					this.FogMaterial.SetFloat("_HaloRadius", this._HaloRadius.Remap(0f, 1f, 2f, 0.1f));
					this.FogMaterial.SetFloat("_HaloAbsorption", this._HaloAbsorption.Remap(0f, 1f, 0f, 16f));
				}
				this.FogMaterial.SetVector("L", -this.Sun.transform.forward);
				Shader.SetGlobalVector("L", -this.Sun.transform.forward);
				this.FogMaterial.SetVector("_LightLocalDirection", this.LocalDirectionalSunLight(this.Sun));
				if (this.EnableInscattering)
				{
					this.FogMaterial.SetFloat("_InscatteringIntensity", this.InscatteringIntensity * 50f);
					this.FogMaterial.SetFloat("InscatteringShape", this.InscatteringShape);
					this.FogMaterial.SetFloat("InscatteringTransitionWideness", this.InscatteringTransitionWideness);
					this.FogMaterial.SetColor("_InscatteringColor", this.InscatteringColor);
				}
				if (this.VolumeFogInscattering)
				{
					this.FogMaterial.SetFloat("VolumeFogInscatteringIntensity", this.VolumeFogInscatteringIntensity * 50f);
					this.FogMaterial.SetFloat("VolumeFogInscatteringAnisotropy", this.VolumeFogInscatteringAnisotropy);
					this.FogMaterial.SetColor("VolumeFogInscatteringColor", this.VolumeFogInscatteringColor);
					this.VolumeFogInscatteringStartDistance = Mathf.Max(0f, this.VolumeFogInscatteringStartDistance);
					this.FogMaterial.SetFloat("VolumeFogInscatteringStartDistance", this.VolumeFogInscatteringStartDistance);
					this.VolumeFogInscatteringTransitionWideness = Mathf.Max(0.01f, this.VolumeFogInscatteringTransitionWideness);
					this.FogMaterial.SetFloat("VolumeFogInscatteringTransitionWideness", this.VolumeFogInscatteringTransitionWideness);
				}
				this.FogMaterial.SetColor("_LightColor", this.Sun.color * this.Sun.intensity);
				if (this.EnableNoise && this._NoiseVolume != null)
				{
					this.FogMaterial.SetFloat("_NoiseDetailRange", this._NoiseDetailRange * 1f);
					this.FogMaterial.SetFloat("_Curl", this._Curl);
					if (this._DirectionalLighting)
					{
						this.FogMaterial.SetFloat("_DirectionalLightingDistance", this._DirectionalLightingDistance);
						this.FogMaterial.SetInt("DirectLightingShadowSteps", this.DirectLightingShadowSteps);
						this.FogMaterial.SetFloat("DirectLightingShadowDensity", this.DirectLightingShadowDensity);
					}
				}
			}
			this.FogMaterial.SetFloat("_Cutoff", this.ShadowCutoff);
			this.FogMaterial.SetFloat("Absorption", this.Absorption);
			this.LightExtinctionColor.r = Mathf.Max(0.1f, this.LightExtinctionColor.r);
			this.LightExtinctionColor.g = Mathf.Max(0.1f, this.LightExtinctionColor.g);
			this.LightExtinctionColor.b = Mathf.Max(0.1f, this.LightExtinctionColor.b);
			this.FogMaterial.SetColor("LightExtinctionColor", this.LightExtinctionColor);
			if (this.Vortex > 0f)
			{
				this.FogMaterial.SetFloat("_Vortex", this.Vortex);
				this.FogMaterial.SetFloat("_Rotation", 0.017453292f * this.rotation);
				this.FogMaterial.SetFloat("_RotationSpeed", -this.RotationSpeed);
			}
			this.DetailDistance = Mathf.Max(1f, this.DetailDistance);
			this.FogMaterial.SetFloat("DetailDistance", this.DetailDistance);
			this.FogMaterial.SetFloat("Octaves", (float)this.Octaves);
			this.FogMaterial.SetFloat("_DetailMaskingThreshold", this._DetailMaskingThreshold);
			this.FogMaterial.SetVector("_VolumePosition", base.gameObject.transform.position);
			this.FogMaterial.SetFloat("gain", this.NoiseIntensity);
			this.FogMaterial.SetFloat("threshold", this.NoiseContrast * 0.5f - 5f);
			this.FogMaterial.SetFloat("_3DNoiseScale", this._3DNoiseScale * 0.001f);
			this.FadeDistance = Mathf.Max(1f, this.FadeDistance);
			this.FogMaterial.SetFloat("FadeDistance", this.FadeDistance);
			this.FogMaterial.SetFloat("STEP_COUNT", (float)this.Iterations);
			this.FogMaterial.SetFloat("DetailTiling", this.DetailTiling);
			this.FogMaterial.SetFloat("BaseTiling", this.BaseTiling * 0.1f);
			this.FogMaterial.SetFloat("Coverage", this.Coverage);
			this.FogMaterial.SetFloat("NoiseDensity", this.NoiseDensity);
			this.FogMaterial.SetVector("Speed", this.Speed * 0.1f);
			this.FogMaterial.SetVector("Stretch", new Vector4(1f, 1f, 1f, 1f) + this.Stretch * 0.01f);
			if (this.useHeightGradient)
			{
				this.FogMaterial.SetVector("_VerticalGradientParams", new Vector4(this.GradMin, this.GradMax, this.GradMin2, this.GradMax2));
			}
			this.FogMaterial.SetColor("_AmbientColor", this._AmbientColor);
			if (this.FogRenderer.lightProbeUsage == LightProbeUsage.UseProxyVolume)
			{
				this._ProxyVolume = true;
			}
			else
			{
				this._ProxyVolume = false;
			}
			this.FogMaterial.SetFloat("_ProxyVolume", (float)((!this._ProxyVolume) ? 0 : 1));
			this.FogMaterial.SetFloat("ShadowBrightness", this.ShadowBrightness);
			this.FogMaterial.SetFloat("_DetailRelativeSpeed", this._DetailRelativeSpeed);
			this.FogMaterial.SetFloat("_BaseRelativeSpeed", this._BaseRelativeSpeed);
			if (this.bSphericalFade)
			{
				this.SphericalFadeDistance = Mathf.Max(1f, this.SphericalFadeDistance);
				this.FogMaterial.SetFloat("SphericalFadeDistance", this.SphericalFadeDistance);
			}
			this.FogMaterial.SetVector("VolumeSize", new Vector4(this.fogVolumeScale.x, this.fogVolumeScale.y, this.fogVolumeScale.z, 0f));
			this.FogMaterial.SetFloat("Exposure", Mathf.Max(0f, this.Exposure));
			this.FogMaterial.SetFloat("Offset", this.Offset);
			this.FogMaterial.SetFloat("Gamma", this.Gamma);
			if (this.Gradient != null)
			{
				this.FogMaterial.SetTexture("_Gradient", this.Gradient);
			}
			this.FogMaterial.SetFloat("InscatteringStartDistance", this.InscatteringStartDistance);
			Vector3 vector = this.currentFogVolume;
			this.FogMaterial.SetVector("_BoxMin", vector * -0.5f);
			this.FogMaterial.SetVector("_BoxMax", vector * 0.5f);
			this.FogMaterial.SetColor("_Color", this.FogMainColor);
			this.FogMaterial.SetColor("_FogColor", this._FogColor);
			this.FogMaterial.SetInt("_AmbientAffectsFogColor", this._AmbientAffectsFogColor ? 1 : 0);
			this.FogMaterial.SetFloat("_SceneIntersectionSoftness", this._SceneIntersectionSoftness);
			this.FogMaterial.SetFloat("_RayStep", this.IterationStep * 0.001f);
			this.FogMaterial.SetFloat("_OptimizationFactor", this._OptimizationFactor);
			this.FogMaterial.SetFloat("_Visibility", ((this.bVolumeFog && this.EnableNoise && this._NoiseVolume) || this.EnableGradient) ? (this.Visibility * 100f) : this.Visibility);
			this.FogRenderer.sortingOrder = this.DrawOrder;
			this.FogMaterial.SetInt("VolumeFogInscatterColorAffectedWithFogColor", this.VolumeFogInscatterColorAffectedWithFogColor ? 1 : 0);
			this.FogMaterial.SetFloat("_FOV", this.GameCamera.fieldOfView);
			this.FogMaterial.SetFloat("HeightAbsorption", this.HeightAbsorption);
			this.FogMaterial.SetVector("_AmbientHeightAbsorption", new Vector4(this.HeightAbsorptionMin, this.HeightAbsorptionMax, this.HeightAbsorption, 0f));
			this.FogMaterial.SetFloat("NormalDistance", this.NormalDistance);
			this.FogMaterial.SetFloat("DirectLightingAmount", this.DirectLightingAmount);
			this.DirectLightingDistance = Mathf.Max(1f, this.DirectLightingDistance);
			this.FogMaterial.SetFloat("DirectLightingDistance", this.DirectLightingDistance);
			this.FogMaterial.SetFloat("LambertianBias", this.LambertianBias);
			this.FogMaterial.SetFloat("_jitter", this._jitter);
			this.FogMaterial.SetInt("SamplingMethod", (int)this._SamplingMethod);
			this.FogMaterial.SetFloat("_PushAlpha", this._PushAlpha);
			if (this._ShadeNoise && this.EnableNoise)
			{
				this.FogMaterial.SetColor("_SelfShadowColor", this._SelfShadowColor);
				this.FogMaterial.SetInt("_SelfShadowSteps", this._SelfShadowSteps);
				this.FogMaterial.SetFloat("ShadowShift", this.ShadowShift);
			}
		}
	}

	// Token: 0x06000059 RID: 89 RVA: 0x000052FB File Offset: 0x000034FB
	private void LateUpdate()
	{
	}

	// Token: 0x0600005A RID: 90 RVA: 0x00005300 File Offset: 0x00003500
	private void OnWillRenderObject()
	{
		this.GetShadowMap();
		this.SurrogateMaterial.DisableKeyword("_EDITOR_WINDOW");
		if (this.PointLightsActive && SystemInfo.graphicsShaderLevel > 30)
		{
			this._InitializeLightManagerIfNeccessary();
			if (this.m_lightManager != null)
			{
				if (this.PointLightsRealTimeUpdate)
				{
					this.m_lightManager.Deinitialize();
					if (this.PointLightBoxCheck)
					{
						this.m_lightManager.FindLightsInFogVolume();
					}
					else
					{
						this.m_lightManager.FindLightsInScene();
					}
				}
				if (!this.m_lightManager.AlreadyUsesTransformForPoI)
				{
					this.m_lightManager.SetPointOfInterest(this._FogVolumeData.GameCamera.transform);
				}
				this.m_lightManager.ManualUpdate(ref this.FrustumPlanes);
			}
			this.FogMaterial.SetInt("_LightsCount", this.GetVisibleLightCount());
		}
		else
		{
			this._DeinitializeLightManagerIfNeccessary();
		}
		if (this.EnableDistanceFields)
		{
			this._InitializePrimitiveManagerIfNeccessary();
			if (this.m_primitiveManager != null)
			{
				if (this.PrimitivesRealTimeUpdate)
				{
					this.m_primitiveManager.Deinitialize();
					this.m_primitiveManager.FindPrimitivesInFogVolume();
				}
				if (!this.m_primitiveManager.AlreadyUsesTransformForPoI)
				{
					this.m_primitiveManager.SetPointOfInterest(this._FogVolumeData.GameCamera.transform);
				}
				this.m_primitiveManager.ManualUpdate(ref this.FrustumPlanes);
			}
			this.FogMaterial.SetInt("_PrimitivesCount", this.GetVisiblePrimitiveCount());
		}
		else
		{
			this._DeinitializePrimitiveManagerIfNeccessary();
		}
		if (this.RT_Opacity != null)
		{
			Shader.SetGlobalTexture("RT_Opacity", this.RT_Opacity);
			if (this.UseConvolvedLightshafts)
			{
				this.FogMaterial.SetTexture("LightshaftTex", this.RT_OpacityBlur);
			}
			else
			{
				this.FogMaterial.SetTexture("LightshaftTex", this.RT_Opacity);
			}
		}
		this.UpdateParams();
		if (!this.RenderableInSceneView && Camera.current.name == "SceneCamera")
		{
			this.FogMaterial.SetVector("_BoxMin", new Vector3(0f, 0f, 0f));
			this.FogMaterial.SetVector("_BoxMax", new Vector3(0f, 0f, 0f));
			return;
		}
		this.FogMaterial.SetVector("_BoxMin", this.currentFogVolume * -0.5f);
		this.FogMaterial.SetVector("_BoxMax", this.currentFogVolume * 0.5f);
	}

	// Token: 0x0600005B RID: 91 RVA: 0x00005584 File Offset: 0x00003784
	private void ToggleKeyword()
	{
		if (this.IsVisible)
		{
			if (this._jitter > 0f)
			{
				this.FogMaterial.EnableKeyword("JITTER");
			}
			else
			{
				this.FogMaterial.DisableKeyword("JITTER");
			}
			if (this._DebugMode != FogVolume.DebugMode.Lit)
			{
				this.FogMaterial.EnableKeyword("DEBUG");
			}
			else
			{
				this.FogMaterial.DisableKeyword("DEBUG");
			}
			FogVolume.SamplingMethod samplingMethod = this._SamplingMethod;
			if (samplingMethod != FogVolume.SamplingMethod.Eye2Box)
			{
				if (samplingMethod == FogVolume.SamplingMethod.ViewAligned)
				{
					this.FogMaterial.EnableKeyword("SAMPLING_METHOD_ViewAligned");
					this.FogMaterial.DisableKeyword("SAMPLING_METHOD_Eye2Box");
				}
			}
			else
			{
				this.FogMaterial.DisableKeyword("SAMPLING_METHOD_ViewAligned");
				this.FogMaterial.EnableKeyword("SAMPLING_METHOD_Eye2Box");
			}
			if (this.LightHalo && this._LightHaloTexture)
			{
				this.FogMaterial.EnableKeyword("HALO");
			}
			else
			{
				this.FogMaterial.DisableKeyword("HALO");
			}
			if (this.ShadowCaster != null)
			{
				if (this.ShadowCaster.SunAttached)
				{
					this.FogMaterial.EnableKeyword("LIGHT_ATTACHED");
				}
				if (!this.ShadowCaster.SunAttached)
				{
					this.FogMaterial.DisableKeyword("LIGHT_ATTACHED");
				}
			}
			if (this.Vortex > 0f)
			{
				this.FogMaterial.EnableKeyword("Twirl");
				switch (this._VortexAxis)
				{
				case FogVolume.VortexAxis.X:
					this.FogMaterial.EnableKeyword("Twirl_X");
					this.FogMaterial.DisableKeyword("Twirl_Y");
					this.FogMaterial.DisableKeyword("Twirl_Z");
					break;
				case FogVolume.VortexAxis.Y:
					this.FogMaterial.DisableKeyword("Twirl_X");
					this.FogMaterial.EnableKeyword("Twirl_Y");
					this.FogMaterial.DisableKeyword("Twirl_Z");
					break;
				case FogVolume.VortexAxis.Z:
					this.FogMaterial.DisableKeyword("Twirl_X");
					this.FogMaterial.DisableKeyword("Twirl_Y");
					this.FogMaterial.EnableKeyword("Twirl_Z");
					break;
				}
			}
			else
			{
				this.FogMaterial.DisableKeyword("Twirl_X");
				this.FogMaterial.DisableKeyword("Twirl_Y");
				this.FogMaterial.DisableKeyword("Twirl_Z");
			}
			if (this.Lambert && this.Sun && this.EnableNoise)
			{
				this.FogMaterial.EnableKeyword("_LAMBERT_SHADING");
			}
			else
			{
				this.FogMaterial.DisableKeyword("_LAMBERT_SHADING");
			}
			if (this.PointLightsActive && SystemInfo.graphicsShaderLevel > 30)
			{
				switch (this._LightScatterMethod)
				{
				case FogVolume.LightScatterMethod.BeersLaw:
					this.FogMaterial.EnableKeyword("ATTEN_METHOD_1");
					this.FogMaterial.DisableKeyword("ATTEN_METHOD_2");
					this.FogMaterial.DisableKeyword("ATTEN_METHOD_3");
					break;
				case FogVolume.LightScatterMethod.InverseSquare:
					this.FogMaterial.DisableKeyword("ATTEN_METHOD_1");
					this.FogMaterial.EnableKeyword("ATTEN_METHOD_2");
					this.FogMaterial.DisableKeyword("ATTEN_METHOD_3");
					break;
				case FogVolume.LightScatterMethod.Linear:
					this.FogMaterial.DisableKeyword("ATTEN_METHOD_1");
					this.FogMaterial.DisableKeyword("ATTEN_METHOD_2");
					this.FogMaterial.EnableKeyword("ATTEN_METHOD_3");
					break;
				}
			}
			else
			{
				this.FogMaterial.DisableKeyword("ATTEN_METHOD_1");
				this.FogMaterial.DisableKeyword("ATTEN_METHOD_2");
				this.FogMaterial.DisableKeyword("ATTEN_METHOD_3");
			}
			if (this.EnableNoise && this._NoiseVolume && this.useHeightGradient)
			{
				this.FogMaterial.EnableKeyword("HEIGHT_GRAD");
			}
			else
			{
				this.FogMaterial.DisableKeyword("HEIGHT_GRAD");
			}
			if (this.ColorAdjust)
			{
				this.FogMaterial.EnableKeyword("ColorAdjust");
			}
			else
			{
				this.FogMaterial.DisableKeyword("ColorAdjust");
			}
			if (this.bVolumeFog)
			{
				this.FogMaterial.EnableKeyword("VOLUME_FOG");
			}
			else
			{
				this.FogMaterial.DisableKeyword("VOLUME_FOG");
			}
			if (this.FogMaterial)
			{
				if (this.EnableGradient && this.Gradient != null)
				{
					this.FogMaterial.EnableKeyword("_FOG_GRADIENT");
				}
				else
				{
					this.FogMaterial.DisableKeyword("_FOG_GRADIENT");
				}
				if (this.EnableNoise)
				{
					this.FogMaterial.EnableKeyword("_FOG_VOLUME_NOISE");
				}
				else
				{
					this.FogMaterial.DisableKeyword("_FOG_VOLUME_NOISE");
				}
				if (this.EnableInscattering && this.Sun && this.Sun.enabled && this.Sun.isActiveAndEnabled)
				{
					this.FogMaterial.EnableKeyword("_INSCATTERING");
				}
				else
				{
					this.FogMaterial.DisableKeyword("_INSCATTERING");
				}
				if (this.VolumeFogInscattering && this.Sun && this.Sun.enabled && this.bVolumeFog)
				{
					this.FogMaterial.EnableKeyword("_VOLUME_FOG_INSCATTERING");
				}
				else
				{
					this.FogMaterial.DisableKeyword("_VOLUME_FOG_INSCATTERING");
				}
				this.FogMaterial.SetFloat("Collisions", (float)(this.SceneCollision ? 1 : 0));
				if (this.ShadowShift > 0f && this.EnableNoise && this.Sun && this._ShadeNoise)
				{
					this.FogMaterial.EnableKeyword("_SHADE");
				}
				else
				{
					this.FogMaterial.DisableKeyword("_SHADE");
				}
				if (this.Tonemap)
				{
					this.FogMaterial.EnableKeyword("_TONEMAP");
				}
				else
				{
					this.FogMaterial.DisableKeyword("_TONEMAP");
				}
				if (this.bSphericalFade)
				{
					this.FogMaterial.EnableKeyword("SPHERICAL_FADE");
				}
				else
				{
					this.FogMaterial.DisableKeyword("SPHERICAL_FADE");
				}
				if (this.EnableDistanceFields)
				{
					this.FogMaterial.EnableKeyword("DF");
				}
				else
				{
					this.FogMaterial.DisableKeyword("DF");
				}
				if (this.bAbsorption)
				{
					this.FogMaterial.EnableKeyword("ABSORPTION");
				}
				else
				{
					this.FogMaterial.DisableKeyword("ABSORPTION");
				}
				if (this._DirectionalLighting && this.EnableNoise && this._NoiseVolume != null && this._DirectionalLightingDistance > 0f && this.DirectLightingShadowDensity > 0f)
				{
					this.FogMaterial.EnableKeyword("DIRECTIONAL_LIGHTING");
					return;
				}
				this.FogMaterial.DisableKeyword("DIRECTIONAL_LIGHTING");
			}
		}
	}

	// Token: 0x0600005C RID: 92 RVA: 0x00005C18 File Offset: 0x00003E18
	public void UpdateBoxMesh()
	{
		if (this.currentFogVolume != this.fogVolumeScale || this.filter == null)
		{
			this.CreateBoxMesh(this.fogVolumeScale);
			this.ShadowMapSetup();
			this._BoxCollider.size = this.fogVolumeScale;
			this.m_hasUpdatedBoxMesh = true;
		}
		base.transform.localScale = Vector3.one;
	}

	// Token: 0x1700000E RID: 14
	// (get) Token: 0x0600005D RID: 93 RVA: 0x00005C80 File Offset: 0x00003E80
	public bool HasUpdatedBoxMesh
	{
		get
		{
			bool hasUpdatedBoxMesh = this.m_hasUpdatedBoxMesh;
			this.m_hasUpdatedBoxMesh = false;
			return hasUpdatedBoxMesh;
		}
	}

	// Token: 0x0600005E RID: 94 RVA: 0x00005C90 File Offset: 0x00003E90
	private void CreateBoxMesh(Vector3 scale)
	{
		this.currentFogVolume = scale;
		if (this.filter == null)
		{
			this.filter = base.gameObject.AddComponent<MeshFilter>();
		}
		if (this.mesh == null)
		{
			this.mesh = new Mesh();
			this.mesh.name = base.gameObject.name;
			this.filter.sharedMesh = this.mesh;
		}
		this.mesh.Clear();
		float y = scale.y;
		float z = scale.z;
		float x = scale.x;
		Vector3 vector = new Vector3(-x * 0.5f, -y * 0.5f, z * 0.5f);
		Vector3 vector2 = new Vector3(x * 0.5f, -y * 0.5f, z * 0.5f);
		Vector3 vector3 = new Vector3(x * 0.5f, -y * 0.5f, -z * 0.5f);
		Vector3 vector4 = new Vector3(-x * 0.5f, -y * 0.5f, -z * 0.5f);
		Vector3 vector5 = new Vector3(-x * 0.5f, y * 0.5f, z * 0.5f);
		Vector3 vector6 = new Vector3(x * 0.5f, y * 0.5f, z * 0.5f);
		Vector3 vector7 = new Vector3(x * 0.5f, y * 0.5f, -z * 0.5f);
		Vector3 vector8 = new Vector3(-x * 0.5f, y * 0.5f, -z * 0.5f);
		Vector3[] array = new Vector3[]
		{
			vector, vector2, vector3, vector4, vector8, vector5, vector, vector4, vector5, vector6,
			vector2, vector, vector7, vector8, vector4, vector3, vector6, vector7, vector3, vector2,
			vector8, vector7, vector6, vector5
		};
		Vector3 up = Vector3.up;
		Vector3 down = Vector3.down;
		Vector3 forward = Vector3.forward;
		Vector3 back = Vector3.back;
		Vector3 left = Vector3.left;
		Vector3 right = Vector3.right;
		Vector3[] array2 = new Vector3[]
		{
			down, down, down, down, left, left, left, left, forward, forward,
			forward, forward, back, back, back, back, right, right, right, right,
			up, up, up, up
		};
		Vector2 vector9 = new Vector2(0f, 0f);
		Vector2 vector10 = new Vector2(1f, 0f);
		Vector2 vector11 = new Vector2(0f, 1f);
		Vector2 vector12 = new Vector2(1f, 1f);
		Vector2[] array3 = new Vector2[]
		{
			vector12, vector11, vector9, vector10, vector12, vector11, vector9, vector10, vector12, vector11,
			vector9, vector10, vector12, vector11, vector9, vector10, vector12, vector11, vector9, vector10,
			vector12, vector11, vector9, vector10
		};
		int[] array4 = new int[]
		{
			3, 1, 0, 3, 2, 1, 7, 5, 4, 7,
			6, 5, 11, 9, 8, 11, 10, 9, 15, 13,
			12, 15, 14, 13, 19, 17, 16, 19, 18, 17,
			23, 21, 20, 23, 22, 21
		};
		this.mesh.vertices = array;
		this.mesh.triangles = array4;
		this.mesh.normals = array2;
		this.mesh.uv = array3;
		this.mesh.RecalculateBounds();
	}

	// Token: 0x0600005F RID: 95 RVA: 0x000061A0 File Offset: 0x000043A0
	private void _InitializeLightManagerIfNeccessary()
	{
		if (this.m_lightManager == null)
		{
			this.m_lightManager = base.GetComponent<FogVolumeLightManager>();
			if (this.m_lightManager == null)
			{
				this.m_lightManager = base.gameObject.AddComponent<FogVolumeLightManager>();
			}
			this.m_lightManager.Initialize();
			if (this.PointLightBoxCheck)
			{
				this.m_lightManager.FindLightsInFogVolume();
				return;
			}
			this.m_lightManager.FindLightsInScene();
		}
	}

	// Token: 0x06000060 RID: 96 RVA: 0x00006210 File Offset: 0x00004410
	private void _DeinitializeLightManagerIfNeccessary()
	{
		if (this.m_lightManager != null)
		{
			this.m_lightManager.Deinitialize();
		}
	}

	// Token: 0x06000061 RID: 97 RVA: 0x0000622B File Offset: 0x0000442B
	public int GetVisibleLightCount()
	{
		if (this.m_lightManager != null)
		{
			return this.m_lightManager.VisibleLightCount;
		}
		return 0;
	}

	// Token: 0x06000062 RID: 98 RVA: 0x00006248 File Offset: 0x00004448
	public int GetTotalLightCount()
	{
		if (this.m_lightManager != null)
		{
			return this.m_lightManager.CurrentLightCount;
		}
		return 0;
	}

	// Token: 0x06000063 RID: 99 RVA: 0x00006268 File Offset: 0x00004468
	private void _InitializePrimitiveManagerIfNeccessary()
	{
		if (this.m_primitiveManager == null)
		{
			this.m_primitiveManager = base.GetComponent<FogVolumePrimitiveManager>();
			if (this.m_primitiveManager == null)
			{
				this.m_primitiveManager = base.gameObject.AddComponent<FogVolumePrimitiveManager>();
			}
			this.m_primitiveManager.Initialize();
			this.m_primitiveManager.FindPrimitivesInFogVolume();
		}
	}

	// Token: 0x06000064 RID: 100 RVA: 0x000062C4 File Offset: 0x000044C4
	private void _DeinitializePrimitiveManagerIfNeccessary()
	{
		if (this.m_primitiveManager != null)
		{
			this.m_primitiveManager.Deinitialize();
		}
	}

	// Token: 0x06000065 RID: 101 RVA: 0x000062DF File Offset: 0x000044DF
	public int GetVisiblePrimitiveCount()
	{
		if (this.m_primitiveManager != null)
		{
			return this.m_primitiveManager.VisiblePrimitiveCount;
		}
		return 0;
	}

	// Token: 0x06000066 RID: 102 RVA: 0x000062FC File Offset: 0x000044FC
	public int GetTotalPrimitiveCount()
	{
		if (this.m_primitiveManager != null)
		{
			return this.m_primitiveManager.CurrentPrimitiveCount;
		}
		return 0;
	}

	// Token: 0x04000053 RID: 83
	public FogVolume.FogType _FogType;

	// Token: 0x04000054 RID: 84
	public bool RenderableInSceneView = true;

	// Token: 0x04000055 RID: 85
	private GameObject FogVolumeGameObject;

	// Token: 0x04000056 RID: 86
	public FogVolume ShadowCaster;

	// Token: 0x04000057 RID: 87
	public Texture2D _PerformanceLUT;

	// Token: 0x04000058 RID: 88
	public Vector3 fogVolumeScale = new Vector3(20f, 20f, 20f);

	// Token: 0x04000059 RID: 89
	[SerializeField]
	public Color FogMainColor = Color.white;

	// Token: 0x0400005A RID: 90
	public Color _FogColor = Color.white;

	// Token: 0x0400005B RID: 91
	public bool _AmbientAffectsFogColor;

	// Token: 0x0400005C RID: 92
	[Range(1f, 5f)]
	public float Exposure = 2.5f;

	// Token: 0x0400005D RID: 93
	[Range(-0.5f, 0.5f)]
	public float Offset;

	// Token: 0x0400005E RID: 94
	[Range(0.01f, 3f)]
	public float Gamma = 1f;

	// Token: 0x0400005F RID: 95
	public bool Tonemap;

	// Token: 0x04000060 RID: 96
	public float Visibility = 25f;

	// Token: 0x04000061 RID: 97
	[HideInInspector]
	public bool ColorAdjust;

	// Token: 0x04000062 RID: 98
	[Header("Lighting")]
	public int _SelfShadowSteps = 3;

	// Token: 0x04000063 RID: 99
	public int ShadowCameraSkippedFrames;

	// Token: 0x04000064 RID: 100
	public Color _SelfShadowColor = Color.black;

	// Token: 0x04000065 RID: 101
	public float VolumeFogInscatteringAnisotropy = 0.5f;

	// Token: 0x04000066 RID: 102
	public float VolumeFogInscatteringIntensity = 0.1f;

	// Token: 0x04000067 RID: 103
	public float VolumeFogInscatteringStartDistance = 10f;

	// Token: 0x04000068 RID: 104
	public float VolumeFogInscatteringTransitionWideness = 1f;

	// Token: 0x04000069 RID: 105
	public Color VolumeFogInscatteringColor = Color.white;

	// Token: 0x0400006A RID: 106
	public bool VolumeFogInscattering;

	// Token: 0x0400006B RID: 107
	public float HeightAbsorption;

	// Token: 0x0400006C RID: 108
	public float HeightAbsorptionMin = -1f;

	// Token: 0x0400006D RID: 109
	public float HeightAbsorptionMax = -1f;

	// Token: 0x0400006E RID: 110
	public bool SunAttached;

	// Token: 0x0400006F RID: 111
	[SerializeField]
	public Light Sun;

	// Token: 0x04000070 RID: 112
	[SerializeField]
	public bool _ShadeNoise;

	// Token: 0x04000071 RID: 113
	public bool _DirectionalLighting;

	// Token: 0x04000072 RID: 114
	public float _DirectionalLightingDistance = 0.01f;

	// Token: 0x04000073 RID: 115
	public float DirectLightingShadowDensity = 1f;

	// Token: 0x04000074 RID: 116
	public Color LightExtinctionColor = new Color(0.31764707f, 0.1764706f, 0.101960786f, 0f);

	// Token: 0x04000075 RID: 117
	public int DirectLightingShadowSteps = 1;

	// Token: 0x04000076 RID: 118
	public bool bAbsorption = true;

	// Token: 0x04000077 RID: 119
	public float Absorption = 0.5f;

	// Token: 0x04000078 RID: 120
	public float _LightExposure = 1f;

	// Token: 0x04000079 RID: 121
	public bool Lambert;

	// Token: 0x0400007A RID: 122
	public float LambertianBias = 1f;

	// Token: 0x0400007B RID: 123
	public float NormalDistance = 0.01f;

	// Token: 0x0400007C RID: 124
	public float DirectLightingAmount = 1f;

	// Token: 0x0400007D RID: 125
	public float DirectLightingDistance = 10f;

	// Token: 0x0400007E RID: 126
	[Range(1f, 3f)]
	public float ShadowBrightness = 1f;

	// Token: 0x0400007F RID: 127
	public Color _AmbientColor = Color.gray;

	// Token: 0x04000080 RID: 128
	private bool _ProxyVolume;

	// Token: 0x04000081 RID: 129
	[SerializeField]
	[Range(0f, 0.15f)]
	public float ShadowShift = 0.0094f;

	// Token: 0x04000082 RID: 130
	[Range(0f, 5f)]
	public float PointLightsIntensity = 1f;

	// Token: 0x04000083 RID: 131
	public float PointLightingDistance = 1000f;

	// Token: 0x04000084 RID: 132
	public float PointLightingDistance2Camera = 50f;

	// Token: 0x04000085 RID: 133
	public float PointLightCullSizeMultiplier = 1f;

	// Token: 0x04000086 RID: 134
	public bool PointLightsActive;

	// Token: 0x04000087 RID: 135
	public bool EnableInscattering;

	// Token: 0x04000088 RID: 136
	public Color InscatteringColor = Color.white;

	// Token: 0x04000089 RID: 137
	public Texture2D _LightHaloTexture;

	// Token: 0x0400008A RID: 138
	public Texture2D CoverageTex;

	// Token: 0x0400008B RID: 139
	public bool LightHalo;

	// Token: 0x0400008C RID: 140
	public float _HaloWidth = 0.75f;

	// Token: 0x0400008D RID: 141
	public float _HaloOpticalDispersion = 1f;

	// Token: 0x0400008E RID: 142
	public float _HaloRadius = 1f;

	// Token: 0x0400008F RID: 143
	public float _HaloIntensity = 1f;

	// Token: 0x04000090 RID: 144
	public float _HaloAbsorption = 0.5f;

	// Token: 0x04000091 RID: 145
	[Range(-1f, 1f)]
	public float InscatteringShape;

	// Token: 0x04000092 RID: 146
	public float InscatteringIntensity = 0.2f;

	// Token: 0x04000093 RID: 147
	public float InscatteringStartDistance;

	// Token: 0x04000094 RID: 148
	public float InscatteringTransitionWideness = 1f;

	// Token: 0x04000095 RID: 149
	[Header("Noise")]
	public bool EnableNoise;

	// Token: 0x04000096 RID: 150
	public int Octaves = 1;

	// Token: 0x04000097 RID: 151
	public float _DetailMaskingThreshold = 18f;

	// Token: 0x04000098 RID: 152
	public bool bSphericalFade;

	// Token: 0x04000099 RID: 153
	public float SphericalFadeDistance = 10f;

	// Token: 0x0400009A RID: 154
	public float DetailDistance = 500f;

	// Token: 0x0400009B RID: 155
	public float DetailTiling = 1f;

	// Token: 0x0400009C RID: 156
	public float _DetailRelativeSpeed = 10f;

	// Token: 0x0400009D RID: 157
	public float _BaseRelativeSpeed = 1f;

	// Token: 0x0400009E RID: 158
	public float _NoiseDetailRange = 0.5f;

	// Token: 0x0400009F RID: 159
	public float _Curl = 0.5f;

	// Token: 0x040000A0 RID: 160
	public float BaseTiling = 8f;

	// Token: 0x040000A1 RID: 161
	public float Coverage = 1.5f;

	// Token: 0x040000A2 RID: 162
	public float NoiseDensity = 1f;

	// Token: 0x040000A3 RID: 163
	public float _3DNoiseScale = 80f;

	// Token: 0x040000A4 RID: 164
	[Range(10f, 300f)]
	public int Iterations = 85;

	// Token: 0x040000A5 RID: 165
	public FogVolumeRenderer.BlendMode _BlendMode = FogVolumeRenderer.BlendMode.TraditionalTransparency;

	// Token: 0x040000A6 RID: 166
	public float IterationStep = 500f;

	// Token: 0x040000A7 RID: 167
	public float _OptimizationFactor;

	// Token: 0x040000A8 RID: 168
	public FogVolume.SamplingMethod _SamplingMethod;

	// Token: 0x040000A9 RID: 169
	public bool _VisibleByReflectionProbeStatic = true;

	// Token: 0x040000AA RID: 170
	private FogVolumeRenderer _FogVolumeRenderer;

	// Token: 0x040000AB RID: 171
	public GameObject GameCameraGO;

	// Token: 0x040000AC RID: 172
	public FogVolume.DebugMode _DebugMode;

	// Token: 0x040000AD RID: 173
	public bool useHeightGradient;

	// Token: 0x040000AE RID: 174
	public float GradMin = 1f;

	// Token: 0x040000AF RID: 175
	public float GradMax = -1f;

	// Token: 0x040000B0 RID: 176
	public float GradMin2 = -1f;

	// Token: 0x040000B1 RID: 177
	public float GradMax2 = -1f;

	// Token: 0x040000B2 RID: 178
	public Texture3D _NoiseVolume2;

	// Token: 0x040000B3 RID: 179
	public Texture3D _NoiseVolume;

	// Token: 0x040000B4 RID: 180
	[Range(0f, 10f)]
	public float NoiseIntensity = 0.3f;

	// Token: 0x040000B5 RID: 181
	[Range(0f, 5f)]
	public float NoiseContrast = 12f;

	// Token: 0x040000B6 RID: 182
	public float FadeDistance = 5000f;

	// Token: 0x040000B7 RID: 183
	[Range(0f, 20f)]
	public float Vortex;

	// Token: 0x040000B8 RID: 184
	public FogVolume.VortexAxis _VortexAxis = FogVolume.VortexAxis.Z;

	// Token: 0x040000B9 RID: 185
	public bool bVolumeFog;

	// Token: 0x040000BA RID: 186
	public bool VolumeFogInscatterColorAffectedWithFogColor = true;

	// Token: 0x040000BB RID: 187
	public FogVolume.LightScatterMethod _LightScatterMethod = FogVolume.LightScatterMethod.InverseSquare;

	// Token: 0x040000BC RID: 188
	[Range(0f, 360f)]
	public float rotation;

	// Token: 0x040000BD RID: 189
	[Range(0f, 10f)]
	public float RotationSpeed;

	// Token: 0x040000BE RID: 190
	public Vector4 Speed = new Vector4(0f, 0f, 0f, 0f);

	// Token: 0x040000BF RID: 191
	public Vector4 Stretch = new Vector4(0f, 0f, 0f, 0f);

	// Token: 0x040000C0 RID: 192
	[SerializeField]
	[Header("Collision")]
	public bool SceneCollision = true;

	// Token: 0x040000C1 RID: 193
	public bool ShowPrimitives;

	// Token: 0x040000C2 RID: 194
	public bool EnableDistanceFields;

	// Token: 0x040000C3 RID: 195
	public float _PrimitiveEdgeSoftener = 1f;

	// Token: 0x040000C4 RID: 196
	public float _PrimitiveCutout;

	// Token: 0x040000C5 RID: 197
	public float Constrain;

	// Token: 0x040000C6 RID: 198
	[SerializeField]
	[Range(1f, 200f)]
	public float _SceneIntersectionSoftness = 1f;

	// Token: 0x040000C7 RID: 199
	[SerializeField]
	[Range(0f, 0.1f)]
	public float _jitter = 0.0045f;

	// Token: 0x040000C8 RID: 200
	public MeshRenderer FogRenderer;

	// Token: 0x040000C9 RID: 201
	public Texture2D[] _InspectorBackground;

	// Token: 0x040000CA RID: 202
	public int _InspectorBackgroundIndex;

	// Token: 0x040000CB RID: 203
	public string Description = "";

	// Token: 0x040000CC RID: 204
	[Header("Gradient")]
	public bool EnableGradient;

	// Token: 0x040000CD RID: 205
	public Texture2D Gradient;

	// Token: 0x040000CE RID: 206
	[SerializeField]
	public bool HideWireframe = true;

	// Token: 0x040000CF RID: 207
	[SerializeField]
	public bool SaveMaterials;

	// Token: 0x040000D0 RID: 208
	[SerializeField]
	public bool RequestSavingMaterials;

	// Token: 0x040000D1 RID: 209
	[SerializeField]
	public int DrawOrder;

	// Token: 0x040000D2 RID: 210
	[SerializeField]
	public bool ShowDebugGizmos;

	// Token: 0x040000D3 RID: 211
	private MeshFilter filter;

	// Token: 0x040000D4 RID: 212
	private Mesh mesh;

	// Token: 0x040000D5 RID: 213
	public RenderTexture RT_Opacity;

	// Token: 0x040000D6 RID: 214
	public RenderTexture RT_OpacityBlur;

	// Token: 0x040000D7 RID: 215
	public float ShadowCutoff = 1f;

	// Token: 0x040000D8 RID: 216
	private Vector3 currentFogVolume = Vector3.one;

	// Token: 0x040000D9 RID: 217
	public bool CastShadows;

	// Token: 0x040000DA RID: 218
	public GameObject ShadowCameraGO;

	// Token: 0x040000DB RID: 219
	public int shadowCameraPosition = 20;

	// Token: 0x040000DC RID: 220
	public ShadowCamera _ShadowCamera;

	// Token: 0x040000DD RID: 221
	public float _PushAlpha = 1f;

	// Token: 0x040000DE RID: 222
	[SerializeField]
	private Material fogMaterial;

	// Token: 0x040000DF RID: 223
	public Shader FogVolumeShader;

	// Token: 0x040000E0 RID: 224
	[SerializeField]
	private GameObject ShadowProjector;

	// Token: 0x040000E1 RID: 225
	private MeshRenderer ShadowProjectorRenderer;

	// Token: 0x040000E2 RID: 226
	private MeshFilter ShadowProjectorMeshFilter;

	// Token: 0x040000E3 RID: 227
	private Material ShadowProjectorMaterial;

	// Token: 0x040000E4 RID: 228
	public Mesh ShadowProjectorMesh;

	// Token: 0x040000E5 RID: 229
	public Color ShadowColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);

	// Token: 0x040000E6 RID: 230
	public CompareFunction _ztest;

	// Token: 0x040000E7 RID: 231
	private Plane[] FrustumPlanes;

	// Token: 0x040000E8 RID: 232
	private Camera GameCamera;

	// Token: 0x040000E9 RID: 233
	private Material SurrogateMaterial;

	// Token: 0x040000EA RID: 234
	private BoxCollider _BoxCollider;

	// Token: 0x040000EB RID: 235
	public FogVolumeData _FogVolumeData;

	// Token: 0x040000EC RID: 236
	private GameObject _FogVolumeDataGO;

	// Token: 0x040000ED RID: 237
	public bool ExcludeFromLowRes;

	// Token: 0x040000EE RID: 238
	public float PointLightCPUMaxDistance = 1f;

	// Token: 0x040000EF RID: 239
	private GameObject PointLightsCameraGO;

	// Token: 0x040000F0 RID: 240
	private Camera PointLightsCamera;

	// Token: 0x040000F1 RID: 241
	public float PointLightScreenMargin = 1f;

	// Token: 0x040000F2 RID: 242
	public bool PointLightsRealTimeUpdate = true;

	// Token: 0x040000F3 RID: 243
	public bool PointLightBoxCheck = true;

	// Token: 0x040000F4 RID: 244
	public bool PrimitivesRealTimeUpdate = true;

	// Token: 0x040000F5 RID: 245
	public bool IsVisible;

	// Token: 0x040000F6 RID: 246
	public bool CreateSurrogate = true;

	// Token: 0x040000F7 RID: 247
	public bool InjectCustomDepthBuffer;

	// Token: 0x040000F8 RID: 248
	public bool UseConvolvedLightshafts;

	// Token: 0x040000F9 RID: 249
	private bool m_hasUpdatedBoxMesh;

	// Token: 0x040000FA RID: 250
	private FogVolumeLightManager m_lightManager;

	// Token: 0x040000FB RID: 251
	private FogVolumePrimitiveManager m_primitiveManager;

	// Token: 0x0200021C RID: 540
	public enum FogType
	{
		// Token: 0x04000DD6 RID: 3542
		Uniform,
		// Token: 0x04000DD7 RID: 3543
		Textured
	}

	// Token: 0x0200021D RID: 541
	public enum SamplingMethod
	{
		// Token: 0x04000DD9 RID: 3545
		Eye2Box,
		// Token: 0x04000DDA RID: 3546
		ViewAligned
	}

	// Token: 0x0200021E RID: 542
	public enum DebugMode
	{
		// Token: 0x04000DDC RID: 3548
		Lit,
		// Token: 0x04000DDD RID: 3549
		Iterations,
		// Token: 0x04000DDE RID: 3550
		Inscattering,
		// Token: 0x04000DDF RID: 3551
		VolumetricShadows,
		// Token: 0x04000DE0 RID: 3552
		VolumeFogInscatterClamp,
		// Token: 0x04000DE1 RID: 3553
		VolumeFogPhase
	}

	// Token: 0x0200021F RID: 543
	public enum VortexAxis
	{
		// Token: 0x04000DE3 RID: 3555
		X,
		// Token: 0x04000DE4 RID: 3556
		Y,
		// Token: 0x04000DE5 RID: 3557
		Z
	}

	// Token: 0x02000220 RID: 544
	public enum LightScatterMethod
	{
		// Token: 0x04000DE7 RID: 3559
		BeersLaw,
		// Token: 0x04000DE8 RID: 3560
		InverseSquare,
		// Token: 0x04000DE9 RID: 3561
		Linear
	}
}
