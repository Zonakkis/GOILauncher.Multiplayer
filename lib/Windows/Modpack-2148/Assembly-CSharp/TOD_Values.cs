using System;
using UnityEngine;

// Token: 0x0200000A RID: 10
[ExecuteInEditMode]
public class TOD_Values : MonoBehaviour
{
	// Token: 0x06000030 RID: 48 RVA: 0x00002807 File Offset: 0x00000A07
	private void OnEnable()
	{
		this.Sun = base.gameObject.GetComponent<FogVolume>().Sun;
	}

	// Token: 0x06000031 RID: 49 RVA: 0x00014FA4 File Offset: 0x000131A4
	private void Update()
	{
		if (this.Active && this.Sun)
		{
			this.Sun.transform.eulerAngles = this.SunDirection;
			this.Sun.color = this.SunColor;
			this.Sun.intensity = this.SunIntensity;
			RenderSettings.ambientLight = this.ambientLight;
		}
	}

	// Token: 0x0400003B RID: 59
	public bool Active;

	// Token: 0x0400003C RID: 60
	public Color ambientLight;

	// Token: 0x0400003D RID: 61
	public Vector3 SunDirection = Vector3.zero;

	// Token: 0x0400003E RID: 62
	private Light Sun;

	// Token: 0x0400003F RID: 63
	public float SunIntensity = 1f;

	// Token: 0x04000040 RID: 64
	public Color SunColor = Color.white;
}
