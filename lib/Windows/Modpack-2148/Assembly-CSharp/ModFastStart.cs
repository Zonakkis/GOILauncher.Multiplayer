using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000309 RID: 777
public class ModFastStart : Mod
{
	// Token: 0x06001A82 RID: 6786 RVA: 0x000138D4 File Offset: 0x00011AD4
	public bool Load()
	{
		SettingsManager.fastStartTime = this.optionsValues[this.option];
		SettingsManager.fastStart = true;
		this.lastOption = this.option;
		this.enabled = true;
		PlayerPrefs.SetInt("ModFastStart", 1);
		PlayerPrefs.Save();
		return true;
	}

	// Token: 0x06001A83 RID: 6787 RVA: 0x00080A68 File Offset: 0x0007EC68
	public ModFastStart()
	{
		this.name = "FastStart";
		this.version = "1.0";
		this.author = "anjo2";
		this.web = "twitch.tv/anjo2";
		this.optionsTitle = "Time";
		this.options = new List<string>
		{
			"0.5s", "0.75s", "1.0s", "1.25s", "1.5s", "1.75s", "2.0s", "2.5s", "3.0s", "3.5s",
			"4.0s", "4.5s", "5.0s", "5.5s"
		};
		this.optionsValues = new float[]
		{
			0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 2.5f, 3f, 3.5f,
			4f, 4.5f, 5f, 5.5f
		};
		this.option = 6;
		this.enabled = false;
		this.lastOption = this.option;
		this.enableSplits = false;
		if (PlayerPrefs.HasKey("ModFastStart"))
		{
			this.enabled = PlayerPrefs.GetInt("ModFastStart") == 1;
		}
		if (PlayerPrefs.HasKey("ModFastStartOption"))
		{
			this.option = PlayerPrefs.GetInt("ModFastStartOption");
		}
	}

	// Token: 0x06001A84 RID: 6788 RVA: 0x00013912 File Offset: 0x00011B12
	public bool UnLoad()
	{
		SettingsManager.fastStart = false;
		SettingsManager.fastStartTime = 6f;
		this.lastOption = this.option;
		this.enabled = false;
		PlayerPrefs.SetInt("ModFastStart", 0);
		PlayerPrefs.Save();
		return true;
	}

	// Token: 0x1700073A RID: 1850
	// (get) Token: 0x06001A85 RID: 6789 RVA: 0x00013948 File Offset: 0x00011B48
	public string name { get; }

	// Token: 0x1700073B RID: 1851
	// (get) Token: 0x06001A86 RID: 6790 RVA: 0x00013950 File Offset: 0x00011B50
	public string version { get; }

	// Token: 0x1700073C RID: 1852
	// (get) Token: 0x06001A87 RID: 6791 RVA: 0x00013958 File Offset: 0x00011B58
	public string author { get; }

	// Token: 0x1700073D RID: 1853
	// (get) Token: 0x06001A88 RID: 6792 RVA: 0x00013960 File Offset: 0x00011B60
	public string web { get; }

	// Token: 0x06001A89 RID: 6793 RVA: 0x00080BCC File Offset: 0x0007EDCC
	public string Update(float deltaTime, GameObject player)
	{
		if (SettingsManager.fastStart != this.enabled)
		{
			SettingsManager.fastStart = this.enabled;
		}
		if (this.enabled && this.lastOption != this.option)
		{
			this.lastOption = this.option;
			PlayerPrefs.SetInt("ModFastStartOption", this.option);
			PlayerPrefs.Save();
		}
		return "";
	}

	// Token: 0x1700073E RID: 1854
	// (get) Token: 0x06001A8A RID: 6794 RVA: 0x00013968 File Offset: 0x00011B68
	// (set) Token: 0x06001A8B RID: 6795 RVA: 0x00013970 File Offset: 0x00011B70
	public int option { get; set; }

	// Token: 0x1700073F RID: 1855
	// (get) Token: 0x06001A8C RID: 6796 RVA: 0x00013979 File Offset: 0x00011B79
	public List<string> options { get; }

	// Token: 0x17000740 RID: 1856
	// (get) Token: 0x06001A8D RID: 6797 RVA: 0x00013981 File Offset: 0x00011B81
	// (set) Token: 0x06001A8E RID: 6798 RVA: 0x00013989 File Offset: 0x00011B89
	public bool enabled { get; set; }

	// Token: 0x17000741 RID: 1857
	// (get) Token: 0x06001A8F RID: 6799 RVA: 0x00013992 File Offset: 0x00011B92
	public float[] optionsValues { get; }

	// Token: 0x17000742 RID: 1858
	// (get) Token: 0x06001A90 RID: 6800 RVA: 0x0001399A File Offset: 0x00011B9A
	// (set) Token: 0x06001A91 RID: 6801 RVA: 0x000139A2 File Offset: 0x00011BA2
	public string optionsTitle { get; set; }

	// Token: 0x17000743 RID: 1859
	// (get) Token: 0x06001A92 RID: 6802 RVA: 0x000139AB File Offset: 0x00011BAB
	// (set) Token: 0x06001A93 RID: 6803 RVA: 0x000139B3 File Offset: 0x00011BB3
	public bool enableSplits { get; set; }

	// Token: 0x06001A94 RID: 6804 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001A95 RID: 6805 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x04001201 RID: 4609
	private int lastOption;
}
