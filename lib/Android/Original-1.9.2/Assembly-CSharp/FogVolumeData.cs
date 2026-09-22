using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200000E RID: 14
[ExecuteInEditMode]
public class FogVolumeData : MonoBehaviour
{
	// Token: 0x1700000A RID: 10
	// (get) Token: 0x0600004A RID: 74 RVA: 0x00006861 File Offset: 0x00004C61
	// (set) Token: 0x0600004B RID: 75 RVA: 0x00006869 File Offset: 0x00004C69
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

	// Token: 0x1700000B RID: 11
	// (get) Token: 0x0600004C RID: 76 RVA: 0x00006884 File Offset: 0x00004C84
	// (set) Token: 0x0600004D RID: 77 RVA: 0x0000688C File Offset: 0x00004C8C
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

	// Token: 0x0600004E RID: 78 RVA: 0x000068AC File Offset: 0x00004CAC
	public void setDownsample(int val)
	{
		if (this._GameCamera.GetComponent<FogVolumeRenderer>())
		{
			this._GameCamera.GetComponent<FogVolumeRenderer>()._Downsample = val;
		}
	}

	// Token: 0x0600004F RID: 79 RVA: 0x000068D4 File Offset: 0x00004CD4
	private void RefreshCamera()
	{
		this.FindFogVolumes();
		foreach (FogVolume fogVolume in this.SceneFogVolumes)
		{
			fogVolume.AssignCamera();
		}
		this.ToggleFogVolumeRenderers();
	}

	// Token: 0x06000050 RID: 80 RVA: 0x00006912 File Offset: 0x00004D12
	private void OnEnable()
	{
		this.Initialize();
	}

	// Token: 0x06000051 RID: 81 RVA: 0x0000691A File Offset: 0x00004D1A
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

	// Token: 0x06000052 RID: 82 RVA: 0x00006958 File Offset: 0x00004D58
	public void FindFogVolumes()
	{
		this.SceneFogVolumes = (FogVolume[])global::UnityEngine.Object.FindObjectsOfType(typeof(FogVolume));
	}

	// Token: 0x06000053 RID: 83 RVA: 0x00006974 File Offset: 0x00004D74
	private void Update()
	{
		if (this.GameCamera == null)
		{
			Debug.Log("No Camera available for Fog Volume. Trying to find another one");
			this.Initialize();
		}
	}

	// Token: 0x06000054 RID: 84 RVA: 0x00006998 File Offset: 0x00004D98
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
							goto IL_00FE;
						}
						fogVolumeRenderer = this.FoundCameras[i].gameObject.AddComponent<FogVolumeRenderer>();
					}
					if (this.ForceNoRenderer)
					{
						fogVolumeRenderer.enabled = false;
					}
				}
				IL_00FE:;
			}
		}
	}

	// Token: 0x06000055 RID: 85 RVA: 0x00006AB8 File Offset: 0x00004EB8
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
		if (this.GameCamera != null)
		{
			if (global::UnityEngine.Object.FindObjectOfType<FogVolumeCamera>())
			{
				global::UnityEngine.Object.FindObjectOfType<FogVolumeCamera>().SceneCamera = this.GameCamera;
			}
			this.GameCamera.depthTextureMode = DepthTextureMode.Depth;
		}
	}

	// Token: 0x1700000C RID: 12
	// (get) Token: 0x06000056 RID: 86 RVA: 0x00006C4C File Offset: 0x0000504C
	public Camera GetFogVolumeCamera
	{
		get
		{
			return this.GameCamera;
		}
	}

	// Token: 0x06000057 RID: 87 RVA: 0x00006C54 File Offset: 0x00005054
	private void OnDisable()
	{
		this.FoundCameras.Clear();
		this.SceneFogVolumes = null;
	}

	// Token: 0x040000F0 RID: 240
	[SerializeField]
	private bool _ForceNoRenderer;

	// Token: 0x040000F1 RID: 241
	[SerializeField]
	private Camera _GameCamera;

	// Token: 0x040000F2 RID: 242
	[SerializeField]
	private List<Camera> FoundCameras;

	// Token: 0x040000F3 RID: 243
	[SerializeField]
	private FogVolume[] SceneFogVolumes;
}
