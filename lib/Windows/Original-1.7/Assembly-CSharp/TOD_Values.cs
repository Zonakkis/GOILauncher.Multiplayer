using System;
using UnityEngine;

// Token: 0x02000009 RID: 9
[ExecuteInEditMode]
public class TOD_Values : MonoBehaviour
{
	// Token: 0x0600002F RID: 47 RVA: 0x000030EE File Offset: 0x000012EE
	private void OnEnable()
	{
		this.Sun = base.gameObject.GetComponent<FogVolume>().Sun;
	}

	// Token: 0x06000030 RID: 48 RVA: 0x00003108 File Offset: 0x00001308
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

	// Token: 0x04000039 RID: 57
	public bool Active;

	// Token: 0x0400003A RID: 58
	public Color ambientLight;

	// Token: 0x0400003B RID: 59
	public Vector3 SunDirection = Vector3.zero;

	// Token: 0x0400003C RID: 60
	private Light Sun;

	// Token: 0x0400003D RID: 61
	public float SunIntensity = 1f;

	// Token: 0x0400003E RID: 62
	public Color SunColor = Color.white;
}
