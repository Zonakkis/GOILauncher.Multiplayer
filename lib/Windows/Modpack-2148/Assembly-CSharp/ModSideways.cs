using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000306 RID: 774
public class ModSideways : Mod
{
	// Token: 0x06001A6E RID: 6766 RVA: 0x000806F0 File Offset: 0x0007E8F0
	public bool Load()
	{
		SettingsManager.sideWayDegrees = this.optionsValues[this.option];
		try
		{
			GameObject.Find("Player").transform.Rotate(new Vector3(0f, 0f, SettingsManager.sideWayDegrees));
			Camera.main.transform.Rotate(new Vector3(0f, 0f, SettingsManager.sideWayDegrees));
			if (SettingsManager.category != 0)
			{
				global::UnityEngine.Object.FindObjectOfType<SettingsManager>().revertGravity(true, true, true, true);
			}
		}
		catch
		{
		}
		SettingsManager.sideWay = true;
		this.lastOption = this.option;
		this.enabled = true;
		return true;
	}

	// Token: 0x06001A6F RID: 6767 RVA: 0x000807A0 File Offset: 0x0007E9A0
	public ModSideways()
	{
		this.name = "Sideways";
		this.version = "1.0";
		this.author = "Codyumm";
		this.web = "";
		this.optionsTitle = "Angle";
		this.options = new List<string>
		{
			"10º", "15º", "20º", "25º", "30º", "45º", "60º", "75º", "90º", "105º",
			"120º", "135º", "150º", "165º", "180º", "-165º", "-150º", "-135º", "-120º", "-115º",
			"-90º", "-75º", "-60º", "-45º", "-30º", "-25º", "-20º", "-15º", "-10º"
		};
		this.optionsValues = new float[]
		{
			10f, 15f, 20f, 25f, 30f, 45f, 60f, 75f, 90f, 105f,
			120f, 135f, 150f, 165f, 180f, 195f, 210f, 225f, 240f, 255f,
			270f, 285f, 300f, 315f, 330f, 335f, 340f, 345f, 350f
		};
		this.option = 8;
		this.lastOption = this.option;
		this.enableSplits = true;
		this.enabled = false;
	}

	// Token: 0x06001A70 RID: 6768 RVA: 0x00080970 File Offset: 0x0007EB70
	public bool UnLoad()
	{
		try
		{
			GameObject.Find("Player").transform.Rotate(new Vector3(0f, 0f, -SettingsManager.sideWayDegrees));
			Camera.main.transform.Rotate(new Vector3(0f, 0f, -SettingsManager.sideWayDegrees));
			if (SettingsManager.category != 0)
			{
				global::UnityEngine.Object.FindObjectOfType<SettingsManager>().revertGravity(false, true, true, true);
			}
		}
		catch
		{
		}
		SettingsManager.sideWay = false;
		SettingsManager.sideWayDegrees = 0f;
		this.lastOption = this.option;
		this.enabled = false;
		return true;
	}

	// Token: 0x17000730 RID: 1840
	// (get) Token: 0x06001A71 RID: 6769 RVA: 0x00013860 File Offset: 0x00011A60
	public string name { get; }

	// Token: 0x17000731 RID: 1841
	// (get) Token: 0x06001A72 RID: 6770 RVA: 0x00013868 File Offset: 0x00011A68
	public string version { get; }

	// Token: 0x17000732 RID: 1842
	// (get) Token: 0x06001A73 RID: 6771 RVA: 0x00013870 File Offset: 0x00011A70
	public string author { get; }

	// Token: 0x17000733 RID: 1843
	// (get) Token: 0x06001A74 RID: 6772 RVA: 0x00013878 File Offset: 0x00011A78
	public string web { get; }

	// Token: 0x06001A75 RID: 6773 RVA: 0x00080A18 File Offset: 0x0007EC18
	public string Update(float deltaTime, GameObject player)
	{
		if (SettingsManager.sideWay != this.enabled)
		{
			SettingsManager.sideWay = this.enabled;
		}
		if (this.enabled && this.lastOption != this.option)
		{
			this.UnLoad();
			this.Load();
		}
		return "";
	}

	// Token: 0x17000734 RID: 1844
	// (get) Token: 0x06001A76 RID: 6774 RVA: 0x00013880 File Offset: 0x00011A80
	// (set) Token: 0x06001A77 RID: 6775 RVA: 0x00013888 File Offset: 0x00011A88
	public int option { get; set; }

	// Token: 0x17000735 RID: 1845
	// (get) Token: 0x06001A78 RID: 6776 RVA: 0x00013891 File Offset: 0x00011A91
	public List<string> options { get; }

	// Token: 0x17000736 RID: 1846
	// (get) Token: 0x06001A79 RID: 6777 RVA: 0x00013899 File Offset: 0x00011A99
	// (set) Token: 0x06001A7A RID: 6778 RVA: 0x000138A1 File Offset: 0x00011AA1
	public bool enabled { get; set; }

	// Token: 0x17000737 RID: 1847
	// (get) Token: 0x06001A7B RID: 6779 RVA: 0x000138AA File Offset: 0x00011AAA
	public float[] optionsValues { get; }

	// Token: 0x17000738 RID: 1848
	// (get) Token: 0x06001A7C RID: 6780 RVA: 0x000138B2 File Offset: 0x00011AB2
	// (set) Token: 0x06001A7D RID: 6781 RVA: 0x000138BA File Offset: 0x00011ABA
	public string optionsTitle { get; set; }

	// Token: 0x17000739 RID: 1849
	// (get) Token: 0x06001A7E RID: 6782 RVA: 0x000138C3 File Offset: 0x00011AC3
	// (set) Token: 0x06001A7F RID: 6783 RVA: 0x000138CB File Offset: 0x00011ACB
	public bool enableSplits { get; set; }

	// Token: 0x06001A80 RID: 6784 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001A81 RID: 6785 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x040011F5 RID: 4597
	private int lastOption;
}
