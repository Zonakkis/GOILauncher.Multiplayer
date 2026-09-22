using System;
using UnityEngine;

// Token: 0x02000123 RID: 291
public class FogControl : MonoBehaviour
{
	// Token: 0x0600077D RID: 1917 RVA: 0x0003F25C File Offset: 0x0003D65C
	private void Start()
	{
		this.topColor = Color.white;
		this.bottomColor = Color.white;
		this.midColor = Color.white;
		this.contrast = 1f;
		this.exposure = 0f;
		this.saturation = 1f;
	}

	// Token: 0x0600077E RID: 1918 RVA: 0x0003F2AC File Offset: 0x0003D6AC
	private void Update()
	{
		float num = 999999f;
		float num2 = -999999f;
		float num3 = 0f;
		float num4 = 0f;
		int num5 = this.colorSets.Length - 1;
		int num6 = 0;
		float y = base.transform.position.y;
		for (int i = 0; i < this.colorSets.Length; i++)
		{
			float y2 = this.colorSets[i].transform.position.y;
			float num7 = y2 - y;
			if (num7 < num && num7 >= 0f)
			{
				num = num7;
				num5 = i;
				num3 = y2;
			}
			else if (num7 > num2 && num7 < 0f)
			{
				num2 = num7;
				num6 = i;
				num4 = y2;
			}
		}
		float num8 = Mathf.Clamp01((y - num4) / (num3 - num4));
		this.sun.color = this.blendColor(this.colorSets[num6].sun, this.colorSets[num5].sun, num8);
		this.topColor = this.blendColor(this.colorSets[num6].sky.Evaluate(1f), this.colorSets[num5].sky.Evaluate(1f), num8);
		this.midColor = this.blendColor(this.colorSets[num6].sky.Evaluate(0.5f), this.colorSets[num5].sky.Evaluate(0.5f), num8);
		this.bottomColor = this.blendColor(this.colorSets[num6].sky.Evaluate(0f), this.colorSets[num5].sky.Evaluate(0f), num8);
		this.sky.sharedMaterial.SetColor("_BottomColor", this.bottomColor);
		this.sky.sharedMaterial.SetColor("_MidColor", this.midColor);
		this.sky.sharedMaterial.SetColor("_TopColor", this.topColor);
		this.exposure = Mathf.Lerp(this.colorSets[num6].exposure, this.colorSets[num5].exposure, num8);
		this.contrast = Mathf.Lerp(this.colorSets[num6].contrast, this.colorSets[num5].contrast, num8);
		this.saturation = Mathf.Lerp(this.colorSets[num6].saturation, this.colorSets[num5].saturation, num8);
		RenderSettings.ambientSkyColor = this.topColor * 1.2f;
		RenderSettings.ambientEquatorColor = this.midColor;
		RenderSettings.ambientGroundColor = this.bottomColor * 0.5f;
		RenderSettings.fogColor = this.blendColor(this.colorSets[num6].fog, this.colorSets[num5].fog, num8);
		RenderSettings.fogDensity = ((y <= 82f) ? Mathf.Lerp(0.03f, 0.013f, Mathf.Abs(y) / 82f) : 0.013f);
		RenderSettings.skybox.SetColor("_BottomColor", this.bottomColor);
		RenderSettings.skybox.SetColor("_MidColor", this.midColor);
		RenderSettings.skybox.SetColor("_TopColor", this.topColor);
	}

	// Token: 0x0600077F RID: 1919 RVA: 0x0003F614 File Offset: 0x0003DA14
	private Color blendColor(Color c1, Color c2, float t)
	{
		float num = this.blendCurve.Evaluate(t);
		float num2 = c1.r * num + c2.r * (1f - num);
		float num3 = c1.g * num + c2.g * (1f - num);
		float num4 = c1.b * num + c2.b * (1f - num);
		return new Color(num2, num3, num4);
	}

	// Token: 0x040006A6 RID: 1702
	public MeshRenderer sky;

	// Token: 0x040006A7 RID: 1703
	public Light sun;

	// Token: 0x040006A8 RID: 1704
	public AnimationCurve blendCurve;

	// Token: 0x040006A9 RID: 1705
	public ColorSet[] colorSets;

	// Token: 0x040006AA RID: 1706
	private Color topColor;

	// Token: 0x040006AB RID: 1707
	private Color bottomColor;

	// Token: 0x040006AC RID: 1708
	private Color midColor;

	// Token: 0x040006AD RID: 1709
	private float contrast;

	// Token: 0x040006AE RID: 1710
	private float exposure;

	// Token: 0x040006AF RID: 1711
	private float saturation;
}
