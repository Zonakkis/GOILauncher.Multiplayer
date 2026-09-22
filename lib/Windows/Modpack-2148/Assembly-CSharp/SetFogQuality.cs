using System;
using UnityEngine;

// Token: 0x02000074 RID: 116
public class SetFogQuality : MonoBehaviour
{
	// Token: 0x060002FD RID: 765 RVA: 0x00004128 File Offset: 0x00002328
	private void Start()
	{
		this.fog = base.GetComponent<FogVolume>();
		this.fogrenderer = global::UnityEngine.Object.FindObjectOfType<FogVolumeRenderer>();
		this.SetQuality(QualitySettings.GetQualityLevel());
	}

	// Token: 0x060002FE RID: 766 RVA: 0x00028CE4 File Offset: 0x00026EE4
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

	// Token: 0x060002FF RID: 767 RVA: 0x0000265E File Offset: 0x0000085E
	private void Update()
	{
	}

	// Token: 0x040004A3 RID: 1187
	private FogVolume fog;

	// Token: 0x040004A4 RID: 1188
	private FogVolumeRenderer fogrenderer;

	// Token: 0x040004A5 RID: 1189
	public int[] sampleAmounts = new int[] { 8, 6, 4 };

	// Token: 0x040004A6 RID: 1190
	private int[] stratusIterations = new int[] { 20, 30, 30, 30, 40, 40 };

	// Token: 0x040004A7 RID: 1191
	private int[] cumulusIterations = new int[] { 15, 25, 30, 35, 50, 60 };

	// Token: 0x040004A8 RID: 1192
	private int[] spaceCloudIterations = new int[] { 10, 20, 20, 30, 40, 40 };

	// Token: 0x040004A9 RID: 1193
	private int[] defaultIterations = new int[] { 10, 15, 15, 15, 25, 25, 30 };
}
