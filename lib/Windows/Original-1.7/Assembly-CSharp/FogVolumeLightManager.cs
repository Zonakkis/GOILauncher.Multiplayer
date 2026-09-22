using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000013 RID: 19
public class FogVolumeLightManager : MonoBehaviour
{
	// Token: 0x17000013 RID: 19
	// (get) Token: 0x06000089 RID: 137 RVA: 0x000078C6 File Offset: 0x00005AC6
	// (set) Token: 0x0600008A RID: 138 RVA: 0x000078CE File Offset: 0x00005ACE
	public int CurrentLightCount { get; private set; }

	// Token: 0x17000014 RID: 20
	// (get) Token: 0x0600008B RID: 139 RVA: 0x000078D7 File Offset: 0x00005AD7
	// (set) Token: 0x0600008C RID: 140 RVA: 0x000078DF File Offset: 0x00005ADF
	public int VisibleLightCount { get; private set; }

	// Token: 0x17000015 RID: 21
	// (get) Token: 0x0600008D RID: 141 RVA: 0x000078E8 File Offset: 0x00005AE8
	// (set) Token: 0x0600008E RID: 142 RVA: 0x000078F0 File Offset: 0x00005AF0
	public bool DrawDebugData { get; set; }

	// Token: 0x17000016 RID: 22
	// (get) Token: 0x0600008F RID: 143 RVA: 0x000078F9 File Offset: 0x00005AF9
	public bool AlreadyUsesTransformForPoI
	{
		get
		{
			return this.m_pointOfInterestTf != null;
		}
	}

	// Token: 0x06000090 RID: 144 RVA: 0x00007908 File Offset: 0x00005B08
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
		FogVolumeLight[] array = Object.FindObjectsOfType<FogVolumeLight>();
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

	// Token: 0x06000091 RID: 145 RVA: 0x000079FC File Offset: 0x00005BFC
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
		FogVolumeLight[] array = Object.FindObjectsOfType<FogVolumeLight>();
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

	// Token: 0x06000092 RID: 146 RVA: 0x00007B54 File Offset: 0x00005D54
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

	// Token: 0x06000093 RID: 147 RVA: 0x00007BE4 File Offset: 0x00005DE4
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

	// Token: 0x06000094 RID: 148 RVA: 0x00007CB4 File Offset: 0x00005EB4
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

	// Token: 0x06000095 RID: 149 RVA: 0x00007D44 File Offset: 0x00005F44
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

	// Token: 0x06000096 RID: 150 RVA: 0x00007E14 File Offset: 0x00006014
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

	// Token: 0x06000097 RID: 151 RVA: 0x00007E74 File Offset: 0x00006074
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

	// Token: 0x06000098 RID: 152 RVA: 0x00007F20 File Offset: 0x00006120
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

	// Token: 0x06000099 RID: 153 RVA: 0x0000802E File Offset: 0x0000622E
	public void SetPointLightCullSizeMultiplier(float _cullSizeMultiplier)
	{
		this.m_pointLightCullSizeMultiplier = _cullSizeMultiplier;
	}

	// Token: 0x0600009A RID: 154 RVA: 0x00008037 File Offset: 0x00006237
	public void SetPointOfInterest(Vector3 _pointOfInterest)
	{
		this.m_pointOfInterestTf = null;
		this.m_pointOfInterest = _pointOfInterest;
	}

	// Token: 0x0600009B RID: 155 RVA: 0x00008047 File Offset: 0x00006247
	public void SetPointOfInterest(Transform _pointOfInterest)
	{
		this.m_pointOfInterestTf = _pointOfInterest;
	}

	// Token: 0x0600009C RID: 156 RVA: 0x00008050 File Offset: 0x00006250
	public Vector4[] GetLightPositionArray()
	{
		return this.m_lightPos;
	}

	// Token: 0x0600009D RID: 157 RVA: 0x00008058 File Offset: 0x00006258
	public Vector4[] GetLightRotationArray()
	{
		return this.m_lightRot;
	}

	// Token: 0x0600009E RID: 158 RVA: 0x00008060 File Offset: 0x00006260
	public Color[] GetLightColorArray()
	{
		return this.m_lightColor;
	}

	// Token: 0x0600009F RID: 159 RVA: 0x00008068 File Offset: 0x00006268
	public Vector4[] GetLightData()
	{
		return this.m_lightData;
	}

	// Token: 0x060000A0 RID: 160 RVA: 0x00008070 File Offset: 0x00006270
	public void Initialize()
	{
		this.m_fogVolume = base.gameObject.GetComponent<FogVolume>();
		this.m_fogVolumeData = Object.FindObjectOfType<FogVolumeData>();
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

	// Token: 0x060000A1 RID: 161 RVA: 0x0000810D File Offset: 0x0000630D
	public void Deinitialize()
	{
		this.VisibleLightCount = 0;
		this.DrawDebugData = false;
	}

	// Token: 0x060000A2 RID: 162 RVA: 0x0000811D File Offset: 0x0000631D
	public void SetFrustumPlanes(ref Plane[] _frustumPlanes)
	{
		this.FrustumPlanes = _frustumPlanes;
	}

	// Token: 0x060000A3 RID: 163 RVA: 0x00008128 File Offset: 0x00006328
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

	// Token: 0x060000A4 RID: 164 RVA: 0x000082E8 File Offset: 0x000064E8
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

	// Token: 0x060000A5 RID: 165 RVA: 0x00008330 File Offset: 0x00006530
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

	// Token: 0x060000A6 RID: 166 RVA: 0x00008554 File Offset: 0x00006754
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

	// Token: 0x060000A7 RID: 167 RVA: 0x000085DC File Offset: 0x000067DC
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

	// Token: 0x0400012B RID: 299
	private float m_pointLightCullSizeMultiplier = 1f;

	// Token: 0x0400012C RID: 300
	private FogVolume m_fogVolume;

	// Token: 0x0400012D RID: 301
	private FogVolumeData m_fogVolumeData;

	// Token: 0x0400012E RID: 302
	private Camera m_camera;

	// Token: 0x0400012F RID: 303
	private BoxCollider m_boxCollider;

	// Token: 0x04000130 RID: 304
	private Transform m_pointOfInterestTf;

	// Token: 0x04000131 RID: 305
	private Vector3 m_pointOfInterest = Vector3.zero;

	// Token: 0x04000132 RID: 306
	private readonly Vector4[] m_lightPos = new Vector4[64];

	// Token: 0x04000133 RID: 307
	private readonly Vector4[] m_lightRot = new Vector4[64];

	// Token: 0x04000134 RID: 308
	private readonly Color[] m_lightColor = new Color[64];

	// Token: 0x04000135 RID: 309
	private readonly Vector4[] m_lightData = new Vector4[64];

	// Token: 0x04000136 RID: 310
	private List<FogVolumeLightManager.LightData> m_lights;

	// Token: 0x04000137 RID: 311
	private List<FogVolumeLightManager.LightData> m_lightsInFrustum;

	// Token: 0x04000138 RID: 312
	private int m_inFrustumCount;

	// Token: 0x04000139 RID: 313
	private Plane[] FrustumPlanes;

	// Token: 0x0400013A RID: 314
	private const int InvalidIndex = -1;

	// Token: 0x0400013B RID: 315
	private const int MaxVisibleLights = 64;

	// Token: 0x0400013C RID: 316
	private const float InvalidSpotLightAngle = -1f;

	// Token: 0x0400013D RID: 317
	private const float NoData = 0f;

	// Token: 0x0400013E RID: 318
	private const float PointLightRangeDivider = 5f;

	// Token: 0x0400013F RID: 319
	private const float SpotLightRangeDivider = 5f;

	// Token: 0x04000140 RID: 320
	private const int MaxLightCount = 1000;

	// Token: 0x04000141 RID: 321
	private const float LightInVolumeBoundsSize = 5f;

	// Token: 0x02000226 RID: 550
	protected class LightData
	{
		// Token: 0x0600162D RID: 5677 RVA: 0x0006AD08 File Offset: 0x00068F08
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

		// Token: 0x170005F2 RID: 1522
		// (get) Token: 0x0600162E RID: 5678 RVA: 0x0006AD5C File Offset: 0x00068F5C
		// (set) Token: 0x0600162F RID: 5679 RVA: 0x0006AD64 File Offset: 0x00068F64
		public EFogVolumeLightType LightType { get; set; }

		// Token: 0x170005F3 RID: 1523
		// (get) Token: 0x06001630 RID: 5680 RVA: 0x0006AD6D File Offset: 0x00068F6D
		// (set) Token: 0x06001631 RID: 5681 RVA: 0x0006AD75 File Offset: 0x00068F75
		public Light Light { get; set; }

		// Token: 0x170005F4 RID: 1524
		// (get) Token: 0x06001632 RID: 5682 RVA: 0x0006AD7E File Offset: 0x00068F7E
		// (set) Token: 0x06001633 RID: 5683 RVA: 0x0006AD86 File Offset: 0x00068F86
		public FogVolumeLight FogVolumeLight { get; set; }

		// Token: 0x170005F5 RID: 1525
		// (get) Token: 0x06001634 RID: 5684 RVA: 0x0006AD8F File Offset: 0x00068F8F
		// (set) Token: 0x06001635 RID: 5685 RVA: 0x0006AD97 File Offset: 0x00068F97
		public Transform Transform { get; set; }

		// Token: 0x170005F6 RID: 1526
		// (get) Token: 0x06001636 RID: 5686 RVA: 0x0006ADA0 File Offset: 0x00068FA0
		// (set) Token: 0x06001637 RID: 5687 RVA: 0x0006ADA8 File Offset: 0x00068FA8
		public float SqDistance { get; set; }

		// Token: 0x170005F7 RID: 1527
		// (get) Token: 0x06001638 RID: 5688 RVA: 0x0006ADB1 File Offset: 0x00068FB1
		// (set) Token: 0x06001639 RID: 5689 RVA: 0x0006ADB9 File Offset: 0x00068FB9
		public float Distance2Camera { get; set; }

		// Token: 0x170005F8 RID: 1528
		// (get) Token: 0x0600163A RID: 5690 RVA: 0x0006ADC2 File Offset: 0x00068FC2
		// (set) Token: 0x0600163B RID: 5691 RVA: 0x0006ADCA File Offset: 0x00068FCA
		public Bounds Bounds { get; set; }
	}
}
