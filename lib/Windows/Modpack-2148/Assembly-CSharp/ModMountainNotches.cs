using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200030E RID: 782
public class ModMountainNotches : Mod
{
	// Token: 0x06001AC3 RID: 6851 RVA: 0x000813A0 File Offset: 0x0007F5A0
	public bool Load()
	{
		try
		{
			global::UnityEngine.Object.Destroy(this.line);
			global::UnityEngine.Object.Destroy(this.lr);
			global::UnityEngine.Object.Destroy(this.line2);
			global::UnityEngine.Object.Destroy(this.lr2);
			this.lastOption = this.option;
			this.line = new GameObject("line");
			this.lr = this.line.AddComponent<LineRenderer>();
			this.lr.material = new Material(Shader.Find("Sprites/Default"));
			this.lr.SetColors(this.optionColors[this.option], this.optionColors[this.option]);
			this.lr.useWorldSpace = true;
			PolygonCollider2D polygonCollider2D = GameObject.Find("Mountain").GetComponentsInChildren<PolygonCollider2D>()[199];
			this.lr.SetVertexCount(polygonCollider2D.points.Length + 1);
			this.lr.SetWidth(0.03f, 0.03f);
			for (int i = 0; i < polygonCollider2D.points.Length; i++)
			{
				this.lr.SetPosition(i, new Vector3(polygonCollider2D.transform.TransformPoint(polygonCollider2D.points[i]).x + polygonCollider2D.offset.x, polygonCollider2D.transform.TransformPoint(polygonCollider2D.points[i]).y + polygonCollider2D.offset.y, polygonCollider2D.transform.TransformPoint(polygonCollider2D.points[i]).z - 15.5f));
			}
			this.lr.SetPosition(polygonCollider2D.points.Length, new Vector3(polygonCollider2D.transform.TransformPoint(polygonCollider2D.points[0]).x + polygonCollider2D.offset.x, polygonCollider2D.transform.TransformPoint(polygonCollider2D.points[0]).y + polygonCollider2D.offset.y, polygonCollider2D.transform.TransformPoint(polygonCollider2D.points[0]).z - 15.5f));
			this.line2 = new GameObject("line2");
			this.lr2 = this.line2.AddComponent<LineRenderer>();
			this.lr2.material = new Material(Shader.Find("Sprites/Default"));
			this.lr2.SetColors(this.optionColors[this.option], this.optionColors[this.option]);
			this.lr2.useWorldSpace = true;
			PolygonCollider2D polygonCollider2D2 = GameObject.Find("Mountain").GetComponentsInChildren<PolygonCollider2D>()[197];
			this.lr2.SetVertexCount(polygonCollider2D2.points.Length + 1);
			this.lr2.SetWidth(0.03f, 0.03f);
			for (int j = 0; j < polygonCollider2D2.points.Length; j++)
			{
				this.lr2.SetPosition(j, new Vector3(polygonCollider2D2.transform.TransformPoint(polygonCollider2D2.points[j]).x + polygonCollider2D2.offset.x, polygonCollider2D2.transform.TransformPoint(polygonCollider2D2.points[j]).y + polygonCollider2D2.offset.y, polygonCollider2D2.transform.TransformPoint(polygonCollider2D2.points[j]).z - 15.5f));
			}
			this.lr2.SetPosition(polygonCollider2D2.points.Length, new Vector3(polygonCollider2D2.transform.TransformPoint(polygonCollider2D2.points[0]).x + polygonCollider2D2.offset.x, polygonCollider2D2.transform.TransformPoint(polygonCollider2D2.points[0]).y + polygonCollider2D2.offset.y, polygonCollider2D2.transform.TransformPoint(polygonCollider2D2.points[0]).z - 15.5f));
		}
		catch
		{
		}
		this.enabled = true;
		return true;
	}

	// Token: 0x06001AC4 RID: 6852 RVA: 0x00081800 File Offset: 0x0007FA00
	public ModMountainNotches()
	{
		this.name = "Mountain Notches";
		this.version = "1.0";
		this.author = "AS13B";
		this.web = "twitch.tv/as13b";
		this.optionsTitle = "Line Color";
		this.options = new List<string> { "White", "Black", "Red", "Green", "Blue", "Cyan", "Grey", "Yellow", "Magenta" };
		this.option = 0;
		this.enabled = false;
		this.optionColors = new Color[]
		{
			Color.white,
			Color.black,
			Color.red,
			Color.green,
			Color.blue,
			Color.cyan,
			Color.grey,
			Color.yellow,
			Color.magenta
		};
		this.enableSplits = false;
	}

	// Token: 0x06001AC5 RID: 6853 RVA: 0x00081948 File Offset: 0x0007FB48
	public bool UnLoad()
	{
		try
		{
			global::UnityEngine.Object.Destroy(this.line);
			global::UnityEngine.Object.Destroy(this.lr);
			global::UnityEngine.Object.Destroy(this.line2);
			global::UnityEngine.Object.Destroy(this.lr2);
		}
		catch
		{
		}
		this.enabled = false;
		return true;
	}

	// Token: 0x17000756 RID: 1878
	// (get) Token: 0x06001AC6 RID: 6854 RVA: 0x00013A94 File Offset: 0x00011C94
	public string name { get; }

	// Token: 0x17000757 RID: 1879
	// (get) Token: 0x06001AC7 RID: 6855 RVA: 0x00013A9C File Offset: 0x00011C9C
	public string version { get; }

	// Token: 0x17000758 RID: 1880
	// (get) Token: 0x06001AC8 RID: 6856 RVA: 0x00013AA4 File Offset: 0x00011CA4
	public string author { get; }

	// Token: 0x17000759 RID: 1881
	// (get) Token: 0x06001AC9 RID: 6857 RVA: 0x00013AAC File Offset: 0x00011CAC
	public string web { get; }

	// Token: 0x06001ACA RID: 6858 RVA: 0x000819A0 File Offset: 0x0007FBA0
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

	// Token: 0x1700075A RID: 1882
	// (get) Token: 0x06001ACB RID: 6859 RVA: 0x00013AB4 File Offset: 0x00011CB4
	// (set) Token: 0x06001ACC RID: 6860 RVA: 0x00013ABC File Offset: 0x00011CBC
	public int option { get; set; }

	// Token: 0x1700075B RID: 1883
	// (get) Token: 0x06001ACD RID: 6861 RVA: 0x00013AC5 File Offset: 0x00011CC5
	public List<string> options { get; }

	// Token: 0x1700075C RID: 1884
	// (get) Token: 0x06001ACE RID: 6862 RVA: 0x00013ACD File Offset: 0x00011CCD
	// (set) Token: 0x06001ACF RID: 6863 RVA: 0x00013AD5 File Offset: 0x00011CD5
	public bool enabled { get; set; }

	// Token: 0x1700075D RID: 1885
	// (get) Token: 0x06001AD0 RID: 6864 RVA: 0x00013ADE File Offset: 0x00011CDE
	// (set) Token: 0x06001AD1 RID: 6865 RVA: 0x00013AE6 File Offset: 0x00011CE6
	public string optionsTitle { get; set; }

	// Token: 0x1700075E RID: 1886
	// (get) Token: 0x06001AD2 RID: 6866 RVA: 0x00013AEF File Offset: 0x00011CEF
	// (set) Token: 0x06001AD3 RID: 6867 RVA: 0x00013AF7 File Offset: 0x00011CF7
	public bool enableSplits { get; set; }

	// Token: 0x06001AD4 RID: 6868 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001AD5 RID: 6869 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x0400121F RID: 4639
	private GameObject line;

	// Token: 0x04001220 RID: 4640
	private LineRenderer lr;

	// Token: 0x04001221 RID: 4641
	public GameObject line2;

	// Token: 0x04001222 RID: 4642
	public LineRenderer lr2;

	// Token: 0x04001223 RID: 4643
	private Color[] optionColors;

	// Token: 0x04001224 RID: 4644
	private int lastOption;
}
