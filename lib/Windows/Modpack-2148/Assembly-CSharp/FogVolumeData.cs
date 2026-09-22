using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000017 RID: 23
[ExecuteInEditMode]
public class FogVolumeData : MonoBehaviour
{
	// Token: 0x1700000F RID: 15
	// (get) Token: 0x0600006B RID: 107 RVA: 0x00002A47 File Offset: 0x00000C47
	// (set) Token: 0x0600006C RID: 108 RVA: 0x00002A4F File Offset: 0x00000C4F
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
	// (get) Token: 0x0600006D RID: 109 RVA: 0x00002A67 File Offset: 0x00000C67
	// (set) Token: 0x0600006E RID: 110 RVA: 0x00002A6F File Offset: 0x00000C6F
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

	// Token: 0x0600006F RID: 111 RVA: 0x00002A8C File Offset: 0x00000C8C
	public void setDownsample(int val)
	{
		if (this._GameCamera.GetComponent<FogVolumeRenderer>())
		{
			this._GameCamera.GetComponent<FogVolumeRenderer>()._Downsample = val;
		}
	}

	// Token: 0x06000070 RID: 112 RVA: 0x00018384 File Offset: 0x00016584
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

	// Token: 0x06000071 RID: 113 RVA: 0x00002AB1 File Offset: 0x00000CB1
	private void OnEnable()
	{
		this.Initialize();
	}

	// Token: 0x06000072 RID: 114 RVA: 0x00002AB9 File Offset: 0x00000CB9
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

	// Token: 0x06000073 RID: 115 RVA: 0x00002AF1 File Offset: 0x00000CF1
	public void FindFogVolumes()
	{
		this.SceneFogVolumes = (FogVolume[])global::UnityEngine.Object.FindObjectsOfType(typeof(FogVolume));
	}

	// Token: 0x06000074 RID: 116 RVA: 0x00002B0D File Offset: 0x00000D0D
	private void Update()
	{
		if (this.GameCamera == null)
		{
			Debug.Log("No Camera available for Fog Volume. Trying to find another one");
			this.Initialize();
		}
	}

	// Token: 0x06000075 RID: 117 RVA: 0x000183BC File Offset: 0x000165BC
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

	// Token: 0x06000076 RID: 118 RVA: 0x000184C4 File Offset: 0x000166C4
	public void FindCamera()
	{
		if (this.FoundCameras != null && this.FoundCameras.Count > 0)
		{
			this.FoundCameras.Clear();
		}
		Camera[] array = (Camera[])global::UnityEngine.Object.FindObjectsOfType(typeof(Camera));
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
	// (get) Token: 0x06000077 RID: 119 RVA: 0x00002B2D File Offset: 0x00000D2D
	public Camera GetFogVolumeCamera
	{
		get
		{
			return this.GameCamera;
		}
	}

	// Token: 0x06000078 RID: 120 RVA: 0x00002B35 File Offset: 0x00000D35
	private void OnDisable()
	{
		this.FoundCameras.Clear();
		this.SceneFogVolumes = null;
	}

	// Token: 0x04000115 RID: 277
	[SerializeField]
	private bool _ForceNoRenderer;

	// Token: 0x04000116 RID: 278
	[SerializeField]
	private Camera _GameCamera;

	// Token: 0x04000117 RID: 279
	[SerializeField]
	private List<Camera> FoundCameras;

	// Token: 0x04000118 RID: 280
	[SerializeField]
	private FogVolume[] SceneFogVolumes;
}
