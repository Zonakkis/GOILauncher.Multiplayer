using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000016 RID: 22
public class FogVolumeLightManager : MonoBehaviour
{
	// Token: 0x1700000E RID: 14
	// (get) Token: 0x0600006B RID: 107 RVA: 0x00007BDD File Offset: 0x00005FDD
	// (set) Token: 0x0600006C RID: 108 RVA: 0x00007BE5 File Offset: 0x00005FE5
	public int CurrentLightCount { get; private set; }

	// Token: 0x1700000F RID: 15
	// (get) Token: 0x0600006D RID: 109 RVA: 0x00007BEE File Offset: 0x00005FEE
	// (set) Token: 0x0600006E RID: 110 RVA: 0x00007BF6 File Offset: 0x00005FF6
	public int VisibleLightCount { get; private set; }

	// Token: 0x17000010 RID: 16
	// (get) Token: 0x0600006F RID: 111 RVA: 0x00007BFF File Offset: 0x00005FFF
	// (set) Token: 0x06000070 RID: 112 RVA: 0x00007C07 File Offset: 0x00006007
	public bool DrawDebugData { get; set; }

	// Token: 0x17000011 RID: 17
	// (get) Token: 0x06000071 RID: 113 RVA: 0x00007C10 File Offset: 0x00006010
	public bool AlreadyUsesTransformForPoI
	{
		get
		{
			return this.m_pointOfInterestTf != null;
		}
	}

	// Token: 0x06000072 RID: 114 RVA: 0x00007C20 File Offset: 0x00006020
	public void FindLightsInScene()
	{
		this.CurrentLightCount = 0;
		this.VisibleLightCount = 0;
		this.m_lights.Clear();
		this.m_lightsInFrustum.Clear();
		for (int i = 0; i < 1000; i++)
		{
			this.m_lights.Add(new FogVolumeLightManager.LightData());
			this.m_lightsInFrustum.Add(new FogVolumeLightManager.LightData());
		}
		FogVolumeLight[] array = global::UnityEngine.Object.FindObjectsOfType<FogVolumeLight>();
		for (int j = 0; j < array.Length; j++)
		{
			Light component = array[j].GetComponent<Light>();
			if (component != null)
			{
				LightType type = component.type;
				if (type != LightType.Point)
				{
					if (type == LightType.Spot)
					{
						this.AddSpotLight(component);
						array[j].IsAddedToNormalLight = true;
					}
				}
				else
				{
					this.AddPointLight(component);
					array[j].IsAddedToNormalLight = true;
				}
			}
			else if (array[j].IsPointLight)
			{
				this.AddSimulatedPointLight(array[j]);
				array[j].IsAddedToNormalLight = false;
			}
			else
			{
				this.AddSimulatedSpotLight(array[j]);
				array[j].IsAddedToNormalLight = false;
			}
		}
	}

	// Token: 0x06000073 RID: 115 RVA: 0x00007D3C File Offset: 0x0000613C
	public void FindLightsInFogVolume()
	{
		this.CurrentLightCount = 0;
		this.VisibleLightCount = 0;
		this.m_lights.Clear();
		this.m_lightsInFrustum.Clear();
		for (int i = 0; i < 1000; i++)
		{
			this.m_lights.Add(new FogVolumeLightManager.LightData());
			this.m_lightsInFrustum.Add(new FogVolumeLightManager.LightData());
		}
		if (this.m_boxCollider == null)
		{
			this.m_boxCollider = base.gameObject.GetComponent<BoxCollider>();
		}
		Bounds bounds = this.m_boxCollider.bounds;
		FogVolumeLight[] array = global::UnityEngine.Object.FindObjectsOfType<FogVolumeLight>();
		for (int j = 0; j < array.Length; j++)
		{
			if (bounds.Intersects(new Bounds(array[j].gameObject.transform.position, Vector3.one * 5f)))
			{
				Light component = array[j].GetComponent<Light>();
				if (component != null)
				{
					LightType type = component.type;
					if (type != LightType.Point)
					{
						if (type == LightType.Spot)
						{
							this.AddSpotLight(component);
							array[j].IsAddedToNormalLight = true;
						}
					}
					else
					{
						this.AddPointLight(component);
						array[j].IsAddedToNormalLight = true;
					}
				}
				else if (array[j].IsPointLight)
				{
					this.AddSimulatedPointLight(array[j]);
					array[j].IsAddedToNormalLight = false;
				}
				else
				{
					this.AddSimulatedSpotLight(array[j]);
					array[j].IsAddedToNormalLight = false;
				}
			}
		}
	}

	// Token: 0x06000074 RID: 116 RVA: 0x00007EBC File Offset: 0x000062BC
	public bool AddSimulatedPointLight(FogVolumeLight _light)
	{
		int num = this._FindFirstFreeLight();
		if (num != -1)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[num];
			this.CurrentLightCount++;
			lightData.LightType = EFogVolumeLightType.FogVolumePointLight;
			lightData.Transform = _light.transform;
			lightData.Light = null;
			lightData.FogVolumeLight = _light;
			lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.FogVolumeLight.Range * 2.5f);
			return true;
		}
		return false;
	}

	// Token: 0x06000075 RID: 117 RVA: 0x00007F4C File Offset: 0x0000634C
	public bool AddSimulatedSpotLight(FogVolumeLight _light)
	{
		int num = this._FindFirstFreeLight();
		if (num != -1)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[num];
			this.CurrentLightCount++;
			lightData.LightType = EFogVolumeLightType.FogVolumeSpotLight;
			lightData.Transform = _light.transform;
			lightData.Light = null;
			lightData.FogVolumeLight = _light;
			Vector3 vector = lightData.Transform.position + lightData.Transform.forward * lightData.FogVolumeLight.Range * 0.5f;
			lightData.Bounds = new Bounds(vector, Vector3.one * lightData.FogVolumeLight.Range * (0.75f + lightData.FogVolumeLight.Angle * 0.03f));
			return true;
		}
		return false;
	}

	// Token: 0x06000076 RID: 118 RVA: 0x0000801C File Offset: 0x0000641C
	public bool AddPointLight(Light _light)
	{
		int num = this._FindFirstFreeLight();
		if (num != -1)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[num];
			this.CurrentLightCount++;
			lightData.LightType = EFogVolumeLightType.PointLight;
			lightData.Transform = _light.transform;
			lightData.Light = _light;
			lightData.FogVolumeLight = null;
			lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.Light.range * 2.5f);
			return true;
		}
		return false;
	}

	// Token: 0x06000077 RID: 119 RVA: 0x000080AC File Offset: 0x000064AC
	public bool AddSpotLight(Light _light)
	{
		int num = this._FindFirstFreeLight();
		if (num != -1)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[num];
			this.CurrentLightCount++;
			lightData.LightType = EFogVolumeLightType.SpotLight;
			lightData.Transform = _light.transform;
			lightData.Light = _light;
			lightData.FogVolumeLight = null;
			Vector3 vector = lightData.Transform.position + lightData.Transform.forward * lightData.Light.range * 0.5f;
			lightData.Bounds = new Bounds(vector, Vector3.one * lightData.Light.range * (0.75f + lightData.Light.spotAngle * 0.03f));
			return true;
		}
		return false;
	}

	// Token: 0x06000078 RID: 120 RVA: 0x0000817C File Offset: 0x0000657C
	public bool RemoveLight(Transform _lightToRemove)
	{
		int count = this.m_lights.Count;
		for (int i = 0; i < count; i++)
		{
			if (object.ReferenceEquals(this.m_lights[i].Transform, _lightToRemove))
			{
				this.m_lights[i].LightType = EFogVolumeLightType.None;
				this.CurrentLightCount--;
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000079 RID: 121 RVA: 0x000081E8 File Offset: 0x000065E8
	public void ManualUpdate(ref Plane[] _frustumPlanes)
	{
		this.FrustumPlanes = _frustumPlanes;
		this.m_camera = ((!(this.m_fogVolumeData != null)) ? null : this.m_fogVolumeData.GameCamera);
		if (this.m_camera == null)
		{
			return;
		}
		if (this.m_boxCollider == null)
		{
			this.m_boxCollider = this.m_fogVolume.GetComponent<BoxCollider>();
		}
		if (this.m_pointOfInterestTf != null)
		{
			this.m_pointOfInterest = this.m_pointOfInterestTf.position;
		}
		this._UpdateBounds();
		this._FindLightsInFrustum();
		if (this.m_lightsInFrustum.Count > 64)
		{
			this._SortLightsInFrustum();
		}
		this._PrepareShaderArrays();
	}

	// Token: 0x0600007A RID: 122 RVA: 0x000082A8 File Offset: 0x000066A8
	public void OnDrawGizmos()
	{
		base.hideFlags = HideFlags.HideInInspector;
		if (this.m_camera == null)
		{
			return;
		}
		if (!this.DrawDebugData)
		{
			return;
		}
		Color color = Gizmos.color;
		Gizmos.color = Color.green;
		for (int i = 0; i < this.VisibleLightCount; i++)
		{
			Gizmos.DrawWireCube(this.m_lightsInFrustum[i].Bounds.center, this.m_lightsInFrustum[i].Bounds.size);
		}
		Gizmos.color = Color.magenta;
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(this.m_camera.transform.position, this.m_camera.transform.rotation, Vector3.one);
		Gizmos.DrawFrustum(this.m_camera.transform.position, this.m_camera.fieldOfView, this.m_camera.nearClipPlane, this.m_fogVolume.PointLightingDistance2Camera, this.m_camera.aspect);
		Gizmos.color = color;
		Gizmos.matrix = matrix;
	}

	// Token: 0x0600007B RID: 123 RVA: 0x000083C6 File Offset: 0x000067C6
	public void SetPointLightCullSizeMultiplier(float _cullSizeMultiplier)
	{
		this.m_pointLightCullSizeMultiplier = _cullSizeMultiplier;
	}

	// Token: 0x0600007C RID: 124 RVA: 0x000083CF File Offset: 0x000067CF
	public void SetPointOfInterest(Vector3 _pointOfInterest)
	{
		this.m_pointOfInterestTf = null;
		this.m_pointOfInterest = _pointOfInterest;
	}

	// Token: 0x0600007D RID: 125 RVA: 0x000083DF File Offset: 0x000067DF
	public void SetPointOfInterest(Transform _pointOfInterest)
	{
		this.m_pointOfInterestTf = _pointOfInterest;
	}

	// Token: 0x0600007E RID: 126 RVA: 0x000083E8 File Offset: 0x000067E8
	public Vector4[] GetLightPositionArray()
	{
		return this.m_lightPos;
	}

	// Token: 0x0600007F RID: 127 RVA: 0x000083F0 File Offset: 0x000067F0
	public Vector4[] GetLightRotationArray()
	{
		return this.m_lightRot;
	}

	// Token: 0x06000080 RID: 128 RVA: 0x000083F8 File Offset: 0x000067F8
	public Color[] GetLightColorArray()
	{
		return this.m_lightColor;
	}

	// Token: 0x06000081 RID: 129 RVA: 0x00008400 File Offset: 0x00006800
	public Vector4[] GetLightData()
	{
		return this.m_lightData;
	}

	// Token: 0x06000082 RID: 130 RVA: 0x00008408 File Offset: 0x00006808
	public void Initialize()
	{
		this.m_fogVolume = base.gameObject.GetComponent<FogVolume>();
		this.m_fogVolumeData = global::UnityEngine.Object.FindObjectOfType<FogVolumeData>();
		this.m_camera = null;
		this.m_boxCollider = null;
		this.CurrentLightCount = 0;
		this.DrawDebugData = false;
		if (this.m_lights == null)
		{
			this.m_lights = new List<FogVolumeLightManager.LightData>(1000);
			this.m_lightsInFrustum = new List<FogVolumeLightManager.LightData>(1000);
			for (int i = 0; i < 1000; i++)
			{
				this.m_lights.Add(new FogVolumeLightManager.LightData());
				this.m_lightsInFrustum.Add(new FogVolumeLightManager.LightData());
			}
		}
	}

	// Token: 0x06000083 RID: 131 RVA: 0x000084AE File Offset: 0x000068AE
	public void Deinitialize()
	{
		this.VisibleLightCount = 0;
		this.DrawDebugData = false;
	}

	// Token: 0x06000084 RID: 132 RVA: 0x000084BE File Offset: 0x000068BE
	public void SetFrustumPlanes(ref Plane[] _frustumPlanes)
	{
		this.FrustumPlanes = _frustumPlanes;
	}

	// Token: 0x06000085 RID: 133 RVA: 0x000084C8 File Offset: 0x000068C8
	private void _UpdateBounds()
	{
		int count = this.m_lights.Count;
		for (int i = 0; i < count; i++)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[i];
			if (lightData.LightType != EFogVolumeLightType.None)
			{
				switch (lightData.LightType)
				{
				case EFogVolumeLightType.FogVolumePointLight:
					lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.FogVolumeLight.Range * this.m_pointLightCullSizeMultiplier);
					break;
				case EFogVolumeLightType.FogVolumeSpotLight:
				{
					Vector3 vector = lightData.Transform.position + lightData.Transform.forward * lightData.FogVolumeLight.Range * 0.5f;
					lightData.Bounds = new Bounds(vector, Vector3.one * lightData.FogVolumeLight.Range * this.m_pointLightCullSizeMultiplier * 1.25f);
					break;
				}
				case EFogVolumeLightType.PointLight:
					lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.Light.range * this.m_pointLightCullSizeMultiplier);
					break;
				case EFogVolumeLightType.SpotLight:
				{
					Vector3 vector2 = lightData.Transform.position + lightData.Transform.forward * lightData.Light.range * 0.5f;
					lightData.Bounds = new Bounds(vector2, Vector3.one * lightData.Light.range * this.m_pointLightCullSizeMultiplier * 1.25f);
					break;
				}
				}
			}
		}
	}

	// Token: 0x06000086 RID: 134 RVA: 0x00008698 File Offset: 0x00006A98
	private int _FindFirstFreeLight()
	{
		if (this.CurrentLightCount < 1000)
		{
			int count = this.m_lights.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.m_lights[i].LightType == EFogVolumeLightType.None)
				{
					return i;
				}
			}
		}
		return -1;
	}

	// Token: 0x06000087 RID: 135 RVA: 0x000086EC File Offset: 0x00006AEC
	private void _FindLightsInFrustum()
	{
		this.m_inFrustumCount = 0;
		Vector3 position = this.m_camera.gameObject.transform.position;
		int count = this.m_lights.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.m_lights[i].Transform == null)
			{
				this.m_lights[i].LightType = EFogVolumeLightType.None;
			}
			if (this.m_lights[i].LightType != EFogVolumeLightType.None)
			{
				float magnitude = (this.m_lights[i].Transform.position - position).magnitude;
				if (magnitude <= this.m_fogVolume.PointLightingDistance2Camera)
				{
					switch (this.m_lights[i].LightType)
					{
					case EFogVolumeLightType.None:
						goto IL_0251;
					case EFogVolumeLightType.FogVolumePointLight:
						if (!this.m_lights[i].FogVolumeLight.Enabled)
						{
							goto IL_0251;
						}
						break;
					case EFogVolumeLightType.FogVolumeSpotLight:
						if (!this.m_lights[i].FogVolumeLight.Enabled)
						{
							goto IL_0251;
						}
						break;
					case EFogVolumeLightType.PointLight:
					case EFogVolumeLightType.SpotLight:
						if (!this.m_lights[i].Light.enabled)
						{
							goto IL_0251;
						}
						break;
					}
					if (GeometryUtility.TestPlanesAABB(this.FrustumPlanes, this.m_lights[i].Bounds))
					{
						FogVolumeLightManager.LightData lightData = this.m_lights[i];
						Vector3 position2 = lightData.Transform.position;
						lightData.SqDistance = (position2 - this.m_pointOfInterest).sqrMagnitude;
						lightData.Distance2Camera = (position2 - position).magnitude;
						this.m_lightsInFrustum[this.m_inFrustumCount++] = lightData;
						if (lightData.FogVolumeLight != null)
						{
							if (lightData.LightType == EFogVolumeLightType.FogVolumePointLight && !lightData.FogVolumeLight.IsPointLight)
							{
								lightData.LightType = EFogVolumeLightType.FogVolumeSpotLight;
							}
							else if (lightData.LightType == EFogVolumeLightType.FogVolumeSpotLight && lightData.FogVolumeLight.IsPointLight)
							{
								lightData.LightType = EFogVolumeLightType.FogVolumePointLight;
							}
						}
					}
				}
			}
			IL_0251:;
		}
	}

	// Token: 0x06000088 RID: 136 RVA: 0x00008958 File Offset: 0x00006D58
	private void _SortLightsInFrustum()
	{
		bool flag;
		do
		{
			flag = true;
			for (int i = 0; i < this.m_inFrustumCount - 1; i++)
			{
				if (this.m_lightsInFrustum[i].SqDistance > this.m_lightsInFrustum[i + 1].SqDistance)
				{
					FogVolumeLightManager.LightData lightData = this.m_lightsInFrustum[i];
					this.m_lightsInFrustum[i] = this.m_lightsInFrustum[i + 1];
					this.m_lightsInFrustum[i + 1] = lightData;
					flag = false;
				}
			}
		}
		while (!flag);
	}

	// Token: 0x06000089 RID: 137 RVA: 0x000089EC File Offset: 0x00006DEC
	private void _PrepareShaderArrays()
	{
		this.VisibleLightCount = 0;
		for (int i = 0; i < 64; i++)
		{
			if (i >= this.m_inFrustumCount)
			{
				break;
			}
			FogVolumeLightManager.LightData lightData = this.m_lightsInFrustum[i];
			switch (lightData.LightType)
			{
			case EFogVolumeLightType.FogVolumePointLight:
			{
				FogVolumeLight fogVolumeLight = lightData.FogVolumeLight;
				this.m_lightPos[i] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				this.m_lightRot[i] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				this.m_lightColor[i] = fogVolumeLight.Color;
				this.m_lightData[i] = new Vector4(fogVolumeLight.Intensity * this.m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / this.m_fogVolume.PointLightingDistance2Camera)), fogVolumeLight.Range / 5f, -1f, 0f);
				this.VisibleLightCount++;
				break;
			}
			case EFogVolumeLightType.FogVolumeSpotLight:
			{
				FogVolumeLight fogVolumeLight2 = lightData.FogVolumeLight;
				this.m_lightPos[i] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				this.m_lightRot[i] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				this.m_lightColor[i] = fogVolumeLight2.Color;
				this.m_lightData[i] = new Vector4(fogVolumeLight2.Intensity * this.m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / this.m_fogVolume.PointLightingDistance2Camera)), fogVolumeLight2.Range / 5f, fogVolumeLight2.Angle, 0f);
				this.VisibleLightCount++;
				break;
			}
			case EFogVolumeLightType.PointLight:
			{
				Light light = lightData.Light;
				this.m_lightPos[i] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				this.m_lightRot[i] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				this.m_lightColor[i] = light.color;
				this.m_lightData[i] = new Vector4(light.intensity * this.m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / this.m_fogVolume.PointLightingDistance2Camera)), light.range / 5f, -1f, 0f);
				this.VisibleLightCount++;
				break;
			}
			case EFogVolumeLightType.SpotLight:
			{
				Light light2 = lightData.Light;
				this.m_lightPos[i] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				this.m_lightRot[i] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				this.m_lightColor[i] = light2.color;
				this.m_lightData[i] = new Vector4(light2.intensity * this.m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / this.m_fogVolume.PointLightingDistance2Camera)), light2.range / 5f, light2.spotAngle, 0f);
				this.VisibleLightCount++;
				break;
			}
			}
		}
	}

	// Token: 0x04000134 RID: 308
	private float m_pointLightCullSizeMultiplier = 1f;

	// Token: 0x04000135 RID: 309
	private FogVolume m_fogVolume;

	// Token: 0x04000136 RID: 310
	private FogVolumeData m_fogVolumeData;

	// Token: 0x04000137 RID: 311
	private Camera m_camera;

	// Token: 0x04000138 RID: 312
	private BoxCollider m_boxCollider;

	// Token: 0x04000139 RID: 313
	private Transform m_pointOfInterestTf;

	// Token: 0x0400013A RID: 314
	private Vector3 m_pointOfInterest = Vector3.zero;

	// Token: 0x0400013B RID: 315
	private readonly Vector4[] m_lightPos = new Vector4[64];

	// Token: 0x0400013C RID: 316
	private readonly Vector4[] m_lightRot = new Vector4[64];

	// Token: 0x0400013D RID: 317
	private readonly Color[] m_lightColor = new Color[64];

	// Token: 0x0400013E RID: 318
	private readonly Vector4[] m_lightData = new Vector4[64];

	// Token: 0x0400013F RID: 319
	private List<FogVolumeLightManager.LightData> m_lights;

	// Token: 0x04000140 RID: 320
	private List<FogVolumeLightManager.LightData> m_lightsInFrustum;

	// Token: 0x04000141 RID: 321
	private int m_inFrustumCount;

	// Token: 0x04000142 RID: 322
	private Plane[] FrustumPlanes;

	// Token: 0x04000143 RID: 323
	private const int InvalidIndex = -1;

	// Token: 0x04000144 RID: 324
	private const int MaxVisibleLights = 64;

	// Token: 0x04000145 RID: 325
	private const float InvalidSpotLightAngle = -1f;

	// Token: 0x04000146 RID: 326
	private const float NoData = 0f;

	// Token: 0x04000147 RID: 327
	private const float PointLightRangeDivider = 5f;

	// Token: 0x04000148 RID: 328
	private const float SpotLightRangeDivider = 5f;

	// Token: 0x04000149 RID: 329
	private const int MaxLightCount = 1000;

	// Token: 0x0400014A RID: 330
	private const float LightInVolumeBoundsSize = 5f;

	// Token: 0x02000017 RID: 23
	protected class LightData
	{
		// Token: 0x0600008A RID: 138 RVA: 0x00008E28 File Offset: 0x00007228
		public LightData()
		{
			this.LightType = EFogVolumeLightType.None;
			this.Light = null;
			this.FogVolumeLight = null;
			this.Transform = null;
			this.SqDistance = 0f;
			this.Distance2Camera = 0f;
			this.Bounds = default(Bounds);
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600008B RID: 139 RVA: 0x00008E7C File Offset: 0x0000727C
		// (set) Token: 0x0600008C RID: 140 RVA: 0x00008E84 File Offset: 0x00007284
		public EFogVolumeLightType LightType { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600008D RID: 141 RVA: 0x00008E8D File Offset: 0x0000728D
		// (set) Token: 0x0600008E RID: 142 RVA: 0x00008E95 File Offset: 0x00007295
		public Light Light { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600008F RID: 143 RVA: 0x00008E9E File Offset: 0x0000729E
		// (set) Token: 0x06000090 RID: 144 RVA: 0x00008EA6 File Offset: 0x000072A6
		public FogVolumeLight FogVolumeLight { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000091 RID: 145 RVA: 0x00008EAF File Offset: 0x000072AF
		// (set) Token: 0x06000092 RID: 146 RVA: 0x00008EB7 File Offset: 0x000072B7
		public Transform Transform { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000093 RID: 147 RVA: 0x00008EC0 File Offset: 0x000072C0
		// (set) Token: 0x06000094 RID: 148 RVA: 0x00008EC8 File Offset: 0x000072C8
		public float SqDistance { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000095 RID: 149 RVA: 0x00008ED1 File Offset: 0x000072D1
		// (set) Token: 0x06000096 RID: 150 RVA: 0x00008ED9 File Offset: 0x000072D9
		public float Distance2Camera { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000097 RID: 151 RVA: 0x00008EE2 File Offset: 0x000072E2
		// (set) Token: 0x06000098 RID: 152 RVA: 0x00008EEA File Offset: 0x000072EA
		public Bounds Bounds { get; set; }
	}
}
