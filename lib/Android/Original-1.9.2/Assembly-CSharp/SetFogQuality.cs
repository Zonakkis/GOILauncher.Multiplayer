using System;
using UnityEngine;

// Token: 0x02000145 RID: 325
public class SetFogQuality : MonoBehaviour
{
	// Token: 0x06000900 RID: 2304 RVA: 0x00049EE6 File Offset: 0x000482E6
	private void Start()
	{
	}

	// Token: 0x06000901 RID: 2305 RVA: 0x00049EE8 File Offset: 0x000482E8
	public void SetQuality(int quality)
	{
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
		FogVolume component = base.GetComponent<FogVolume>();
		if (component != null)
		{
			string name = base.gameObject.name;
			if (name != null)
			{
				if (name == "Stratus")
				{
					component.Iterations = this.stratusIterations[quality];
					return;
				}
				if (name == "Cumulus")
				{
					component.Iterations = this.cumulusIterations[quality];
					return;
				}
				if (name == "SpaceCloud")
				{
					component.Iterations = this.spaceCloudIterations[quality];
					return;
				}
			}
			component.Iterations = this.defaultIterations[quality];
		}
	}

	// Token: 0x06000902 RID: 2306 RVA: 0x00049FFC File Offset: 0x000483FC
	private void Update()
	{
	}

	// Token: 0x04000885 RID: 2181
	private FogVolume fog;

	// Token: 0x04000886 RID: 2182
	private FogVolumeRenderer fogrenderer;

	// Token: 0x04000887 RID: 2183
	public int[] sampleAmounts = new int[] { 8, 6, 4 };

	// Token: 0x04000888 RID: 2184
	private int[] stratusIterations = new int[] { 20, 30, 30, 30, 40, 40 };

	// Token: 0x04000889 RID: 2185
	private int[] cumulusIterations = new int[] { 15, 25, 30, 35, 50, 60 };

	// Token: 0x0400088A RID: 2186
	private int[] spaceCloudIterations = new int[] { 10, 20, 20, 30, 40, 40 };

	// Token: 0x0400088B RID: 2187
	private int[] defaultIterations = new int[] { 10, 15, 15, 15, 25, 25, 30 };
}
