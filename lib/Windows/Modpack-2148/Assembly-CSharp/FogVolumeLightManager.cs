using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200001F RID: 31
public class FogVolumeLightManager : MonoBehaviour
{
	// Token: 0x17000013 RID: 19
	// (get) Token: 0x0600008C RID: 140 RVA: 0x00002C1A File Offset: 0x00000E1A
	// (set) Token: 0x0600008D RID: 141 RVA: 0x00002C22 File Offset: 0x00000E22
	public int CurrentLightCount { get; private set; }

	// Token: 0x17000014 RID: 20
	// (get) Token: 0x0600008E RID: 142 RVA: 0x00002C2B File Offset: 0x00000E2B
	// (set) Token: 0x0600008F RID: 143 RVA: 0x00002C33 File Offset: 0x00000E33
	public int VisibleLightCount { get; private set; }

	// Token: 0x17000015 RID: 21
	// (get) Token: 0x06000090 RID: 144 RVA: 0x00002C3C File Offset: 0x00000E3C
	// (set) Token: 0x06000091 RID: 145 RVA: 0x00002C44 File Offset: 0x00000E44
	public bool DrawDebugData { get; set; }

	// Token: 0x17000016 RID: 22
	// (get) Token: 0x06000092 RID: 146 RVA: 0x00002C4D File Offset: 0x00000E4D
	public bool AlreadyUsesTransformForPoI
	{
		get
		{
			return this.m_pointOfInterestTf != null;
		}
	}

	// Token: 0x06000093 RID: 147 RVA: 0x00019360 File Offset: 0x00017560
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
				if (type != LightType.Spot)
				{
					if (type == LightType.Point)
					{
						this.AddPointLight(component);
						array[j].IsAddedToNormalLight = true;
					}
				}
				else
				{
					this.AddSpotLight(component);
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

	// Token: 0x06000094 RID: 148 RVA: 0x00019454 File Offset: 0x00017654
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
					if (type != LightType.Spot)
					{
						if (type == LightType.Point)
						{
							this.AddPointLight(component);
							array[j].IsAddedToNormalLight = true;
						}
					}
					else
					{
						this.AddSpotLight(component);
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

	// Token: 0x06000095 RID: 149 RVA: 0x000195AC File Offset: 0x000177AC
	public bool AddSimulatedPointLight(FogVolumeLight _light)
	{
		int num = this._FindFirstFreeLight();
		if (num != -1)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[num];
			int currentLightCount = this.CurrentLightCount;
			this.CurrentLightCount = currentLightCount + 1;
			lightData.LightType = EFogVolumeLightType.FogVolumePointLight;
			lightData.Transform = _light.transform;
			lightData.Light = null;
			lightData.FogVolumeLight = _light;
			lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.FogVolumeLight.Range * 2.5f);
			return true;
		}
		return false;
	}

	// Token: 0x06000096 RID: 150 RVA: 0x0001963C File Offset: 0x0001783C
	public bool AddSimulatedSpotLight(FogVolumeLight _light)
	{
		int num = this._FindFirstFreeLight();
		if (num != -1)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[num];
			int currentLightCount = this.CurrentLightCount;
			this.CurrentLightCount = currentLightCount + 1;
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

	// Token: 0x06000097 RID: 151 RVA: 0x0001970C File Offset: 0x0001790C
	public bool AddPointLight(Light _light)
	{
		int num = this._FindFirstFreeLight();
		if (num != -1)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[num];
			int currentLightCount = this.CurrentLightCount;
			this.CurrentLightCount = currentLightCount + 1;
			lightData.LightType = EFogVolumeLightType.PointLight;
			lightData.Transform = _light.transform;
			lightData.Light = _light;
			lightData.FogVolumeLight = null;
			lightData.Bounds = new Bounds(lightData.Transform.position, Vector3.one * lightData.Light.range * 2.5f);
			return true;
		}
		return false;
	}

	// Token: 0x06000098 RID: 152 RVA: 0x0001979C File Offset: 0x0001799C
	public bool AddSpotLight(Light _light)
	{
		int num = this._FindFirstFreeLight();
		if (num != -1)
		{
			FogVolumeLightManager.LightData lightData = this.m_lights[num];
			int currentLightCount = this.CurrentLightCount;
			this.CurrentLightCount = currentLightCount + 1;
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

	// Token: 0x06000099 RID: 153 RVA: 0x0001986C File Offset: 0x00017A6C
	public bool RemoveLight(Transform _lightToRemove)
	{
		int count = this.m_lights.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.m_lights[i].Transform == _lightToRemove)
			{
				this.m_lights[i].LightType = EFogVolumeLightType.None;
				int currentLightCount = this.CurrentLightCount;
				this.CurrentLightCount = currentLightCount - 1;
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600009A RID: 154 RVA: 0x000198CC File Offset: 0x00017ACC
	public void ManualUpdate(ref Plane[] _frustumPlanes)
	{
		this.FrustumPlanes = _frustumPlanes;
		this.m_camera = ((this.m_fogVolumeData != null) ? this.m_fogVolumeData.GameCamera : null);
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

	// Token: 0x0600009B RID: 155 RVA: 0x00019978 File Offset: 0x00017B78
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

	// Token: 0x0600009C RID: 156 RVA: 0x00002C5B File Offset: 0x00000E5B
	public void SetPointLightCullSizeMultiplier(float _cullSizeMultiplier)
	{
		this.m_pointLightCullSizeMultiplier = _cullSizeMultiplier;
	}

	// Token: 0x0600009D RID: 157 RVA: 0x00002C64 File Offset: 0x00000E64
	public void SetPointOfInterest(Vector3 _pointOfInterest)
	{
		this.m_pointOfInterestTf = null;
		this.m_pointOfInterest = _pointOfInterest;
	}

	// Token: 0x0600009E RID: 158 RVA: 0x00002C74 File Offset: 0x00000E74
	public void SetPointOfInterest(Transform _pointOfInterest)
	{
		this.m_pointOfInterestTf = _pointOfInterest;
	}

	// Token: 0x0600009F RID: 159 RVA: 0x00002C7D File Offset: 0x00000E7D
	public Vector4[] GetLightPositionArray()
	{
		return this.m_lightPos;
	}

	// Token: 0x060000A0 RID: 160 RVA: 0x00002C85 File Offset: 0x00000E85
	public Vector4[] GetLightRotationArray()
	{
		return this.m_lightRot;
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x00002C8D File Offset: 0x00000E8D
	public Color[] GetLightColorArray()
	{
		return this.m_lightColor;
	}

	// Token: 0x060000A2 RID: 162 RVA: 0x00002C95 File Offset: 0x00000E95
	public Vector4[] GetLightData()
	{
		return this.m_lightData;
	}

	// Token: 0x060000A3 RID: 163 RVA: 0x00019A88 File Offset: 0x00017C88
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

	// Token: 0x060000A4 RID: 164 RVA: 0x00002C9D File Offset: 0x00000E9D
	public void Deinitialize()
	{
		this.VisibleLightCount = 0;
		this.DrawDebugData = false;
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x00002CAD File Offset: 0x00000EAD
	public void SetFrustumPlanes(ref Plane[] _frustumPlanes)
	{
		this.FrustumPlanes = _frustumPlanes;
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x00019B28 File Offset: 0x00017D28
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

	// Token: 0x060000A7 RID: 167 RVA: 0x00019CE8 File Offset: 0x00017EE8
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

	// Token: 0x060000A8 RID: 168 RVA: 0x00019D30 File Offset: 0x00017F30
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
			if (this.m_lights[i].LightType != EFogVolumeLightType.None && (this.m_lights[i].Transform.position - position).magnitude <= this.m_fogVolume.PointLightingDistance2Camera)
			{
				switch (this.m_lights[i].LightType)
				{
				case EFogVolumeLightType.None:
					goto IL_020C;
				case EFogVolumeLightType.FogVolumePointLight:
					if (!this.m_lights[i].FogVolumeLight.Enabled)
					{
						goto IL_020C;
					}
					break;
				case EFogVolumeLightType.FogVolumeSpotLight:
					if (!this.m_lights[i].FogVolumeLight.Enabled)
					{
						goto IL_020C;
					}
					break;
				case EFogVolumeLightType.PointLight:
				case EFogVolumeLightType.SpotLight:
					if (!this.m_lights[i].Light.enabled)
					{
						goto IL_020C;
					}
					break;
				}
				if (GeometryUtility.TestPlanesAABB(this.FrustumPlanes, this.m_lights[i].Bounds))
				{
					FogVolumeLightManager.LightData lightData = this.m_lights[i];
					Vector3 position2 = lightData.Transform.position;
					lightData.SqDistance = (position2 - this.m_pointOfInterest).sqrMagnitude;
					lightData.Distance2Camera = (position2 - position).magnitude;
					List<FogVolumeLightManager.LightData> lightsInFrustum = this.m_lightsInFrustum;
					int inFrustumCount = this.m_inFrustumCount;
					this.m_inFrustumCount = inFrustumCount + 1;
					lightsInFrustum[inFrustumCount] = lightData;
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
			IL_020C:;
		}
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x00019F54 File Offset: 0x00018154
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

	// Token: 0x060000AA RID: 170 RVA: 0x00019FDC File Offset: 0x000181DC
	private void _PrepareShaderArrays()
	{
		this.VisibleLightCount = 0;
		int num = 0;
		while (num < 64 && num < this.m_inFrustumCount)
		{
			FogVolumeLightManager.LightData lightData = this.m_lightsInFrustum[num];
			switch (lightData.LightType)
			{
			case EFogVolumeLightType.FogVolumePointLight:
			{
				FogVolumeLight fogVolumeLight = lightData.FogVolumeLight;
				this.m_lightPos[num] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				this.m_lightRot[num] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				this.m_lightColor[num] = fogVolumeLight.Color;
				this.m_lightData[num] = new Vector4(fogVolumeLight.Intensity * this.m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / this.m_fogVolume.PointLightingDistance2Camera)), fogVolumeLight.Range / 5f, -1f, 0f);
				int num2 = this.VisibleLightCount;
				this.VisibleLightCount = num2 + 1;
				break;
			}
			case EFogVolumeLightType.FogVolumeSpotLight:
			{
				FogVolumeLight fogVolumeLight2 = lightData.FogVolumeLight;
				this.m_lightPos[num] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				this.m_lightRot[num] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				this.m_lightColor[num] = fogVolumeLight2.Color;
				this.m_lightData[num] = new Vector4(fogVolumeLight2.Intensity * this.m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / this.m_fogVolume.PointLightingDistance2Camera)), fogVolumeLight2.Range / 5f, fogVolumeLight2.Angle, 0f);
				int num2 = this.VisibleLightCount;
				this.VisibleLightCount = num2 + 1;
				break;
			}
			case EFogVolumeLightType.PointLight:
			{
				Light light = lightData.Light;
				this.m_lightPos[num] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				this.m_lightRot[num] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				this.m_lightColor[num] = light.color;
				this.m_lightData[num] = new Vector4(light.intensity * this.m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / this.m_fogVolume.PointLightingDistance2Camera)), light.range / 5f, -1f, 0f);
				int num2 = this.VisibleLightCount;
				this.VisibleLightCount = num2 + 1;
				break;
			}
			case EFogVolumeLightType.SpotLight:
			{
				Light light2 = lightData.Light;
				this.m_lightPos[num] = base.gameObject.transform.InverseTransformPoint(lightData.Transform.position);
				this.m_lightRot[num] = base.gameObject.transform.InverseTransformVector(lightData.Transform.forward);
				this.m_lightColor[num] = light2.color;
				this.m_lightData[num] = new Vector4(light2.intensity * this.m_fogVolume.PointLightsIntensity * (1f - Mathf.Clamp01(lightData.Distance2Camera / this.m_fogVolume.PointLightingDistance2Camera)), light2.range / 5f, light2.spotAngle, 0f);
				int num2 = this.VisibleLightCount;
				this.VisibleLightCount = num2 + 1;
				break;
			}
			}
			num++;
		}
	}

	// Token: 0x04000159 RID: 345
	private float m_pointLightCullSizeMultiplier = 1f;

	// Token: 0x0400015A RID: 346
	private FogVolume m_fogVolume;

	// Token: 0x0400015B RID: 347
	private FogVolumeData m_fogVolumeData;

	// Token: 0x0400015C RID: 348
	private Camera m_camera;

	// Token: 0x0400015D RID: 349
	private BoxCollider m_boxCollider;

	// Token: 0x0400015E RID: 350
	private Transform m_pointOfInterestTf;

	// Token: 0x0400015F RID: 351
	private Vector3 m_pointOfInterest = Vector3.zero;

	// Token: 0x04000160 RID: 352
	private readonly Vector4[] m_lightPos = new Vector4[64];

	// Token: 0x04000161 RID: 353
	private readonly Vector4[] m_lightRot = new Vector4[64];

	// Token: 0x04000162 RID: 354
	private readonly Color[] m_lightColor = new Color[64];

	// Token: 0x04000163 RID: 355
	private readonly Vector4[] m_lightData = new Vector4[64];

	// Token: 0x04000164 RID: 356
	private List<FogVolumeLightManager.LightData> m_lights;

	// Token: 0x04000165 RID: 357
	private List<FogVolumeLightManager.LightData> m_lightsInFrustum;

	// Token: 0x04000166 RID: 358
	private int m_inFrustumCount;

	// Token: 0x04000167 RID: 359
	private Plane[] FrustumPlanes;

	// Token: 0x04000168 RID: 360
	private const int InvalidIndex = -1;

	// Token: 0x04000169 RID: 361
	private const int MaxVisibleLights = 64;

	// Token: 0x0400016A RID: 362
	private const float InvalidSpotLightAngle = -1f;

	// Token: 0x0400016B RID: 363
	private const float NoData = 0f;

	// Token: 0x0400016C RID: 364
	private const float PointLightRangeDivider = 5f;

	// Token: 0x0400016D RID: 365
	private const float SpotLightRangeDivider = 5f;

	// Token: 0x0400016E RID: 366
	private const int MaxLightCount = 1000;

	// Token: 0x0400016F RID: 367
	private const float LightInVolumeBoundsSize = 5f;

	// Token: 0x02000020 RID: 32
	protected class LightData
	{
		// Token: 0x060000AC RID: 172 RVA: 0x0001A42C File Offset: 0x0001862C
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

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00002CB7 File Offset: 0x00000EB7
		// (set) Token: 0x060000AE RID: 174 RVA: 0x00002CBF File Offset: 0x00000EBF
		public EFogVolumeLightType LightType { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00002CC8 File Offset: 0x00000EC8
		// (set) Token: 0x060000B0 RID: 176 RVA: 0x00002CD0 File Offset: 0x00000ED0
		public Light Light { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00002CD9 File Offset: 0x00000ED9
		// (set) Token: 0x060000B2 RID: 178 RVA: 0x00002CE1 File Offset: 0x00000EE1
		public FogVolumeLight FogVolumeLight { get; set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x00002CEA File Offset: 0x00000EEA
		// (set) Token: 0x060000B4 RID: 180 RVA: 0x00002CF2 File Offset: 0x00000EF2
		public Transform Transform { get; set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x00002CFB File Offset: 0x00000EFB
		// (set) Token: 0x060000B6 RID: 182 RVA: 0x00002D03 File Offset: 0x00000F03
		public float SqDistance { get; set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x00002D0C File Offset: 0x00000F0C
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x00002D14 File Offset: 0x00000F14
		public float Distance2Camera { get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x00002D1D File Offset: 0x00000F1D
		// (set) Token: 0x060000BA RID: 186 RVA: 0x00002D25 File Offset: 0x00000F25
		public Bounds Bounds { get; set; }
	}
}
