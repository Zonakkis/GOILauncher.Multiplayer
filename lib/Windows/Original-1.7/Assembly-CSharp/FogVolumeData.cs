using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000010 RID: 16
[ExecuteInEditMode]
public class FogVolumeData : MonoBehaviour
{
	// Token: 0x1700000F RID: 15
	// (get) Token: 0x06000068 RID: 104 RVA: 0x00006710 File Offset: 0x00004910
	// (set) Token: 0x06000069 RID: 105 RVA: 0x00006718 File Offset: 0x00004918
	public bool ForceNoRenderer
	{
		get
		{
			return this._ForceNoRenderer;
		}
		set
		{
			if (this._ForceNoRenderer != value)
			{
				this._ForceNoRenderer = value;
				this.ToggleFogVolumeRenderers();
			}
		}
	}

	// Token: 0x17000010 RID: 16
	// (get) Token: 0x0600006A RID: 106 RVA: 0x00006730 File Offset: 0x00004930
	// (set) Token: 0x0600006B RID: 107 RVA: 0x00006738 File Offset: 0x00004938
	public Camera GameCamera
	{
		get
		{
			return this._GameCamera;
		}
		set
		{
			if (this._GameCamera != value)
			{
				this._GameCamera = value;
				this.RefreshCamera();
			}
		}
	}

	// Token: 0x0600006C RID: 108 RVA: 0x00006755 File Offset: 0x00004955
	public void setDownsample(int val)
	{
		if (this._GameCamera.GetComponent<FogVolumeRenderer>())
		{
			this._GameCamera.GetComponent<FogVolumeRenderer>()._Downsample = val;
		}
	}

	// Token: 0x0600006D RID: 109 RVA: 0x0000677C File Offset: 0x0000497C
	private void RefreshCamera()
	{
		this.FindFogVolumes();
		FogVolume[] sceneFogVolumes = this.SceneFogVolumes;
		for (int i = 0; i < sceneFogVolumes.Length; i++)
		{
			sceneFogVolumes[i].AssignCamera();
		}
		this.ToggleFogVolumeRenderers();
	}

	// Token: 0x0600006E RID: 110 RVA: 0x000067B2 File Offset: 0x000049B2
	private void OnEnable()
	{
		this.Initialize();
	}

	// Token: 0x0600006F RID: 111 RVA: 0x000067BA File Offset: 0x000049BA
	private void Initialize()
	{
		if (this.FoundCameras == null)
		{
			this.FoundCameras = new List<Camera>();
		}
		this.FindCamera();
		this.RefreshCamera();
		if (this.FoundCameras.Count == 0)
		{
			Debug.Log("Definetly, no camera available for Fog Volume");
		}
	}

	// Token: 0x06000070 RID: 112 RVA: 0x000067F2 File Offset: 0x000049F2
	public void FindFogVolumes()
	{
		this.SceneFogVolumes = (FogVolume[])Object.FindObjectsOfType(typeof(FogVolume));
	}

	// Token: 0x06000071 RID: 113 RVA: 0x0000680E File Offset: 0x00004A0E
	private void Update()
	{
		if (this.GameCamera == null)
		{
			Debug.Log("No Camera available for Fog Volume. Trying to find another one");
			this.Initialize();
		}
	}

	// Token: 0x06000072 RID: 114 RVA: 0x00006830 File Offset: 0x00004A30
	private void ToggleFogVolumeRenderers()
	{
		if (this.FoundCameras != null)
		{
			for (int i = 0; i < this.FoundCameras.Count; i++)
			{
				if (this.FoundCameras[i] != this._GameCamera)
				{
					if (this.FoundCameras[i].GetComponent<FogVolumeRenderer>())
					{
						this.FoundCameras[i].GetComponent<FogVolumeRenderer>().enabled = false;
					}
				}
				else if (this.FoundCameras[i].GetComponent<FogVolumeRenderer>() && !this._ForceNoRenderer)
				{
					this.FoundCameras[i].GetComponent<FogVolumeRenderer>().enabled = true;
				}
				else
				{
					FogVolumeRenderer fogVolumeRenderer = this.FoundCameras[i].GetComponent<FogVolumeRenderer>();
					if (fogVolumeRenderer == null)
					{
						if (this.ForceNoRenderer)
						{
							goto IL_00E4;
						}
						fogVolumeRenderer = this.FoundCameras[i].gameObject.AddComponent<FogVolumeRenderer>();
					}
					if (this.ForceNoRenderer)
					{
						fogVolumeRenderer.enabled = false;
					}
				}
				IL_00E4:;
			}
		}
	}

	// Token: 0x06000073 RID: 115 RVA: 0x00006938 File Offset: 0x00004B38
	public void FindCamera()
	{
		if (this.FoundCameras != null && this.FoundCameras.Count > 0)
		{
			this.FoundCameras.Clear();
		}
		Camera[] array = (Camera[])Object.FindObjectsOfType(typeof(Camera));
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].name.Contains("FogVolumeCamera") && !array[i].name.Contains("Shadow Camera") && array[i].gameObject.hideFlags == HideFlags.None)
			{
				this.FoundCameras.Add(array[i]);
			}
		}
		if (this.GameCamera == null)
		{
			this.GameCamera = Camera.main;
		}
		if (this.GameCamera == null)
		{
			foreach (Camera camera in this.FoundCameras)
			{
				if (camera.isActiveAndEnabled && camera.gameObject.activeInHierarchy && camera.gameObject.hideFlags == HideFlags.None)
				{
					this.GameCamera = camera;
					break;
				}
			}
		}
		this.GameCamera != null;
	}

	// Token: 0x17000011 RID: 17
	// (get) Token: 0x06000074 RID: 116 RVA: 0x00006A6C File Offset: 0x00004C6C
	public Camera GetFogVolumeCamera
	{
		get
		{
			return this.GameCamera;
		}
	}

	// Token: 0x06000075 RID: 117 RVA: 0x00006A74 File Offset: 0x00004C74
	private void OnDisable()
	{
		this.FoundCameras.Clear();
		this.SceneFogVolumes = null;
	}

	// Token: 0x040000FC RID: 252
	[SerializeField]
	private bool _ForceNoRenderer;

	// Token: 0x040000FD RID: 253
	[SerializeField]
	private Camera _GameCamera;

	// Token: 0x040000FE RID: 254
	[SerializeField]
	private List<Camera> FoundCameras;

	// Token: 0x040000FF RID: 255
	[SerializeField]
	private FogVolume[] SceneFogVolumes;
}
