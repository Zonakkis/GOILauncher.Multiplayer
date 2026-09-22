using System;
using System.Collections.Generic;
using Oxide.Core;
using UnityEngine;

// Token: 0x0200030F RID: 783
public class ModMultiplayer : Mod
{
	// Token: 0x06001AD6 RID: 6870 RVA: 0x000819E4 File Offset: 0x0007FBE4
	public bool Load()
	{
		try
		{
			if (!this.enabled)
			{
				Interface.Initialize();
				Interface.CallHook("IInit", null);
				Debug.Log("Multiplayer Enabled");
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		this.enabled = true;
		return true;
	}

	// Token: 0x06001AD7 RID: 6871 RVA: 0x00081A3C File Offset: 0x0007FC3C
	public ModMultiplayer()
	{
		this.name = "Multiplayer";
		this.version = "0.6.3";
		this.author = "Skippy";
		this.web = "https://github.com/Skippeh/";
		this.optionsTitle = "";
		this.options = new List<string>();
		this.option = 0;
		this.enabled = false;
		this.enableSplits = false;
	}

	// Token: 0x06001AD8 RID: 6872 RVA: 0x00013B00 File Offset: 0x00011D00
	public bool UnLoad()
	{
		this.enabled = true;
		return true;
	}

	// Token: 0x1700075F RID: 1887
	// (get) Token: 0x06001AD9 RID: 6873 RVA: 0x00013B0A File Offset: 0x00011D0A
	public string name { get; }

	// Token: 0x17000760 RID: 1888
	// (get) Token: 0x06001ADA RID: 6874 RVA: 0x00013B12 File Offset: 0x00011D12
	public string version { get; }

	// Token: 0x17000761 RID: 1889
	// (get) Token: 0x06001ADB RID: 6875 RVA: 0x00013B1A File Offset: 0x00011D1A
	public string author { get; }

	// Token: 0x17000762 RID: 1890
	// (get) Token: 0x06001ADC RID: 6876 RVA: 0x00013B22 File Offset: 0x00011D22
	public string web { get; }

	// Token: 0x06001ADD RID: 6877 RVA: 0x000126FA File Offset: 0x000108FA
	public string Update(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x17000763 RID: 1891
	// (get) Token: 0x06001ADE RID: 6878 RVA: 0x00013B2A File Offset: 0x00011D2A
	// (set) Token: 0x06001ADF RID: 6879 RVA: 0x00013B32 File Offset: 0x00011D32
	public int option { get; set; }

	// Token: 0x17000764 RID: 1892
	// (get) Token: 0x06001AE0 RID: 6880 RVA: 0x00013B3B File Offset: 0x00011D3B
	public List<string> options { get; }

	// Token: 0x17000765 RID: 1893
	// (get) Token: 0x06001AE1 RID: 6881 RVA: 0x00013B43 File Offset: 0x00011D43
	// (set) Token: 0x06001AE2 RID: 6882 RVA: 0x00013B4B File Offset: 0x00011D4B
	public bool enabled { get; set; }

	// Token: 0x17000766 RID: 1894
	// (get) Token: 0x06001AE3 RID: 6883 RVA: 0x00013B54 File Offset: 0x00011D54
	// (set) Token: 0x06001AE4 RID: 6884 RVA: 0x00013B5C File Offset: 0x00011D5C
	public string optionsTitle { get; set; }

	// Token: 0x17000767 RID: 1895
	// (get) Token: 0x06001AE5 RID: 6885 RVA: 0x00013B65 File Offset: 0x00011D65
	// (set) Token: 0x06001AE6 RID: 6886 RVA: 0x00013B6D File Offset: 0x00011D6D
	public bool enableSplits { get; set; }

	// Token: 0x06001AE7 RID: 6887 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001AE8 RID: 6888 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}
}
