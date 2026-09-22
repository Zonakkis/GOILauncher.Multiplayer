using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200030D RID: 781
public class ModInvisibleMountain : Mod
{
	// Token: 0x06001AA9 RID: 6825 RVA: 0x00080E0C File Offset: 0x0007F00C
	public bool Load()
	{
		try
		{
			this.lastOption = this.option;
			switch (this.option)
			{
			case 0:
				this.renderMountain(true);
				this.renderMountainCollider(true);
				this.renderBackground(false);
				this.renderWater(true);
				this.renderProps(true);
				this.renderBucket(true);
				this.renderSnake(true);
				break;
			case 1:
				this.renderMountain(false);
				this.renderMountainCollider(true);
				this.renderBackground(true);
				this.renderWater(true);
				this.renderProps(true);
				this.renderBucket(true);
				this.renderSnake(true);
				break;
			case 2:
				this.renderMountain(false);
				this.renderMountainCollider(false);
				this.renderBackground(true);
				this.renderWater(true);
				this.renderProps(true);
				this.renderBucket(true);
				this.renderSnake(true);
				break;
			case 3:
				this.renderMountain(false);
				this.renderMountainCollider(false);
				this.renderBackground(false);
				this.renderWater(true);
				this.renderProps(true);
				this.renderBucket(true);
				this.renderSnake(true);
				break;
			case 4:
				this.renderMountain(false);
				this.renderMountainCollider(false);
				this.renderBackground(false);
				this.renderWater(false);
				this.renderProps(true);
				this.renderBucket(true);
				this.renderSnake(true);
				break;
			case 5:
				this.renderMountain(false);
				this.renderMountainCollider(false);
				this.renderBackground(false);
				this.renderWater(false);
				this.renderProps(false);
				this.renderBucket(true);
				this.renderSnake(true);
				break;
			case 6:
				this.renderMountain(false);
				this.renderMountainCollider(false);
				this.renderBackground(false);
				this.renderWater(false);
				this.renderProps(false);
				this.renderBucket(false);
				this.renderSnake(true);
				break;
			case 7:
				this.renderMountain(false);
				this.renderMountainCollider(false);
				this.renderBackground(false);
				this.renderWater(false);
				this.renderProps(false);
				this.renderBucket(false);
				this.renderSnake(false);
				break;
			}
		}
		catch
		{
		}
		this.enabled = true;
		return true;
	}

	// Token: 0x06001AAA RID: 6826 RVA: 0x00081028 File Offset: 0x0007F228
	public ModInvisibleMountain()
	{
		this.name = "Invisible Mountain";
		this.version = "1.0";
		this.author = "AS13B";
		this.web = "twitch.tv/as13b";
		this.optionsTitle = "";
		this.options = new List<string> { "No Background", "70%", "75%", "80%", "85%", "90%", "95%", "100%" };
		this.option = 0;
		this.enabled = false;
		this.enableSplits = true;
	}

	// Token: 0x06001AAB RID: 6827 RVA: 0x000810EC File Offset: 0x0007F2EC
	public bool UnLoad()
	{
		try
		{
			this.renderMountain(true);
			this.renderMountainCollider(true);
			this.renderBackground(true);
			this.renderWater(true);
			this.renderProps(true);
			this.renderBucket(true);
			this.renderSnake(true);
		}
		catch
		{
		}
		this.enabled = false;
		return true;
	}

	// Token: 0x1700074D RID: 1869
	// (get) Token: 0x06001AAC RID: 6828 RVA: 0x00013A28 File Offset: 0x00011C28
	public string name { get; }

	// Token: 0x1700074E RID: 1870
	// (get) Token: 0x06001AAD RID: 6829 RVA: 0x00013A30 File Offset: 0x00011C30
	public string version { get; }

	// Token: 0x1700074F RID: 1871
	// (get) Token: 0x06001AAE RID: 6830 RVA: 0x00013A38 File Offset: 0x00011C38
	public string author { get; }

	// Token: 0x17000750 RID: 1872
	// (get) Token: 0x06001AAF RID: 6831 RVA: 0x00013A40 File Offset: 0x00011C40
	public string web { get; }

	// Token: 0x06001AB0 RID: 6832 RVA: 0x00081148 File Offset: 0x0007F348
	public string Update(float deltaTime, GameObject player)
	{
		try
		{
			if (this.enabled && this.lastOption != this.option)
			{
				this.Load();
			}
		}
		catch
		{
		}
		return "";
	}

	// Token: 0x06001AB1 RID: 6833 RVA: 0x0008118C File Offset: 0x0007F38C
	public void renderMountain(bool val)
	{
		try
		{
			Renderer[] componentsInChildren = GameObject.Find("Mountain").GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = val;
			}
		}
		catch
		{
		}
	}

	// Token: 0x06001AB2 RID: 6834 RVA: 0x000811D8 File Offset: 0x0007F3D8
	public void renderMountainCollider(bool val)
	{
		try
		{
			Renderer[] componentsInChildren = GameObject.Find("Mountain_NoCollide").GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = val;
			}
		}
		catch
		{
		}
	}

	// Token: 0x06001AB3 RID: 6835 RVA: 0x00081224 File Offset: 0x0007F424
	public void renderBackground(bool val)
	{
		try
		{
			Renderer[] componentsInChildren = GameObject.Find("Background").GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = val;
			}
		}
		catch
		{
		}
	}

	// Token: 0x06001AB4 RID: 6836 RVA: 0x00081270 File Offset: 0x0007F470
	public void renderWater(bool val)
	{
		try
		{
			Renderer[] componentsInChildren = GameObject.Find("Water").GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = val;
			}
		}
		catch
		{
		}
	}

	// Token: 0x06001AB5 RID: 6837 RVA: 0x000812BC File Offset: 0x0007F4BC
	public void renderProps(bool val)
	{
		try
		{
			Renderer[] componentsInChildren = GameObject.Find("Props").GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = val;
			}
		}
		catch
		{
		}
	}

	// Token: 0x06001AB6 RID: 6838 RVA: 0x00081308 File Offset: 0x0007F508
	public void renderBucket(bool val)
	{
		try
		{
			Renderer[] componentsInChildren = GameObject.Find("Rope4").GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = val;
			}
		}
		catch
		{
		}
	}

	// Token: 0x06001AB7 RID: 6839 RVA: 0x00081354 File Offset: 0x0007F554
	public void renderSnake(bool val)
	{
		try
		{
			Renderer[] componentsInChildren = GameObject.Find("Snake").GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = val;
			}
		}
		catch
		{
		}
	}

	// Token: 0x17000751 RID: 1873
	// (get) Token: 0x06001AB8 RID: 6840 RVA: 0x00013A48 File Offset: 0x00011C48
	// (set) Token: 0x06001AB9 RID: 6841 RVA: 0x00013A50 File Offset: 0x00011C50
	public int option { get; set; }

	// Token: 0x17000752 RID: 1874
	// (get) Token: 0x06001ABA RID: 6842 RVA: 0x00013A59 File Offset: 0x00011C59
	public List<string> options { get; }

	// Token: 0x17000753 RID: 1875
	// (get) Token: 0x06001ABB RID: 6843 RVA: 0x00013A61 File Offset: 0x00011C61
	// (set) Token: 0x06001ABC RID: 6844 RVA: 0x00013A69 File Offset: 0x00011C69
	public bool enabled { get; set; }

	// Token: 0x17000754 RID: 1876
	// (get) Token: 0x06001ABD RID: 6845 RVA: 0x00013A72 File Offset: 0x00011C72
	// (set) Token: 0x06001ABE RID: 6846 RVA: 0x00013A7A File Offset: 0x00011C7A
	public string optionsTitle { get; set; }

	// Token: 0x17000755 RID: 1877
	// (get) Token: 0x06001ABF RID: 6847 RVA: 0x00013A83 File Offset: 0x00011C83
	// (set) Token: 0x06001AC0 RID: 6848 RVA: 0x00013A8B File Offset: 0x00011C8B
	public bool enableSplits { get; set; }

	// Token: 0x06001AC1 RID: 6849 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001AC2 RID: 6850 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x04001215 RID: 4629
	private int lastOption;
}
