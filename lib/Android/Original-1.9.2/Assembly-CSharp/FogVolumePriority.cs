using System;
using UnityEngine;

// Token: 0x02000025 RID: 37
[ExecuteInEditMode]
public class FogVolumePriority : MonoBehaviour
{
	// Token: 0x06000102 RID: 258 RVA: 0x0000C25C File Offset: 0x0000A65C
	private void OnEnable()
	{
		this.thisFog = base.GetComponent<FogVolume>();
		this._FogVolumeData = this.thisFog._FogVolumeData;
	}

	// Token: 0x06000103 RID: 259 RVA: 0x0000C27C File Offset: 0x0000A67C
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
			}
			else
			{
				this.thisFog.DrawOrder = this.FogOrderCameraAbove;
			}
		}
	}

	// Token: 0x040001EA RID: 490
	public Camera GameCamera;

	// Token: 0x040001EB RID: 491
	public int FogOrderCameraAbove = 1;

	// Token: 0x040001EC RID: 492
	public int FogOrderCameraBelow = -1;

	// Token: 0x040001ED RID: 493
	public float HeightThreshold = 30f;

	// Token: 0x040001EE RID: 494
	public FogVolume thisFog;

	// Token: 0x040001EF RID: 495
	private FogVolumeData _FogVolumeData;

	// Token: 0x040001F0 RID: 496
	public float CurrentHeight;

	// Token: 0x040001F1 RID: 497
	public GameObject Horizon;
}
