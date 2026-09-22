using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200030C RID: 780
public class ModInvisible : Mod
{
	// Token: 0x06001A96 RID: 6806 RVA: 0x00080C30 File Offset: 0x0007EE30
	public bool Load()
	{
		try
		{
			MeshRenderer[] componentsInChildren = GameObject.Find("Player").gameObject.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			SkinnedMeshRenderer[] componentsInChildren2 = GameObject.Find("Player").gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
			SkinnedMeshRenderer[] componentsInChildren3 = GameObject.Find("dude").gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int k = 0; k < componentsInChildren3.Length; k++)
			{
				componentsInChildren3[k].enabled = false;
			}
		}
		catch
		{
		}
		this.enabled = true;
		return true;
	}

	// Token: 0x06001A97 RID: 6807 RVA: 0x00080CE8 File Offset: 0x0007EEE8
	public ModInvisible()
	{
		this.name = "Invisible Player";
		this.version = "1.0";
		this.author = "anjo2";
		this.web = "twitch.tv/anjo2";
		this.options = new List<string>();
		this.optionsTitle = "";
		this.option = 0;
		this.enabled = false;
		this.enableSplits = true;
	}

	// Token: 0x06001A98 RID: 6808 RVA: 0x00080D54 File Offset: 0x0007EF54
	public bool UnLoad()
	{
		try
		{
			MeshRenderer[] componentsInChildren = GameObject.Find("Player").gameObject.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
			SkinnedMeshRenderer[] componentsInChildren2 = GameObject.Find("Player").gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].enabled = true;
			}
			SkinnedMeshRenderer[] componentsInChildren3 = GameObject.Find("dude").gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int k = 0; k < componentsInChildren3.Length; k++)
			{
				componentsInChildren3[k].enabled = true;
			}
		}
		catch
		{
		}
		this.enabled = false;
		return true;
	}

	// Token: 0x17000744 RID: 1860
	// (get) Token: 0x06001A99 RID: 6809 RVA: 0x000139BC File Offset: 0x00011BBC
	public string name { get; }

	// Token: 0x17000745 RID: 1861
	// (get) Token: 0x06001A9A RID: 6810 RVA: 0x000139C4 File Offset: 0x00011BC4
	public string version { get; }

	// Token: 0x17000746 RID: 1862
	// (get) Token: 0x06001A9B RID: 6811 RVA: 0x000139CC File Offset: 0x00011BCC
	public string author { get; }

	// Token: 0x17000747 RID: 1863
	// (get) Token: 0x06001A9C RID: 6812 RVA: 0x000139D4 File Offset: 0x00011BD4
	public string web { get; }

	// Token: 0x06001A9D RID: 6813 RVA: 0x000126FA File Offset: 0x000108FA
	public string Update(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x17000748 RID: 1864
	// (get) Token: 0x06001A9E RID: 6814 RVA: 0x000139DC File Offset: 0x00011BDC
	// (set) Token: 0x06001A9F RID: 6815 RVA: 0x000139E4 File Offset: 0x00011BE4
	public int option { get; set; }

	// Token: 0x17000749 RID: 1865
	// (get) Token: 0x06001AA0 RID: 6816 RVA: 0x000139ED File Offset: 0x00011BED
	public List<string> options { get; }

	// Token: 0x1700074A RID: 1866
	// (get) Token: 0x06001AA1 RID: 6817 RVA: 0x000139F5 File Offset: 0x00011BF5
	// (set) Token: 0x06001AA2 RID: 6818 RVA: 0x000139FD File Offset: 0x00011BFD
	public bool enabled { get; set; }

	// Token: 0x1700074B RID: 1867
	// (get) Token: 0x06001AA3 RID: 6819 RVA: 0x00013A06 File Offset: 0x00011C06
	// (set) Token: 0x06001AA4 RID: 6820 RVA: 0x00013A0E File Offset: 0x00011C0E
	public string optionsTitle { get; set; }

	// Token: 0x1700074C RID: 1868
	// (get) Token: 0x06001AA5 RID: 6821 RVA: 0x00013A17 File Offset: 0x00011C17
	// (set) Token: 0x06001AA6 RID: 6822 RVA: 0x00013A1F File Offset: 0x00011C1F
	public bool enableSplits { get; set; }

	// Token: 0x06001AA7 RID: 6823 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001AA8 RID: 6824 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}
}
