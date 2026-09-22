using System;
using FogVolumeUtilities;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000005 RID: 5
[ExecuteInEditMode]
public class FogVolume : MonoBehaviour
{
	// Token: 0x06000005 RID: 5 RVA: 0x000025E0 File Offset: 0x000009E0
	public void setNoiseIntensity(float value)
	{
		this.NoiseIntensity = value;
	}

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000007 RID: 7 RVA: 0x00002609 File Offset: 0x00000A09
	// (set) Token: 0x06000006 RID: 6 RVA: 0x000025E9 File Offset: 0x000009E9
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

	// Token: 0x06000008 RID: 8 RVA: 0x00002611 File Offset: 0x00000A11
	public float GetVisibility()
	{
		return this.Visibility;
	}

	// Token: 0x17000003 RID: 3
	// (get) Token: 0x06000009 RID: 9 RVA: 0x00002619 File Offset: 0x00000A19
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

	// Token: 0x0600000A RID: 10 RVA: 0x00002638 File Offset: 0x00000A38
	private void RemoveMaterial()
	{
	}

	// Token: 0x0600000B RID: 11 RVA: 0x0000263C File Offset: 0x00000A3C
	private void CreateMaterial()
	{
		if (this.SaveMaterials)
		{
			this.FogVolumeShader = Shader.Find("Hidden/FogVolume");
			this.fogMaterial = new Material(this.FogVolumeShader);
		}
		else
		{
			global::UnityEngine.Object.DestroyImmediate(this.fogMaterial);
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
	}

	// Token: 0x0600000C RID: 12 RVA: 0x000026FC File Offset: 0x00000AFC
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

	// Token: 0x0600000D RID: 13 RVA: 0x000027D8 File Offset: 0x00000BD8
	private void ShadowMapSetup()
	{
		if (this.ShadowCameraGO)
		{
			this.ShadowCameraGO.GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
			this.ShadowCameraGO.GetComponent<Camera>().renderingPath = RenderingPath.Forward;
		}
		if (this.CastShadows)
		{
			this.fogVolumeScale.z = this.fogVolumeScale.x;
		}
		this._ShadowCamera.CameraTransform();
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
				this.ShadowProjectorMesh = gameObject.GetComponent<MeshFilter>().mesh;
				global::UnityEngine.Object.DestroyImmediate(gameObject, true);
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

	// Token: 0x0600000E RID: 14 RVA: 0x00002ABC File Offset: 0x00000EBC
	private void SetShadowProyectorLayer()
	{
		if (this.ShadowProjector)
		{
			if (!this.RenderableInSceneView)
			{
				if (this.ShadowProjector.layer == LayerMask.NameToLayer("Default"))
				{
					this.ShadowProjector.layer = LayerMask.NameToLayer("UI");
				}
			}
			else if (this.ShadowProjector.layer == LayerMask.NameToLayer("UI"))
			{
				this.ShadowProjector.layer = LayerMask.NameToLayer("Default");
			}
		}
	}

	// Token: 0x0600000F RID: 15 RVA: 0x00002B48 File Offset: 0x00000F48
	private void FindDirectionalLight()
	{
		Light[] array = global::UnityEngine.Object.FindObjectsOfType<Light>();
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

	// Token: 0x06000010 RID: 16 RVA: 0x00002C3C File Offset: 0x0000103C
	public void FindFogVolumeData()
	{
		if (this._FogVolumeDataGO == null)
		{
			FogVolumeData[] array = global::UnityEngine.Object.FindObjectsOfType<FogVolumeData>();
			if (array.Length == 0)
			{
				this._FogVolumeDataGO = new GameObject();
				this._FogVolumeData = this._FogVolumeDataGO.AddComponent<FogVolumeData>();
				this._FogVolumeDataGO.name = "Fog Volume Data";
			}
			else
			{
				this._FogVolumeDataGO = array[0].gameObject;
				this._FogVolumeData = array[0];
			}
		}
		else
		{
			this._FogVolumeData = this._FogVolumeDataGO.GetComponent<FogVolumeData>();
		}
	}

	// Token: 0x06000011 RID: 17 RVA: 0x00002CC8 File Offset: 0x000010C8
	private void MoveToLayer()
	{
		if (!this.CastShadows && !this.ExcludeFromLowRes)
		{
			if (this._FogType == FogVolume.FogType.Textured)
			{
				if (this.FogVolumeGameObject.layer != LayerMask.NameToLayer("FogVolume"))
				{
					this.FogVolumeGameObject.layer = LayerMask.NameToLayer("FogVolume");
				}
			}
			else if (this.FogVolumeGameObject.layer != LayerMask.NameToLayer("FogVolumeUniform"))
			{
				this.FogVolumeGameObject.layer = LayerMask.NameToLayer("FogVolumeUniform");
			}
		}
	}

	// Token: 0x06000012 RID: 18 RVA: 0x00002D5C File Offset: 0x0000115C
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
			}
		}
		else if (!this._FogVolumeData.ForceNoRenderer)
		{
			this._FogVolumeRenderer.enabled = true;
		}
	}

	// Token: 0x06000013 RID: 19 RVA: 0x00002E4E File Offset: 0x0000124E
	private void SetIcon()
	{
	}

	// Token: 0x06000014 RID: 20 RVA: 0x00002E50 File Offset: 0x00001250
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
		this.ShadowCameraGO = new GameObject();
		this.ShadowCameraGO.transform.parent = this.FogVolumeGameObject.transform;
		this.ShadowCameraGO.AddComponent<Camera>();
		Camera component = this.ShadowCameraGO.GetComponent<Camera>();
		component.orthographic = true;
		component.clearFlags = CameraClearFlags.Color;
		component.backgroundColor = new Color(0f, 0f, 0f, 0f);
		this._ShadowCamera = this.ShadowCameraGO.AddComponent<ShadowCamera>();
		this.ShadowCameraGO.name = this.FogVolumeGameObject.name + " Shadow Camera";
		this.ShadowCameraGO.hideFlags = HideFlags.HideAndDontSave;
		this.ShadowMapSetup();
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

	// Token: 0x06000015 RID: 21 RVA: 0x00003088 File Offset: 0x00001488
	private float GetPointLightDistance2Camera(Vector3 lightPosition)
	{
		this.PointLightsCameraGO = Camera.current.gameObject;
		return (lightPosition - this.PointLightsCameraGO.transform.position).magnitude;
	}

	// Token: 0x06000016 RID: 22 RVA: 0x000030C8 File Offset: 0x000014C8
	private bool PointIsVisible(Vector3 point)
	{
		this.PointLightsCamera = this.PointLightsCameraGO.GetComponent<Camera>();
		float num = 0f - this.PointLightScreenMargin;
		float num2 = 1f + this.PointLightScreenMargin;
		Vector3 vector = this.PointLightsCamera.WorldToViewportPoint(point);
		return vector.z > num && vector.x > num && vector.x < num2 && vector.y > num && vector.y < num2;
	}

	// Token: 0x06000017 RID: 23 RVA: 0x0000315C File Offset: 0x0000155C
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

	// Token: 0x06000018 RID: 24 RVA: 0x000032CB File Offset: 0x000016CB
	private Vector3 LocalDirectionalSunLight(Light Sun)
	{
		return base.transform.InverseTransformVector(-Sun.transform.forward);
	}

	// Token: 0x06000019 RID: 25 RVA: 0x000032E8 File Offset: 0x000016E8
	private void OnDisable()
	{
		this.m_lightManager = null;
		this.m_primitiveManager = null;
	}

	// Token: 0x0600001A RID: 26 RVA: 0x000032F8 File Offset: 0x000016F8
	public static void Wireframe(GameObject obj, bool Enable)
	{
	}

	// Token: 0x0600001B RID: 27 RVA: 0x000032FA File Offset: 0x000016FA
	private void OnBecameVisible()
	{
	}

	// Token: 0x0600001C RID: 28 RVA: 0x000032FC File Offset: 0x000016FC
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
				this._ShadowCamera.enabled = this.CastShadows;
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

	// Token: 0x0600001D RID: 29 RVA: 0x00003548 File Offset: 0x00001948
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

	// Token: 0x0600001E RID: 30 RVA: 0x00003840 File Offset: 0x00001C40
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

	// Token: 0x0600001F RID: 31 RVA: 0x00003928 File Offset: 0x00001D28
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
			if (this._FogType == FogVolume.FogType.Textured)
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
			this.FogMaterial.SetFloat("_ProxyVolume", (float)(this._ProxyVolume ? 1 : 0));
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
			this.FogMaterial.SetInt("_AmbientAffectsFogColor", (!this._AmbientAffectsFogColor) ? 0 : 1);
			this.FogMaterial.SetFloat("_SceneIntersectionSoftness", this._SceneIntersectionSoftness);
			this.FogMaterial.SetFloat("_RayStep", this.IterationStep * 0.001f);
			this.FogMaterial.SetFloat("_OptimizationFactor", this._OptimizationFactor);
			this.FogMaterial.SetFloat("_Visibility", ((!this.bVolumeFog || !this.EnableNoise || !this._NoiseVolume) && !this.EnableGradient) ? this.Visibility : (this.Visibility * 100f));
			this.FogRenderer.sortingOrder = this.DrawOrder;
			this.FogMaterial.SetInt("VolumeFogInscatterColorAffectedWithFogColor", (!this.VolumeFogInscatterColorAffectedWithFogColor) ? 0 : 1);
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

	// Token: 0x06000020 RID: 32 RVA: 0x00004694 File Offset: 0x00002A94
	private void LateUpdate()
	{
	}

	// Token: 0x06000021 RID: 33 RVA: 0x00004698 File Offset: 0x00002A98
	private void OnWillRenderObject()
	{
		this.GetShadowMap();
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
		}
		else
		{
			this.FogMaterial.SetVector("_BoxMin", this.currentFogVolume * -0.5f);
			this.FogMaterial.SetVector("_BoxMax", this.currentFogVolume * 0.5f);
		}
	}

	// Token: 0x06000022 RID: 34 RVA: 0x0000493C File Offset: 0x00002D3C
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
				FogVolume.VortexAxis vortexAxis = this._VortexAxis;
				if (vortexAxis != FogVolume.VortexAxis.X)
				{
					if (vortexAxis != FogVolume.VortexAxis.Y)
					{
						if (vortexAxis == FogVolume.VortexAxis.Z)
						{
							this.FogMaterial.DisableKeyword("Twirl_X");
							this.FogMaterial.DisableKeyword("Twirl_Y");
							this.FogMaterial.EnableKeyword("Twirl_Z");
						}
					}
					else
					{
						this.FogMaterial.DisableKeyword("Twirl_X");
						this.FogMaterial.EnableKeyword("Twirl_Y");
						this.FogMaterial.DisableKeyword("Twirl_Z");
					}
				}
				else
				{
					this.FogMaterial.EnableKeyword("Twirl_X");
					this.FogMaterial.DisableKeyword("Twirl_Y");
					this.FogMaterial.DisableKeyword("Twirl_Z");
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
				FogVolume.LightScatterMethod lightScatterMethod = this._LightScatterMethod;
				if (lightScatterMethod != FogVolume.LightScatterMethod.BeersLaw)
				{
					if (lightScatterMethod != FogVolume.LightScatterMethod.InverseSquare)
					{
						if (lightScatterMethod == FogVolume.LightScatterMethod.Linear)
						{
							this.FogMaterial.DisableKeyword("ATTEN_METHOD_1");
							this.FogMaterial.DisableKeyword("ATTEN_METHOD_2");
							this.FogMaterial.EnableKeyword("ATTEN_METHOD_3");
						}
					}
					else
					{
						this.FogMaterial.DisableKeyword("ATTEN_METHOD_1");
						this.FogMaterial.EnableKeyword("ATTEN_METHOD_2");
						this.FogMaterial.DisableKeyword("ATTEN_METHOD_3");
					}
				}
				else
				{
					this.FogMaterial.EnableKeyword("ATTEN_METHOD_1");
					this.FogMaterial.DisableKeyword("ATTEN_METHOD_2");
					this.FogMaterial.DisableKeyword("ATTEN_METHOD_3");
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
				if (this.EnableNoise && !SystemInfo.supports3DTextures)
				{
					Debug.Log("Noise not supported. SM level found: " + SystemInfo.graphicsShaderLevel / 10);
				}
				if (this.EnableNoise && SystemInfo.supports3DTextures)
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
				this.FogMaterial.SetFloat("Collisions", (float)((!this.SceneCollision) ? 0 : 1));
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
				}
				else
				{
					this.FogMaterial.DisableKeyword("DIRECTIONAL_LIGHTING");
				}
			}
		}
	}

	// Token: 0x06000023 RID: 35 RVA: 0x000050E4 File Offset: 0x000034E4
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

	// Token: 0x17000004 RID: 4
	// (get) Token: 0x06000024 RID: 36 RVA: 0x00005154 File Offset: 0x00003554
	public bool HasUpdatedBoxMesh
	{
		get
		{
			bool hasUpdatedBoxMesh = this.m_hasUpdatedBoxMesh;
			this.m_hasUpdatedBoxMesh = false;
			return hasUpdatedBoxMesh;
		}
	}

	// Token: 0x06000025 RID: 37 RVA: 0x00005170 File Offset: 0x00003570
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

	// Token: 0x06000026 RID: 38 RVA: 0x000057F0 File Offset: 0x00003BF0
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
			}
			else
			{
				this.m_lightManager.FindLightsInScene();
			}
		}
	}

	// Token: 0x06000027 RID: 39 RVA: 0x0000586D File Offset: 0x00003C6D
	private void _DeinitializeLightManagerIfNeccessary()
	{
		if (this.m_lightManager != null)
		{
			this.m_lightManager.Deinitialize();
		}
	}

	// Token: 0x06000028 RID: 40 RVA: 0x0000588B File Offset: 0x00003C8B
	public int GetVisibleLightCount()
	{
		if (this.m_lightManager != null)
		{
			return this.m_lightManager.VisibleLightCount;
		}
		return 0;
	}

	// Token: 0x06000029 RID: 41 RVA: 0x000058AB File Offset: 0x00003CAB
	public int GetTotalLightCount()
	{
		if (this.m_lightManager != null)
		{
			return this.m_lightManager.CurrentLightCount;
		}
		return 0;
	}

	// Token: 0x0600002A RID: 42 RVA: 0x000058CC File Offset: 0x00003CCC
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

	// Token: 0x0600002B RID: 43 RVA: 0x0000592E File Offset: 0x00003D2E
	private void _DeinitializePrimitiveManagerIfNeccessary()
	{
		if (this.m_primitiveManager != null)
		{
			this.m_primitiveManager.Deinitialize();
		}
	}

	// Token: 0x0600002C RID: 44 RVA: 0x0000594C File Offset: 0x00003D4C
	public int GetVisiblePrimitiveCount()
	{
		if (this.m_primitiveManager != null)
		{
			return this.m_primitiveManager.VisiblePrimitiveCount;
		}
		return 0;
	}

	// Token: 0x0600002D RID: 45 RVA: 0x0000596C File Offset: 0x00003D6C
	public int GetTotalPrimitiveCount()
	{
		if (this.m_primitiveManager != null)
		{
			return this.m_primitiveManager.CurrentPrimitiveCount;
		}
		return 0;
	}

	// Token: 0x04000015 RID: 21
	public FogVolume.FogType _FogType;

	// Token: 0x04000016 RID: 22
	public bool RenderableInSceneView = true;

	// Token: 0x04000017 RID: 23
	private GameObject FogVolumeGameObject;

	// Token: 0x04000018 RID: 24
	public FogVolume ShadowCaster;

	// Token: 0x04000019 RID: 25
	public Texture2D _PerformanceLUT;

	// Token: 0x0400001A RID: 26
	public Vector3 fogVolumeScale = new Vector3(20f, 20f, 20f);

	// Token: 0x0400001B RID: 27
	[SerializeField]
	public Color FogMainColor = Color.white;

	// Token: 0x0400001C RID: 28
	public Color _FogColor = Color.white;

	// Token: 0x0400001D RID: 29
	public bool _AmbientAffectsFogColor;

	// Token: 0x0400001E RID: 30
	[Range(1f, 5f)]
	public float Exposure = 2.5f;

	// Token: 0x0400001F RID: 31
	[Range(-0.5f, 0.5f)]
	public float Offset;

	// Token: 0x04000020 RID: 32
	[Range(0.01f, 3f)]
	public float Gamma = 1f;

	// Token: 0x04000021 RID: 33
	public bool Tonemap;

	// Token: 0x04000022 RID: 34
	public float Visibility = 25f;

	// Token: 0x04000023 RID: 35
	[HideInInspector]
	public bool ColorAdjust;

	// Token: 0x04000024 RID: 36
	[Header("Lighting")]
	public int _SelfShadowSteps = 3;

	// Token: 0x04000025 RID: 37
	public int ShadowCameraSkippedFrames;

	// Token: 0x04000026 RID: 38
	public Color _SelfShadowColor = Color.black;

	// Token: 0x04000027 RID: 39
	public float VolumeFogInscatteringAnisotropy = 0.5f;

	// Token: 0x04000028 RID: 40
	public float VolumeFogInscatteringIntensity = 0.1f;

	// Token: 0x04000029 RID: 41
	public float VolumeFogInscatteringStartDistance = 10f;

	// Token: 0x0400002A RID: 42
	public float VolumeFogInscatteringTransitionWideness = 1f;

	// Token: 0x0400002B RID: 43
	public Color VolumeFogInscatteringColor = Color.white;

	// Token: 0x0400002C RID: 44
	public bool VolumeFogInscattering;

	// Token: 0x0400002D RID: 45
	public float HeightAbsorption;

	// Token: 0x0400002E RID: 46
	public float HeightAbsorptionMin = -1f;

	// Token: 0x0400002F RID: 47
	public float HeightAbsorptionMax = -1f;

	// Token: 0x04000030 RID: 48
	public bool SunAttached;

	// Token: 0x04000031 RID: 49
	[SerializeField]
	public Light Sun;

	// Token: 0x04000032 RID: 50
	[SerializeField]
	public bool _ShadeNoise;

	// Token: 0x04000033 RID: 51
	public bool _DirectionalLighting;

	// Token: 0x04000034 RID: 52
	public float _DirectionalLightingDistance = 0.01f;

	// Token: 0x04000035 RID: 53
	public float DirectLightingShadowDensity = 1f;

	// Token: 0x04000036 RID: 54
	public Color LightExtinctionColor = new Color(0.31764707f, 0.1764706f, 0.101960786f, 0f);

	// Token: 0x04000037 RID: 55
	public int DirectLightingShadowSteps = 1;

	// Token: 0x04000038 RID: 56
	public bool bAbsorption = true;

	// Token: 0x04000039 RID: 57
	public float Absorption = 0.5f;

	// Token: 0x0400003A RID: 58
	public float _LightExposure = 1f;

	// Token: 0x0400003B RID: 59
	public bool Lambert;

	// Token: 0x0400003C RID: 60
	public float LambertianBias = 1f;

	// Token: 0x0400003D RID: 61
	public float NormalDistance = 0.01f;

	// Token: 0x0400003E RID: 62
	public float DirectLightingAmount = 1f;

	// Token: 0x0400003F RID: 63
	public float DirectLightingDistance = 10f;

	// Token: 0x04000040 RID: 64
	[Range(1f, 3f)]
	public float ShadowBrightness = 1f;

	// Token: 0x04000041 RID: 65
	public Color _AmbientColor = Color.gray;

	// Token: 0x04000042 RID: 66
	private bool _ProxyVolume;

	// Token: 0x04000043 RID: 67
	[SerializeField]
	[Range(0f, 0.15f)]
	public float ShadowShift = 0.0094f;

	// Token: 0x04000044 RID: 68
	[Range(0f, 5f)]
	public float PointLightsIntensity = 1f;

	// Token: 0x04000045 RID: 69
	public float PointLightingDistance = 1000f;

	// Token: 0x04000046 RID: 70
	public float PointLightingDistance2Camera = 50f;

	// Token: 0x04000047 RID: 71
	public float PointLightCullSizeMultiplier = 1f;

	// Token: 0x04000048 RID: 72
	public bool PointLightsActive;

	// Token: 0x04000049 RID: 73
	public bool EnableInscattering;

	// Token: 0x0400004A RID: 74
	public Color InscatteringColor = Color.white;

	// Token: 0x0400004B RID: 75
	public Texture2D _LightHaloTexture;

	// Token: 0x0400004C RID: 76
	public Texture2D CoverageTex;

	// Token: 0x0400004D RID: 77
	public bool LightHalo;

	// Token: 0x0400004E RID: 78
	public float _HaloWidth = 0.75f;

	// Token: 0x0400004F RID: 79
	public float _HaloOpticalDispersion = 1f;

	// Token: 0x04000050 RID: 80
	public float _HaloRadius = 1f;

	// Token: 0x04000051 RID: 81
	public float _HaloIntensity = 1f;

	// Token: 0x04000052 RID: 82
	public float _HaloAbsorption = 0.5f;

	// Token: 0x04000053 RID: 83
	[Range(-1f, 1f)]
	public float InscatteringShape;

	// Token: 0x04000054 RID: 84
	public float InscatteringIntensity = 0.2f;

	// Token: 0x04000055 RID: 85
	public float InscatteringStartDistance;

	// Token: 0x04000056 RID: 86
	public float InscatteringTransitionWideness = 1f;

	// Token: 0x04000057 RID: 87
	[Header("Noise")]
	public bool EnableNoise;

	// Token: 0x04000058 RID: 88
	public int Octaves = 1;

	// Token: 0x04000059 RID: 89
	public float _DetailMaskingThreshold = 18f;

	// Token: 0x0400005A RID: 90
	public bool bSphericalFade;

	// Token: 0x0400005B RID: 91
	public float SphericalFadeDistance = 10f;

	// Token: 0x0400005C RID: 92
	public float DetailDistance = 500f;

	// Token: 0x0400005D RID: 93
	public float DetailTiling = 1f;

	// Token: 0x0400005E RID: 94
	public float _DetailRelativeSpeed = 10f;

	// Token: 0x0400005F RID: 95
	public float _BaseRelativeSpeed = 1f;

	// Token: 0x04000060 RID: 96
	public float _NoiseDetailRange = 0.5f;

	// Token: 0x04000061 RID: 97
	public float _Curl = 0.5f;

	// Token: 0x04000062 RID: 98
	public float BaseTiling = 8f;

	// Token: 0x04000063 RID: 99
	public float Coverage = 1.5f;

	// Token: 0x04000064 RID: 100
	public float NoiseDensity = 1f;

	// Token: 0x04000065 RID: 101
	public float _3DNoiseScale = 80f;

	// Token: 0x04000066 RID: 102
	[Range(10f, 300f)]
	public int Iterations = 85;

	// Token: 0x04000067 RID: 103
	public FogVolumeRenderer.BlendMode _BlendMode = FogVolumeRenderer.BlendMode.TraditionalTransparency;

	// Token: 0x04000068 RID: 104
	public float IterationStep = 500f;

	// Token: 0x04000069 RID: 105
	public float _OptimizationFactor;

	// Token: 0x0400006A RID: 106
	public FogVolume.SamplingMethod _SamplingMethod;

	// Token: 0x0400006B RID: 107
	public bool _VisibleByReflectionProbeStatic = true;

	// Token: 0x0400006C RID: 108
	private FogVolumeRenderer _FogVolumeRenderer;

	// Token: 0x0400006D RID: 109
	public GameObject GameCameraGO;

	// Token: 0x0400006E RID: 110
	public FogVolume.DebugMode _DebugMode;

	// Token: 0x0400006F RID: 111
	public bool useHeightGradient;

	// Token: 0x04000070 RID: 112
	public float GradMin = 1f;

	// Token: 0x04000071 RID: 113
	public float GradMax = -1f;

	// Token: 0x04000072 RID: 114
	public float GradMin2 = -1f;

	// Token: 0x04000073 RID: 115
	public float GradMax2 = -1f;

	// Token: 0x04000074 RID: 116
	public Texture3D _NoiseVolume2;

	// Token: 0x04000075 RID: 117
	public Texture3D _NoiseVolume;

	// Token: 0x04000076 RID: 118
	[Range(0f, 10f)]
	public float NoiseIntensity = 0.3f;

	// Token: 0x04000077 RID: 119
	[Range(0f, 5f)]
	public float NoiseContrast = 12f;

	// Token: 0x04000078 RID: 120
	public float FadeDistance = 5000f;

	// Token: 0x04000079 RID: 121
	[Range(0f, 20f)]
	public float Vortex;

	// Token: 0x0400007A RID: 122
	public FogVolume.VortexAxis _VortexAxis = FogVolume.VortexAxis.Z;

	// Token: 0x0400007B RID: 123
	public bool bVolumeFog;

	// Token: 0x0400007C RID: 124
	public bool VolumeFogInscatterColorAffectedWithFogColor = true;

	// Token: 0x0400007D RID: 125
	public FogVolume.LightScatterMethod _LightScatterMethod = FogVolume.LightScatterMethod.InverseSquare;

	// Token: 0x0400007E RID: 126
	[Range(0f, 360f)]
	public float rotation;

	// Token: 0x0400007F RID: 127
	[Range(0f, 10f)]
	public float RotationSpeed;

	// Token: 0x04000080 RID: 128
	public Vector4 Speed = new Vector4(0f, 0f, 0f, 0f);

	// Token: 0x04000081 RID: 129
	public Vector4 Stretch = new Vector4(0f, 0f, 0f, 0f);

	// Token: 0x04000082 RID: 130
	[SerializeField]
	[Header("Collision")]
	public bool SceneCollision = true;

	// Token: 0x04000083 RID: 131
	public bool ShowPrimitives;

	// Token: 0x04000084 RID: 132
	public bool EnableDistanceFields;

	// Token: 0x04000085 RID: 133
	public float _PrimitiveEdgeSoftener = 1f;

	// Token: 0x04000086 RID: 134
	public float _PrimitiveCutout;

	// Token: 0x04000087 RID: 135
	public float Constrain;

	// Token: 0x04000088 RID: 136
	[SerializeField]
	[Range(1f, 200f)]
	public float _SceneIntersectionSoftness = 1f;

	// Token: 0x04000089 RID: 137
	[SerializeField]
	[Range(0f, 0.1f)]
	public float _jitter = 0.0045f;

	// Token: 0x0400008A RID: 138
	public MeshRenderer FogRenderer;

	// Token: 0x0400008B RID: 139
	public Texture2D[] _InspectorBackground;

	// Token: 0x0400008C RID: 140
	public int _InspectorBackgroundIndex;

	// Token: 0x0400008D RID: 141
	public string Description = string.Empty;

	// Token: 0x0400008E RID: 142
	[Header("Gradient")]
	public bool EnableGradient;

	// Token: 0x0400008F RID: 143
	public Texture2D Gradient;

	// Token: 0x04000090 RID: 144
	[SerializeField]
	public bool HideWireframe = true;

	// Token: 0x04000091 RID: 145
	[SerializeField]
	public bool SaveMaterials;

	// Token: 0x04000092 RID: 146
	[SerializeField]
	public bool RequestSavingMaterials;

	// Token: 0x04000093 RID: 147
	[SerializeField]
	public int DrawOrder;

	// Token: 0x04000094 RID: 148
	[SerializeField]
	public bool ShowDebugGizmos;

	// Token: 0x04000095 RID: 149
	private MeshFilter filter;

	// Token: 0x04000096 RID: 150
	private Mesh mesh;

	// Token: 0x04000097 RID: 151
	public RenderTexture RT_Opacity;

	// Token: 0x04000098 RID: 152
	public RenderTexture RT_OpacityBlur;

	// Token: 0x04000099 RID: 153
	public float ShadowCutoff = 1f;

	// Token: 0x0400009A RID: 154
	private Vector3 currentFogVolume = Vector3.one;

	// Token: 0x0400009B RID: 155
	public bool CastShadows;

	// Token: 0x0400009C RID: 156
	public GameObject ShadowCameraGO;

	// Token: 0x0400009D RID: 157
	public int shadowCameraPosition = 20;

	// Token: 0x0400009E RID: 158
	public ShadowCamera _ShadowCamera;

	// Token: 0x0400009F RID: 159
	public float _PushAlpha = 1f;

	// Token: 0x040000A0 RID: 160
	[SerializeField]
	private Material fogMaterial;

	// Token: 0x040000A1 RID: 161
	public Shader FogVolumeShader;

	// Token: 0x040000A2 RID: 162
	[SerializeField]
	private GameObject ShadowProjector;

	// Token: 0x040000A3 RID: 163
	private MeshRenderer ShadowProjectorRenderer;

	// Token: 0x040000A4 RID: 164
	private MeshFilter ShadowProjectorMeshFilter;

	// Token: 0x040000A5 RID: 165
	private Material ShadowProjectorMaterial;

	// Token: 0x040000A6 RID: 166
	public Mesh ShadowProjectorMesh;

	// Token: 0x040000A7 RID: 167
	public Color ShadowColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);

	// Token: 0x040000A8 RID: 168
	public CompareFunction _ztest;

	// Token: 0x040000A9 RID: 169
	private Plane[] FrustumPlanes;

	// Token: 0x040000AA RID: 170
	private Camera GameCamera;

	// Token: 0x040000AB RID: 171
	private Material SurrogateMaterial;

	// Token: 0x040000AC RID: 172
	private BoxCollider _BoxCollider;

	// Token: 0x040000AD RID: 173
	public FogVolumeData _FogVolumeData;

	// Token: 0x040000AE RID: 174
	private GameObject _FogVolumeDataGO;

	// Token: 0x040000AF RID: 175
	public bool ExcludeFromLowRes;

	// Token: 0x040000B0 RID: 176
	public float PointLightCPUMaxDistance = 1f;

	// Token: 0x040000B1 RID: 177
	private GameObject PointLightsCameraGO;

	// Token: 0x040000B2 RID: 178
	private Camera PointLightsCamera;

	// Token: 0x040000B3 RID: 179
	public float PointLightScreenMargin = 1f;

	// Token: 0x040000B4 RID: 180
	public bool PointLightsRealTimeUpdate = true;

	// Token: 0x040000B5 RID: 181
	public bool PointLightBoxCheck = true;

	// Token: 0x040000B6 RID: 182
	public bool PrimitivesRealTimeUpdate = true;

	// Token: 0x040000B7 RID: 183
	public bool IsVisible;

	// Token: 0x040000B8 RID: 184
	public bool CreateSurrogate = true;

	// Token: 0x040000B9 RID: 185
	public bool InjectCustomDepthBuffer;

	// Token: 0x040000BA RID: 186
	public bool UseConvolvedLightshafts;

	// Token: 0x040000BB RID: 187
	private bool m_hasUpdatedBoxMesh;

	// Token: 0x040000BC RID: 188
	private FogVolumeLightManager m_lightManager;

	// Token: 0x040000BD RID: 189
	private FogVolumePrimitiveManager m_primitiveManager;

	// Token: 0x02000006 RID: 6
	public enum FogType
	{
		// Token: 0x040000BF RID: 191
		Uniform,
		// Token: 0x040000C0 RID: 192
		Textured
	}

	// Token: 0x02000007 RID: 7
	public enum SamplingMethod
	{
		// Token: 0x040000C2 RID: 194
		Eye2Box,
		// Token: 0x040000C3 RID: 195
		ViewAligned
	}

	// Token: 0x02000008 RID: 8
	public enum DebugMode
	{
		// Token: 0x040000C5 RID: 197
		Lit,
		// Token: 0x040000C6 RID: 198
		Iterations,
		// Token: 0x040000C7 RID: 199
		Inscattering,
		// Token: 0x040000C8 RID: 200
		VolumetricShadows,
		// Token: 0x040000C9 RID: 201
		VolumeFogInscatterClamp,
		// Token: 0x040000CA RID: 202
		VolumeFogPhase
	}

	// Token: 0x02000009 RID: 9
	public enum VortexAxis
	{
		// Token: 0x040000CC RID: 204
		X,
		// Token: 0x040000CD RID: 205
		Y,
		// Token: 0x040000CE RID: 206
		Z
	}

	// Token: 0x0200000A RID: 10
	public enum LightScatterMethod
	{
		// Token: 0x040000D0 RID: 208
		BeersLaw,
		// Token: 0x040000D1 RID: 209
		InverseSquare,
		// Token: 0x040000D2 RID: 210
		Linear
	}
}
