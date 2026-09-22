using System;
using UnityEngine;

// Token: 0x0200002F RID: 47
[ExecuteInEditMode]
public class FogVolumePriority : MonoBehaviour
{
	// Token: 0x06000135 RID: 309 RVA: 0x0000317B File Offset: 0x0000137B
	private void OnEnable()
	{
		this.thisFog = base.GetComponent<FogVolume>();
		this._FogVolumeData = this.thisFog._FogVolumeData;
	}

	// Token: 0x06000136 RID: 310 RVA: 0x0001DD08 File Offset: 0x0001BF08
	private void Update()
	{
		if (this.Horizon)
		{
			this.HeightThreshold = this.Horizon.transform.position.y;
		}
		if (this._FogVolumeData)
		{
			this.GameCamera = this._FogVolumeData.GameCamera;
		}
		else
		{
			this.GameCamera = Camera.main;
		}
		if (this.GameCamera)
		{
			if (!Application.isPlaying)
			{
				if (Camera.current != null)
				{
					this.CurrentHeight = Camera.current.gameObject.transform.position.y;
				}
			}
			else
			{
				this.CurrentHeight = this.GameCamera.gameObject.transform.position.y;
			}
			if (this.HeightThreshold > this.CurrentHeight && this.Horizon != null)
			{
				this.thisFog.DrawOrder = this.FogOrderCameraBelow;
				return;
			}
			this.thisFog.DrawOrder = this.FogOrderCameraAbove;
		}
	}

	// Token: 0x04000221 RID: 545
	public Camera GameCamera;

	// Token: 0x04000222 RID: 546
	public int FogOrderCameraAbove = 1;

	// Token: 0x04000223 RID: 547
	public int FogOrderCameraBelow = -1;

	// Token: 0x04000224 RID: 548
	public float HeightThreshold = 30f;

	// Token: 0x04000225 RID: 549
	public FogVolume thisFog;

	// Token: 0x04000226 RID: 550
	private FogVolumeData _FogVolumeData;

	// Token: 0x04000227 RID: 551
	public float CurrentHeight;

	// Token: 0x04000228 RID: 552
	public GameObject Horizon;
}
