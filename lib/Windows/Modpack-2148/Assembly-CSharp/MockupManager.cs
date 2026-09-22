using System;
using UnityEngine;

// Token: 0x02000007 RID: 7
[ExecuteInEditMode]
public class MockupManager : MonoBehaviour
{
	// Token: 0x0600001E RID: 30 RVA: 0x000145F8 File Offset: 0x000127F8
	private void OnEnable()
	{
		this.BoundariesCollider = new BoxCollider[2];
		this.BoundariesCollider[0] = this.BoundariesExterior.GetComponent<BoxCollider>();
		this.BoundariesCollider[1] = this.BoundariesInterior.GetComponent<BoxCollider>();
		this.GameCamera = Camera.main;
		this.InitialFOV = this.GameCamera.fieldOfView;
		this._ExplorationCamera = this.GameCamera.GetComponent<ExplorationCamera>();
		this._FogVolumeRenderer = Camera.main.GetComponent<FogVolumeRenderer>();
		if (this.CameraRoot)
		{
			this._Rotator = this.CameraRoot.GetComponent<Rotator>();
		}
		if (this._ShadowCaster)
		{
			this.AtmosphereSizeOutside = this._ShadowCaster.fogVolumeScale.x;
		}
		this.SceneReflectionProbes = global::UnityEngine.Object.FindObjectsOfType(typeof(ReflectionProbe)) as ReflectionProbe[];
		this.ObjectsToToggleDefaultVisible = new bool[this.ObjectsToToggle.Length];
		for (int i = 0; i < this.ObjectsToToggle.Length; i++)
		{
			this.ObjectsToToggleDefaultVisible[i] = this.ObjectsToToggle[i].activeInHierarchy;
		}
		this.SetupOverlayEffect();
		this._ExplorationCamera.enabled = true;
	}

	// Token: 0x0600001F RID: 31 RVA: 0x0001471C File Offset: 0x0001291C
	private void PointIsInsideVolume(Vector3 PointPosition)
	{
		bool flag = false;
		Vector3 vector = Vector3.zero;
		GameObject gameObject;
		if (this.isInside)
		{
			gameObject = this.BoundariesInterior;
			vector = this.BoundariesInterior.transform.localScale;
		}
		else
		{
			gameObject = this.BoundariesExterior;
			vector = this.BoundariesExterior.transform.localScale;
		}
		float num = gameObject.transform.position.x + vector.x / 2f;
		float num2 = gameObject.transform.position.x - vector.x / 2f;
		float num3 = gameObject.transform.position.y + vector.y / 2f;
		float num4 = gameObject.transform.position.y - vector.y / 2f;
		float num5 = gameObject.transform.position.z + vector.z / 2f;
		float num6 = gameObject.transform.position.z - vector.z / 2f;
		if (num > PointPosition.x && num2 < PointPosition.x && num3 > PointPosition.y && num4 < PointPosition.y && num5 > PointPosition.z && num6 < PointPosition.z)
		{
			flag = true;
		}
		this._CurrentPlayableBoundingBox.max = new Vector3(num, num3, num3);
		this._CurrentPlayableBoundingBox.min = new Vector3(num2, num4, num4);
		this.isInside = flag;
	}

	// Token: 0x06000020 RID: 32 RVA: 0x00014894 File Offset: 0x00012A94
	private void ToggleObjects()
	{
		for (int i = 0; i < this.ObjectsToToggle.Length; i++)
		{
			if (this.isInside)
			{
				this.ObjectsToToggle[i].SetActive(!this.ObjectsToToggleDefaultVisible[i]);
			}
			else
			{
				this.ObjectsToToggle[i].SetActive(this.ObjectsToToggleDefaultVisible[i]);
			}
		}
	}

	// Token: 0x06000021 RID: 33 RVA: 0x000148EC File Offset: 0x00012AEC
	private void SetupOverlayEffect()
	{
		this.FadeEffect = this.GameCamera.GetComponent<Fade>();
		if (this.FadeEffect == null)
		{
			this.FadeEffect = this.GameCamera.gameObject.AddComponent<Fade>();
			this.FadeEffect._Color.a = 0f;
		}
	}

	// Token: 0x06000022 RID: 34 RVA: 0x00014944 File Offset: 0x00012B44
	private void Fade()
	{
		if (this.FadeEffect)
		{
			this.fade = Mathf.Lerp(this.fade, 0f, 0.1f);
			this.FadeEffect._Color.a = this.fade;
			return;
		}
		Debug.LogError("Fade effect not set");
	}

	// Token: 0x06000023 RID: 35 RVA: 0x00002738 File Offset: 0x00000938
	private void Teleport()
	{
		this.GameCamera.transform.localRotation = this.RespawnPoint.localRotation;
		this.GameCamera.transform.localPosition = this.RespawnPoint.localPosition;
	}

	// Token: 0x06000024 RID: 36 RVA: 0x0001499C File Offset: 0x00012B9C
	private void OnBoxExit()
	{
		this.fade = 1f;
		this.GameCamera.transform.parent.eulerAngles = Vector3.zero;
		this.exit = true;
		this._ExplorationCamera.enabled = false;
		this.Teleport();
		this.GameCamera.fieldOfView = this.InitialFOV;
		this._ExplorationCamera.FOV = this.InitialFOV;
		this._ExplorationCamera.tilt = 0f;
		this._ExplorationCamera.Speed = this._ExplorationCamera.InitialSpeed;
		this._ExplorationCamera.enabled = true;
	}

	// Token: 0x06000025 RID: 37 RVA: 0x00002770 File Offset: 0x00000970
	private void OnBoxEnter()
	{
		this.enter = true;
		this.fade = 1f;
	}

	// Token: 0x06000026 RID: 38 RVA: 0x00014A3C File Offset: 0x00012C3C
	private Vector3 ClosestPoint(MockupManager.BoundingBox _BoundingBox, Vector3 Point)
	{
		Vector3 vector = default(Vector3);
		if (Point.x > _BoundingBox.max.x)
		{
			vector.x = _BoundingBox.max.x;
		}
		else if (Point.x < _BoundingBox.min.x)
		{
			vector.x = _BoundingBox.min.x;
		}
		else
		{
			vector.x = Point.x;
		}
		if (Point.y > _BoundingBox.max.y)
		{
			vector.y = _BoundingBox.max.y;
		}
		else if (Point.y < _BoundingBox.min.y)
		{
			vector.y = _BoundingBox.min.y;
		}
		else
		{
			vector.y = Point.y;
		}
		if (Point.z > _BoundingBox.max.z)
		{
			vector.z = _BoundingBox.max.z;
		}
		else if (Point.z < _BoundingBox.min.z)
		{
			vector.z = _BoundingBox.min.z;
		}
		else
		{
			vector.z = Point.z;
		}
		return vector;
	}

	// Token: 0x06000027 RID: 39 RVA: 0x00014B64 File Offset: 0x00012D64
	private void BoundariesDistanceCheck()
	{
		if (this.isInside)
		{
			float num = Vector3.Magnitude(this.CurrentCameraPosition - this.BoundariesExterior.transform.position);
			num /= this.InteriorRadiusFade;
			num = Mathf.Pow(num, this.RadialPow);
			num = Mathf.Clamp(num, 0f, 1.1f);
			if ((double)num > 0.1)
			{
				this.fade = num;
			}
			if (num > 0.99f)
			{
				this.Teleport();
			}
		}
		this._ClosestPoint = this.ClosestPoint(this._CurrentPlayableBoundingBox, this.CurrentCameraPosition);
		if (this.DebugIntersectionPoint != null)
		{
			this.DebugIntersectionPoint.transform.position = this._ClosestPoint;
		}
	}

	// Token: 0x06000028 RID: 40 RVA: 0x00014C20 File Offset: 0x00012E20
	private void FixedUpdate()
	{
		this.CurrentCameraPosition = this.GameCamera.transform.position;
		if (this._ShadowCaster && this._ExplorationCamera && this._Rotator && this._FogVolumeRenderer && this.GameCamera && this._FogVolume)
		{
			this._ExplorationCamera.FOVTransitionSpeed = this.FOVTransitionSpeed;
			this.PointIsInsideVolume(this.GameCamera.transform.position);
			this.BoundariesDistanceCheck();
			if (this.isInside)
			{
				if (!this.enter)
				{
					this.OnBoxEnter();
				}
				this.Fade();
				this.ToggleObjects();
				this.count++;
				this._ShadowCaster.fogVolumeScale.x = this.AtmosphereSizeInside;
				this._ShadowCaster.fogVolumeScale.x = this.AtmosphereSizeInside;
				this._ShadowCaster.UpdateBoxMesh();
				this.CurrentFOV = Mathf.Lerp(this.CurrentFOV, this.targetFOV, this.FOVTransitionSpeed);
				this._ExplorationCamera.FOV = this.CurrentFOV;
				this._Rotator.enabled = false;
				this._FogVolumeRenderer.enabled = true;
				this.GameCamera.clearFlags = CameraClearFlags.Skybox;
				for (int i = 0; i < this.SceneReflectionProbes.Length; i++)
				{
					this.SceneReflectionProbes[i].enabled = false;
				}
				this.exit = false;
			}
			else
			{
				this.Fade();
				this.enter = false;
				for (int j = 0; j < this.SceneReflectionProbes.Length; j++)
				{
					this.SceneReflectionProbes[j].enabled = true;
				}
				this.ToggleObjects();
				this.GameCamera.clearFlags = CameraClearFlags.Color;
				this._ShadowCaster.fogVolumeScale.x = this.AtmosphereSizeOutside;
				this._ShadowCaster.UpdateBoxMesh();
				if (this.count < 1)
				{
					this._Rotator.enabled = true;
				}
				this.CurrentFOV = this._ExplorationCamera.FOV;
				if (!this.EnableFogVolumeRendererOutsideBox)
				{
					this._FogVolumeRenderer.enabled = false;
				}
				if (this.RespawnPoint != null && !this.exit)
				{
					this.OnBoxExit();
					this._ExplorationCamera.enabled = true;
				}
			}
		}
		else
		{
			MonoBehaviour.print("Config not complete");
		}
		this._ShadowCaster.UpdateBoxMesh();
		this._FogVolume.UpdateBoxMesh();
	}

	// Token: 0x06000029 RID: 41 RVA: 0x00002784 File Offset: 0x00000984
	private void Update()
	{
		if (!this.isInside)
		{
			if (this.EnableFogVolumeRendererOutsideBox)
			{
				this._FogVolumeRenderer.enabled = true;
				return;
			}
			this._FogVolumeRenderer.enabled = false;
		}
	}

	// Token: 0x04000010 RID: 16
	public GameObject CameraRoot;

	// Token: 0x04000011 RID: 17
	public GameObject BoundariesInterior;

	// Token: 0x04000012 RID: 18
	public GameObject BoundariesExterior;

	// Token: 0x04000013 RID: 19
	private Rotator _Rotator;

	// Token: 0x04000014 RID: 20
	private Camera GameCamera;

	// Token: 0x04000015 RID: 21
	public FogVolume _FogVolume;

	// Token: 0x04000016 RID: 22
	public FogVolume _ShadowCaster;

	// Token: 0x04000017 RID: 23
	private ExplorationCamera _ExplorationCamera;

	// Token: 0x04000018 RID: 24
	private FogVolumeRenderer _FogVolumeRenderer;

	// Token: 0x04000019 RID: 25
	public ShadowCamera.TextureSize OutsideShadowResolution = ShadowCamera.TextureSize._256;

	// Token: 0x0400001A RID: 26
	public ShadowCamera.TextureSize InsideShadowResolution = ShadowCamera.TextureSize._128;

	// Token: 0x0400001B RID: 27
	[Range(0.001f, 0.1f)]
	public float FOVTransitionSpeed = 0.01f;

	// Token: 0x0400001C RID: 28
	public bool EnableFogVolumeRendererOutsideBox;

	// Token: 0x0400001D RID: 29
	public float CurrentFOV;

	// Token: 0x0400001E RID: 30
	public float targetFOV = 90f;

	// Token: 0x0400001F RID: 31
	private float _InitialFogVolumeSize;

	// Token: 0x04000020 RID: 32
	public float AtmosphereSizeInside = 100f;

	// Token: 0x04000021 RID: 33
	public float AtmosphereSizeOutside = 49.99f;

	// Token: 0x04000022 RID: 34
	private ReflectionProbe[] SceneReflectionProbes;

	// Token: 0x04000023 RID: 35
	private bool isInside;

	// Token: 0x04000024 RID: 36
	public GameObject[] ObjectsToToggle;

	// Token: 0x04000025 RID: 37
	private bool[] ObjectsToToggleDefaultVisible;

	// Token: 0x04000026 RID: 38
	private int count;

	// Token: 0x04000027 RID: 39
	public float InitialFOV;

	// Token: 0x04000028 RID: 40
	public Transform RespawnPoint;

	// Token: 0x04000029 RID: 41
	private Vector3 CurrentCameraPosition;

	// Token: 0x0400002A RID: 42
	private bool exit;

	// Token: 0x0400002B RID: 43
	private bool enter;

	// Token: 0x0400002C RID: 44
	public GameObject DebugIntersectionPoint;

	// Token: 0x0400002D RID: 45
	private BoxCollider[] BoundariesCollider;

	// Token: 0x0400002E RID: 46
	private MockupManager.BoundingBox _InteriorBox;

	// Token: 0x0400002F RID: 47
	private MockupManager.BoundingBox _CurrentPlayableBoundingBox;

	// Token: 0x04000030 RID: 48
	private float fade;

	// Token: 0x04000031 RID: 49
	public float CameraRotation = 1f;

	// Token: 0x04000032 RID: 50
	private Fade FadeEffect;

	// Token: 0x04000033 RID: 51
	private Vector3 _ClosestPoint;

	// Token: 0x04000034 RID: 52
	public float InteriorRadiusFade = 74.6f;

	// Token: 0x04000035 RID: 53
	public float RadialPow = 10.1f;

	// Token: 0x02000008 RID: 8
	private struct BoundingBox
	{
		// Token: 0x0600002B RID: 43 RVA: 0x00014F18 File Offset: 0x00013118
		public BoundingBox(Vector3 _min, Vector3 _max)
		{
			this.min = Vector3.zero;
			this.max = Vector3.zero;
			this.min.x = _min.x;
			this.min.y = _min.y;
			this.min.x = _min.x;
			this.max.x = _max.x;
			this.max.y = _max.y;
			this.max.x = _max.x;
		}

		// Token: 0x04000036 RID: 54
		public Vector3 min;

		// Token: 0x04000037 RID: 55
		public Vector3 max;
	}
}
