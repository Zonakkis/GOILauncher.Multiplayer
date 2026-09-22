using System;
using UnityEngine;

// Token: 0x0200004B RID: 75
public class SetFogQuality : MonoBehaviour
{
	// Token: 0x06000260 RID: 608 RVA: 0x00016693 File Offset: 0x00014893
	private void Start()
	{
		this.fog = base.GetComponent<FogVolume>();
		this.fogrenderer = Object.FindObjectOfType<FogVolumeRenderer>();
		this.SetQuality(QualitySettings.GetQualityLevel());
	}

	// Token: 0x06000261 RID: 609 RVA: 0x000166B8 File Offset: 0x000148B8
	public void SetQuality(int quality)
	{
		Debug.Log("new quality: " + quality.ToString());
		if (quality == 0)
		{
			this.fog.enabled = false;
			this.fog.GetComponent<MeshRenderer>().enabled = false;
			this.fogrenderer.enabled = false;
		}
		else
		{
			this.fog.enabled = true;
			this.fog.GetComponent<MeshRenderer>().enabled = true;
			this.fogrenderer.enabled = true;
		}
		if (quality < QualitySettings.names.Length / 2)
		{
			this.fogrenderer._Downsample = this.sampleAmounts[0];
		}
		else if (quality < QualitySettings.names.Length - 1)
		{
			this.fogrenderer._Downsample = this.sampleAmounts[1];
		}
		else
		{
			this.fogrenderer._Downsample = this.sampleAmounts[2];
		}
		if (this.fog != null)
		{
			string name = base.gameObject.name;
			if (name != null)
			{
				if (name == "Stratus")
				{
					this.fog.Iterations = this.stratusIterations[quality];
					return;
				}
				if (name == "Cumulus")
				{
					this.fog.Iterations = this.cumulusIterations[quality];
					return;
				}
				if (name == "SpaceCloud")
				{
					this.fog.Iterations = this.spaceCloudIterations[quality];
					return;
				}
			}
			this.fog.Iterations = this.defaultIterations[quality];
		}
	}

	// Token: 0x06000262 RID: 610 RVA: 0x0001681F File Offset: 0x00014A1F
	private void Update()
	{
	}

	// Token: 0x040003F1 RID: 1009
	private FogVolume fog;

	// Token: 0x040003F2 RID: 1010
	private FogVolumeRenderer fogrenderer;

	// Token: 0x040003F3 RID: 1011
	public int[] sampleAmounts = new int[] { 8, 6, 4 };

	// Token: 0x040003F4 RID: 1012
	private int[] stratusIterations = new int[] { 20, 30, 30, 30, 40, 40 };

	// Token: 0x040003F5 RID: 1013
	private int[] cumulusIterations = new int[] { 15, 25, 30, 35, 50, 60 };

	// Token: 0x040003F6 RID: 1014
	private int[] spaceCloudIterations = new int[] { 10, 20, 20, 30, 40, 40 };

	// Token: 0x040003F7 RID: 1015
	private int[] defaultIterations = new int[] { 10, 15, 15, 15, 25, 25, 30 };
}
