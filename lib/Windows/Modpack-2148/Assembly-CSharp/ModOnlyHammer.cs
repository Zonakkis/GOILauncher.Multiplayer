using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002EF RID: 751
public class ModOnlyHammer : Mod
{
	// Token: 0x0600199A RID: 6554 RVA: 0x0007ADA0 File Offset: 0x00078FA0
	public bool Load()
	{
		try
		{
			GameObject.Find("Player").GetComponent<PolygonCollider2D>().isTrigger = true;
			GameObject.Find("Player").transform.Find("PotCollider").GetComponent<PolygonCollider2D>().isTrigger = true;
			PolygonCollider2D[] components = GameObject.Find("Player").transform.Find("PotCollider").transform.Find("Sides").GetComponents<PolygonCollider2D>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].isTrigger = true;
			}
			this.dude = GameObject.Find("Player").transform.Find("dude").gameObject;
			this.dude.SetActive(false);
			GameObject.Find("Player").transform.Find("Pot").transform.Find("Mesh").gameObject.SetActive(false);
		}
		catch
		{
		}
		this.enabled = true;
		return true;
	}

	// Token: 0x0600199B RID: 6555 RVA: 0x0007AEAC File Offset: 0x000790AC
	public ModOnlyHammer()
	{
		this.name = "Only Hammer";
		this.version = "1.0";
		this.author = "pfg";
		this.web = "";
		this.optionsTitle = "";
		this.options = new List<string>();
		this.option = 0;
		this.enabled = false;
		this.enableSplits = true;
	}

	// Token: 0x0600199C RID: 6556 RVA: 0x0007AF18 File Offset: 0x00079118
	public bool UnLoad()
	{
		this.enabled = false;
		try
		{
			GameObject.Find("Player").GetComponent<PolygonCollider2D>().isTrigger = false;
			GameObject.Find("Player").transform.Find("PotCollider").GetComponent<PolygonCollider2D>().isTrigger = false;
			PolygonCollider2D[] components = GameObject.Find("Player").transform.Find("PotCollider").transform.Find("Sides").GetComponents<PolygonCollider2D>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].isTrigger = false;
			}
			GameObject.Find("Player").transform.Find("dude").gameObject.SetActive(true);
			GameObject.Find("Player").transform.Find("Pot").transform.Find("Mesh").gameObject.SetActive(true);
		}
		catch
		{
		}
		return true;
	}

	// Token: 0x170006EA RID: 1770
	// (get) Token: 0x0600199D RID: 6557 RVA: 0x0001310B File Offset: 0x0001130B
	public string name { get; }

	// Token: 0x170006EB RID: 1771
	// (get) Token: 0x0600199E RID: 6558 RVA: 0x00013113 File Offset: 0x00011313
	public string version { get; }

	// Token: 0x170006EC RID: 1772
	// (get) Token: 0x0600199F RID: 6559 RVA: 0x0001311B File Offset: 0x0001131B
	public string author { get; }

	// Token: 0x170006ED RID: 1773
	// (get) Token: 0x060019A0 RID: 6560 RVA: 0x00013123 File Offset: 0x00011323
	public string web { get; }

	// Token: 0x060019A1 RID: 6561 RVA: 0x0001312B File Offset: 0x0001132B
	public string Update(float deltaTime, GameObject player)
	{
		if (this.enabled && this.dude != null && this.dude.activeSelf)
		{
			this.dude.SetActive(false);
		}
		return "";
	}

	// Token: 0x170006EE RID: 1774
	// (get) Token: 0x060019A2 RID: 6562 RVA: 0x00013161 File Offset: 0x00011361
	// (set) Token: 0x060019A3 RID: 6563 RVA: 0x00013169 File Offset: 0x00011369
	public int option { get; set; }

	// Token: 0x170006EF RID: 1775
	// (get) Token: 0x060019A4 RID: 6564 RVA: 0x00013172 File Offset: 0x00011372
	public List<string> options { get; }

	// Token: 0x170006F0 RID: 1776
	// (get) Token: 0x060019A5 RID: 6565 RVA: 0x0001317A File Offset: 0x0001137A
	// (set) Token: 0x060019A6 RID: 6566 RVA: 0x00013182 File Offset: 0x00011382
	public bool enabled { get; set; }

	// Token: 0x170006F1 RID: 1777
	// (get) Token: 0x060019A7 RID: 6567 RVA: 0x0001318B File Offset: 0x0001138B
	// (set) Token: 0x060019A8 RID: 6568 RVA: 0x00013193 File Offset: 0x00011393
	public string optionsTitle { get; set; }

	// Token: 0x170006F2 RID: 1778
	// (get) Token: 0x060019A9 RID: 6569 RVA: 0x0001319C File Offset: 0x0001139C
	// (set) Token: 0x060019AA RID: 6570 RVA: 0x000131A4 File Offset: 0x000113A4
	public bool enableSplits { get; set; }

	// Token: 0x060019AB RID: 6571 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x060019AC RID: 6572 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x04001151 RID: 4433
	private GameObject dude;
}
