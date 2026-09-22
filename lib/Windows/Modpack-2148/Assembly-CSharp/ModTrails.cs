using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000323 RID: 803
public class ModTrails : Mod
{
	// Token: 0x06001BB2 RID: 7090 RVA: 0x0008845C File Offset: 0x0008665C
	public bool Load()
	{
		try
		{
			if (GameObject.Find("Player") && this.options.Count > 0)
			{
				if (this.options.Count <= this.option)
				{
					this.option = 0;
					this.lastOption = this.option;
				}
				this.Color1 = this.optionColors[this.option];
				this.trailRenderer = GameObject.Find("Tip").AddComponent<TrailRenderer>();
				this.trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
				this.trailRenderer.startWidth = 0.12f;
				this.trailRenderer.endWidth = 0.01f;
				this.trailRenderer.minVertexDistance = 0.05f;
				Gradient gradient = new Gradient();
				gradient.SetKeys(new GradientColorKey[]
				{
					new GradientColorKey(this.Color1, 0f),
					new GradientColorKey(this.Color2, 1f)
				}, new GradientAlphaKey[]
				{
					new GradientAlphaKey(this.alpha, 0f),
					new GradientAlphaKey(this.alpha, 0.7f)
				});
				this.trailRenderer.colorGradient = gradient;
				this.trailRenderer.time = this.traillen;
				PlayerPrefs.SetInt("ModTrailsOption", this.option);
				PlayerPrefs.Save();
			}
		}
		catch
		{
		}
		this.enabled = true;
		PlayerPrefs.SetInt("ModTrails", 1);
		PlayerPrefs.Save();
		return true;
	}

	// Token: 0x06001BB3 RID: 7091 RVA: 0x0008860C File Offset: 0x0008680C
	public ModTrails()
	{
		this.name = "Hammer Trail";
		this.version = "1.0";
		this.author = "Codyumm";
		this.web = "";
		this.options = new List<string>();
		this.optionsTitle = "";
		this.option = 0;
		this.enabled = false;
		this.timeResetColor = 0f;
		this.targetColor = global::UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.6f, 1f);
		this.enableSplits = false;
		this.options = new List<string> { "RGB", "Red", "Black", "White", "Green", "Blue", "Cyan", "Grey", "Yellow", "Magenta" };
		this.optionColors = new Color[]
		{
			Color.clear,
			Color.red,
			Color.black,
			Color.white,
			Color.green,
			Color.blue,
			Color.cyan,
			Color.grey,
			Color.yellow,
			Color.magenta
		};
		if (PlayerPrefs.HasKey("ModTrailsOption"))
		{
			this.option = PlayerPrefs.GetInt("ModTrailsOption");
		}
		if (this.options.Count <= this.option)
		{
			this.option = 0;
			this.lastOption = this.option;
		}
		this.traillen = 0.4f;
		this.Color1 = this.optionColors[0];
		this.Color2 = Color.black;
		if (PlayerPrefs.HasKey("ModTrails"))
		{
			this.enabled = PlayerPrefs.GetInt("ModTrails") == 1;
		}
	}

	// Token: 0x06001BB4 RID: 7092 RVA: 0x00088844 File Offset: 0x00086A44
	public bool UnLoad()
	{
		this.enabled = false;
		try
		{
			global::UnityEngine.Object.Destroy(GameObject.Find("Tip").GetComponent<TrailRenderer>());
		}
		catch
		{
		}
		PlayerPrefs.SetInt("ModTrails", 0);
		PlayerPrefs.Save();
		return true;
	}

	// Token: 0x170007C6 RID: 1990
	// (get) Token: 0x06001BB5 RID: 7093 RVA: 0x00014042 File Offset: 0x00012242
	public string name { get; }

	// Token: 0x170007C7 RID: 1991
	// (get) Token: 0x06001BB6 RID: 7094 RVA: 0x0001404A File Offset: 0x0001224A
	public string version { get; }

	// Token: 0x170007C8 RID: 1992
	// (get) Token: 0x06001BB7 RID: 7095 RVA: 0x00014052 File Offset: 0x00012252
	public string author { get; }

	// Token: 0x170007C9 RID: 1993
	// (get) Token: 0x06001BB8 RID: 7096 RVA: 0x0001405A File Offset: 0x0001225A
	public string web { get; }

	// Token: 0x06001BB9 RID: 7097 RVA: 0x00088894 File Offset: 0x00086A94
	public string Update(float deltaTime, GameObject player)
	{
		if (this.lastOption != this.option && player && this.enabled)
		{
			this.Color1 = this.optionColors[this.option];
			this.Apply();
			this.lastOption = this.option;
			PlayerPrefs.SetInt("ModTrailsOption", this.option);
			PlayerPrefs.Save();
		}
		if (this.option == 0 && this.enabled)
		{
			if (deltaTime > this.timeResetColor)
			{
				this.targetColor = global::UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.6f, 1f);
				this.timeResetColor = 1f;
			}
			else
			{
				this.timeResetColor -= deltaTime;
			}
			this.Color1 = Color.Lerp(this.Color1, this.targetColor, deltaTime / this.timeResetColor);
			this.Apply();
		}
		return "";
	}

	// Token: 0x170007CA RID: 1994
	// (get) Token: 0x06001BBA RID: 7098 RVA: 0x00014062 File Offset: 0x00012262
	// (set) Token: 0x06001BBB RID: 7099 RVA: 0x0001406A File Offset: 0x0001226A
	public int option { get; set; }

	// Token: 0x170007CB RID: 1995
	// (get) Token: 0x06001BBC RID: 7100 RVA: 0x00014073 File Offset: 0x00012273
	public List<string> options { get; }

	// Token: 0x170007CC RID: 1996
	// (get) Token: 0x06001BBD RID: 7101 RVA: 0x0001407B File Offset: 0x0001227B
	// (set) Token: 0x06001BBE RID: 7102 RVA: 0x00014083 File Offset: 0x00012283
	public bool enabled { get; set; }

	// Token: 0x170007CD RID: 1997
	// (get) Token: 0x06001BBF RID: 7103 RVA: 0x0001408C File Offset: 0x0001228C
	// (set) Token: 0x06001BC0 RID: 7104 RVA: 0x00014094 File Offset: 0x00012294
	public string optionsTitle { get; set; }

	// Token: 0x170007CE RID: 1998
	// (get) Token: 0x06001BC1 RID: 7105 RVA: 0x0001409D File Offset: 0x0001229D
	// (set) Token: 0x06001BC2 RID: 7106 RVA: 0x000140A5 File Offset: 0x000122A5
	public bool enableSplits { get; set; }

	// Token: 0x170007CF RID: 1999
	// (get) Token: 0x06001BC3 RID: 7107 RVA: 0x000140AE File Offset: 0x000122AE
	// (set) Token: 0x06001BC4 RID: 7108 RVA: 0x000140B5 File Offset: 0x000122B5
	public static bool isEnabled { get; set; }

	// Token: 0x06001BC5 RID: 7109 RVA: 0x00088994 File Offset: 0x00086B94
	private void Apply()
	{
		Gradient gradient = new Gradient();
		gradient.SetKeys(new GradientColorKey[]
		{
			new GradientColorKey(this.Color1, 0f),
			new GradientColorKey(this.Color2, 1f)
		}, new GradientAlphaKey[]
		{
			new GradientAlphaKey(this.alpha, 0f),
			new GradientAlphaKey(this.alpha, 0.7f)
		});
		this.trailRenderer.colorGradient = gradient;
		this.trailRenderer.time = this.traillen;
	}

	// Token: 0x06001BC6 RID: 7110 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001BC7 RID: 7111 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x170007D0 RID: 2000
	// (get) Token: 0x06001BC8 RID: 7112 RVA: 0x000140BD File Offset: 0x000122BD
	// (set) Token: 0x06001BC9 RID: 7113 RVA: 0x000140C5 File Offset: 0x000122C5
	public int lastOption { get; set; }

	// Token: 0x170007D1 RID: 2001
	// (get) Token: 0x06001BCA RID: 7114 RVA: 0x000140CE File Offset: 0x000122CE
	// (set) Token: 0x06001BCB RID: 7115 RVA: 0x000140D6 File Offset: 0x000122D6
	public Color32 targetColor { get; set; }

	// Token: 0x040012B3 RID: 4787
	public TrailRenderer trail;

	// Token: 0x040012B4 RID: 4788
	public float alpha = 0.5f;

	// Token: 0x040012B5 RID: 4789
	public TrailRenderer trail1;

	// Token: 0x040012B6 RID: 4790
	public TrailRenderer trail2;

	// Token: 0x040012B7 RID: 4791
	public TrailRenderer trail3;

	// Token: 0x040012B8 RID: 4792
	public TrailRenderer trail4;

	// Token: 0x040012B9 RID: 4793
	public Color Color1;

	// Token: 0x040012BA RID: 4794
	public Color Color2;

	// Token: 0x040012BB RID: 4795
	public float C1RF;

	// Token: 0x040012BC RID: 4796
	public Color C1R;

	// Token: 0x040012BD RID: 4797
	public float C1GF;

	// Token: 0x040012BE RID: 4798
	public Color C1G;

	// Token: 0x040012BF RID: 4799
	public float C1BF;

	// Token: 0x040012C0 RID: 4800
	public Color C1B;

	// Token: 0x040012C1 RID: 4801
	public Color C2R;

	// Token: 0x040012C2 RID: 4802
	public Color C2G;

	// Token: 0x040012C3 RID: 4803
	public Color C2B;

	// Token: 0x040012C4 RID: 4804
	public float C2RF;

	// Token: 0x040012C5 RID: 4805
	public float C2GF;

	// Token: 0x040012C6 RID: 4806
	public float C2BF;

	// Token: 0x040012C7 RID: 4807
	public float traillen;

	// Token: 0x040012C8 RID: 4808
	private Color[] optionColors;

	// Token: 0x040012CA RID: 4810
	public TrailRenderer trailRenderer;

	// Token: 0x040012CB RID: 4811
	public float timeResetColor;
}
