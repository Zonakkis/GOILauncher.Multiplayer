using System;
using UnityEngine;

// Token: 0x0200001C RID: 28
[ExecuteInEditMode]
public class FogVolumePriority : MonoBehaviour
{
	// Token: 0x06000113 RID: 275 RVA: 0x0000C60F File Offset: 0x0000A80F
	private void OnEnable()
	{
		this.thisFog = base.GetComponent<FogVolume>();
		this._FogVolumeData = this.thisFog._FogVolumeData;
	}

	// Token: 0x06000114 RID: 276 RVA: 0x0000C630 File Offset: 0x0000A830
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

	// Token: 0x040001D4 RID: 468
	public Camera GameCamera;

	// Token: 0x040001D5 RID: 469
	public int FogOrderCameraAbove = 1;

	// Token: 0x040001D6 RID: 470
	public int FogOrderCameraBelow = -1;

	// Token: 0x040001D7 RID: 471
	public float HeightThreshold = 30f;

	// Token: 0x040001D8 RID: 472
	public FogVolume thisFog;

	// Token: 0x040001D9 RID: 473
	private FogVolumeData _FogVolumeData;

	// Token: 0x040001DA RID: 474
	public float CurrentHeight;

	// Token: 0x040001DB RID: 475
	public GameObject Horizon;
}
