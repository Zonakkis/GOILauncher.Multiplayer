using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// Token: 0x02000046 RID: 70
public class FogControl : MonoBehaviour
{
	// Token: 0x0600019C RID: 412 RVA: 0x0001F0F4 File Offset: 0x0001D2F4
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

	// Token: 0x0600019D RID: 413 RVA: 0x0001F168 File Offset: 0x0001D368
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

	// Token: 0x0600019E RID: 414 RVA: 0x0001F4E4 File Offset: 0x0001D6E4
	private Color blendColor(Color c1, Color c2, float t)
	{
		float num = this.blendCurve.Evaluate(t);
		float num2 = c1.r * num + c2.r * (1f - num);
		float num3 = c1.g * num + c2.g * (1f - num);
		float num4 = c1.b * num + c2.b * (1f - num);
		return new Color(num2, num3, num4);
	}

	// Token: 0x0400028D RID: 653
	public MeshRenderer sky;

	// Token: 0x0400028E RID: 654
	public Light sun;

	// Token: 0x0400028F RID: 655
	public AnimationCurve blendCurve;

	// Token: 0x04000290 RID: 656
	public ColorSet[] colorSets;

	// Token: 0x04000291 RID: 657
	private Color topColor;

	// Token: 0x04000292 RID: 658
	private Color bottomColor;

	// Token: 0x04000293 RID: 659
	private Color midColor;

	// Token: 0x04000294 RID: 660
	private float contrast;

	// Token: 0x04000295 RID: 661
	private float exposure;

	// Token: 0x04000296 RID: 662
	private float saturation;

	// Token: 0x04000297 RID: 663
	private PostProcessVolume post;

	// Token: 0x04000298 RID: 664
	private PostProcessProfile p;

	// Token: 0x04000299 RID: 665
	private ColorGrading settings;
}
