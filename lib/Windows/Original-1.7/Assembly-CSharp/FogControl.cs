using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// Token: 0x02000031 RID: 49
public class FogControl : MonoBehaviour
{
	// Token: 0x06000172 RID: 370 RVA: 0x0000DF0C File Offset: 0x0000C10C
	private void Start()
	{
		this.topColor = Color.white;
		this.bottomColor = Color.white;
		this.midColor = Color.white;
		this.contrast = 1f;
		this.exposure = 0f;
		this.saturation = 1f;
		this.post = base.GetComponent<PostProcessVolume>();
		this.settings = this.post.profile.GetSetting<ColorGrading>();
	}

	// Token: 0x06000173 RID: 371 RVA: 0x0000DF80 File Offset: 0x0000C180
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
		RenderSettings.skybox.SetColor("_BottomColor", this.bottomColor);
		RenderSettings.skybox.SetColor("_MidColor", this.midColor);
		RenderSettings.skybox.SetColor("_TopColor", this.topColor);
		this.settings.contrast.value = this.contrast;
		this.settings.postExposure.value = this.exposure;
		this.settings.saturation.value = this.saturation;
		this.post.profile.RemoveSettings<ColorGrading>();
		this.post.profile.AddSettings(this.settings);
	}

	// Token: 0x06000174 RID: 372 RVA: 0x0000E2FC File Offset: 0x0000C4FC
	private Color blendColor(Color c1, Color c2, float t)
	{
		float num = this.blendCurve.Evaluate(t);
		float num2 = c1.r * num + c2.r * (1f - num);
		float num3 = c1.g * num + c2.g * (1f - num);
		float num4 = c1.b * num + c2.b * (1f - num);
		return new Color(num2, num3, num4);
	}

	// Token: 0x04000237 RID: 567
	public MeshRenderer sky;

	// Token: 0x04000238 RID: 568
	public Light sun;

	// Token: 0x04000239 RID: 569
	public AnimationCurve blendCurve;

	// Token: 0x0400023A RID: 570
	public ColorSet[] colorSets;

	// Token: 0x0400023B RID: 571
	private Color topColor;

	// Token: 0x0400023C RID: 572
	private Color bottomColor;

	// Token: 0x0400023D RID: 573
	private Color midColor;

	// Token: 0x0400023E RID: 574
	private float contrast;

	// Token: 0x0400023F RID: 575
	private float exposure;

	// Token: 0x04000240 RID: 576
	private float saturation;

	// Token: 0x04000241 RID: 577
	private PostProcessVolume post;

	// Token: 0x04000242 RID: 578
	private PostProcessProfile p;

	// Token: 0x04000243 RID: 579
	private ColorGrading settings;
}
