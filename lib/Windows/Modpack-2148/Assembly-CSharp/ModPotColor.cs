using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000315 RID: 789
public class ModPotColor : Mod
{
	// Token: 0x06001AFC RID: 6908 RVA: 0x00081E58 File Offset: 0x00080058
	public bool Load()
	{
		try
		{
			if (this.option > 0)
			{
				float num = (float)Mathf.Min(PlayerPrefs.GetInt("NumWins"), 50) / 50f;
				num *= num;
				ModPotColor.SetPotColor(this.optionColors[this.option], num);
				PlayerPrefs.SetInt("ModPotColorOption", this.option);
				PlayerPrefs.Save();
			}
			else
			{
				ModPotColor.SetPotColor(0f, 0f, 0f, 1f, 0f);
			}
		}
		catch
		{
		}
		this.enabled = true;
		PlayerPrefs.SetInt("ModPotColor", 1);
		PlayerPrefs.Save();
		return true;
	}

	// Token: 0x06001AFD RID: 6909 RVA: 0x00081F04 File Offset: 0x00080104
	public ModPotColor()
	{
		this.name = "Pot Color";
		this.version = "1.0";
		this.author = "anjo2";
		this.web = "twitch.tv/anjo2";
		this.optionsTitle = "Color";
		this.options = new List<string> { "Black", "Yellow", "Red", "Green", "Blue", "Pink", "Purple" };
		this.option = 0;
		this.enabled = false;
		this.optionColors = new Color[]
		{
			Color.black,
			Color.yellow,
			Color.red,
			Color.green,
			new Color(0f, 0f, 1f, 1f),
			new Color(0.99215686f, 0.4745098f, 0.65882355f, 1f),
			new Color(0.5f, 0f, 0.5f, 1f)
		};
		this.enableSplits = false;
		if (PlayerPrefs.HasKey("ModPotColor"))
		{
			this.enabled = PlayerPrefs.GetInt("ModPotColor") == 1;
		}
		if (PlayerPrefs.HasKey("ModPotColorOption"))
		{
			this.option = PlayerPrefs.GetInt("ModPotColorOption");
		}
	}

	// Token: 0x06001AFE RID: 6910 RVA: 0x00082094 File Offset: 0x00080294
	public bool UnLoad()
	{
		try
		{
			float num = (float)Mathf.Min(PlayerPrefs.GetInt("NumWins"), 50) / 50f;
			num *= num;
			ModPotColor.SetPotColor(1f, 1f, 1f, 1f, num);
		}
		catch
		{
		}
		this.enabled = false;
		PlayerPrefs.SetInt("ModPotColor", 0);
		PlayerPrefs.Save();
		return true;
	}

	// Token: 0x17000771 RID: 1905
	// (get) Token: 0x06001AFF RID: 6911 RVA: 0x00013BEC File Offset: 0x00011DEC
	public string name { get; }

	// Token: 0x17000772 RID: 1906
	// (get) Token: 0x06001B00 RID: 6912 RVA: 0x00013BF4 File Offset: 0x00011DF4
	public string version { get; }

	// Token: 0x17000773 RID: 1907
	// (get) Token: 0x06001B01 RID: 6913 RVA: 0x00013BFC File Offset: 0x00011DFC
	public string author { get; }

	// Token: 0x17000774 RID: 1908
	// (get) Token: 0x06001B02 RID: 6914 RVA: 0x00013C04 File Offset: 0x00011E04
	public string web { get; }

	// Token: 0x06001B03 RID: 6915 RVA: 0x00082108 File Offset: 0x00080308
	public string Update(float deltaTime, GameObject player)
	{
		try
		{
			if (this.enabled && this.lastOption != this.option)
			{
				PlayerPrefs.SetInt("ModPotColorOption", this.option);
				PlayerPrefs.Save();
				this.Load();
			}
		}
		catch
		{
		}
		return "";
	}

	// Token: 0x17000775 RID: 1909
	// (get) Token: 0x06001B04 RID: 6916 RVA: 0x00013C0C File Offset: 0x00011E0C
	// (set) Token: 0x06001B05 RID: 6917 RVA: 0x00013C14 File Offset: 0x00011E14
	public int option { get; set; }

	// Token: 0x17000776 RID: 1910
	// (get) Token: 0x06001B06 RID: 6918 RVA: 0x00013C1D File Offset: 0x00011E1D
	public List<string> options { get; }

	// Token: 0x17000777 RID: 1911
	// (get) Token: 0x06001B07 RID: 6919 RVA: 0x00013C25 File Offset: 0x00011E25
	// (set) Token: 0x06001B08 RID: 6920 RVA: 0x00013C2D File Offset: 0x00011E2D
	public bool enabled { get; set; }

	// Token: 0x06001B09 RID: 6921 RVA: 0x00013C36 File Offset: 0x00011E36
	public static void SetPotColor(Color color, float gold)
	{
		GameObject.Find("Player").transform.Find("Pot/Mesh").GetComponent<MeshRenderer>().material.color = color;
	}

	// Token: 0x06001B0A RID: 6922 RVA: 0x00013C61 File Offset: 0x00011E61
	public static void SetPotColor(float R, float G, float B, float A, float gold)
	{
		ModPotColor.SetPotColor(new Color(R, G, B, A), gold);
	}

	// Token: 0x17000778 RID: 1912
	// (get) Token: 0x06001B0B RID: 6923 RVA: 0x00013C73 File Offset: 0x00011E73
	// (set) Token: 0x06001B0C RID: 6924 RVA: 0x00013C7B File Offset: 0x00011E7B
	public string optionsTitle { get; set; }

	// Token: 0x17000779 RID: 1913
	// (get) Token: 0x06001B0D RID: 6925 RVA: 0x00013C84 File Offset: 0x00011E84
	// (set) Token: 0x06001B0E RID: 6926 RVA: 0x00013C8C File Offset: 0x00011E8C
	public bool enableSplits { get; set; }

	// Token: 0x06001B0F RID: 6927 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001B10 RID: 6928 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x04001245 RID: 4677
	private GameObject player;

	// Token: 0x04001246 RID: 4678
	private Color[] optionColors;

	// Token: 0x04001247 RID: 4679
	private int lastOption;
}
