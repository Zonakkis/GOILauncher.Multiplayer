using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000317 RID: 791
public class ModRollingOverIt : Mod
{
	// Token: 0x06001B37 RID: 6967 RVA: 0x0008289C File Offset: 0x00080A9C
	public bool Load()
	{
		try
		{
			JointAngleLimits2D jointAngleLimits2D = default(JointAngleLimits2D);
			jointAngleLimits2D.max = 1000000f;
			jointAngleLimits2D.min = -1000000f;
			GameObject.Find("Player").GetComponent<HingeJoint2D>().limits = jointAngleLimits2D;
		}
		catch
		{
		}
		this.enabled = true;
		return true;
	}

	// Token: 0x06001B38 RID: 6968 RVA: 0x000828FC File Offset: 0x00080AFC
	public ModRollingOverIt()
	{
		this.name = "Rolling Over It";
		this.version = "1.0";
		this.author = "pfg";
		this.web = "";
		this.optionsTitle = "";
		this.options = new List<string>();
		this.option = 0;
		this.enabled = false;
		this.limitsDefault = default(JointAngleLimits2D);
		this.limitsDefault.max = 15f;
		this.limitsDefault.min = -15f;
		this.enableSplits = true;
	}

	// Token: 0x06001B39 RID: 6969 RVA: 0x00082994 File Offset: 0x00080B94
	public bool UnLoad()
	{
		try
		{
			GameObject.Find("Player").GetComponent<HingeJoint2D>().limits = this.limitsDefault;
		}
		catch
		{
		}
		this.enabled = false;
		return true;
	}

	// Token: 0x1700078C RID: 1932
	// (get) Token: 0x06001B3A RID: 6970 RVA: 0x00013D9A File Offset: 0x00011F9A
	public string name { get; }

	// Token: 0x1700078D RID: 1933
	// (get) Token: 0x06001B3B RID: 6971 RVA: 0x00013DA2 File Offset: 0x00011FA2
	public string version { get; }

	// Token: 0x1700078E RID: 1934
	// (get) Token: 0x06001B3C RID: 6972 RVA: 0x00013DAA File Offset: 0x00011FAA
	public string author { get; }

	// Token: 0x1700078F RID: 1935
	// (get) Token: 0x06001B3D RID: 6973 RVA: 0x00013DB2 File Offset: 0x00011FB2
	public string web { get; }

	// Token: 0x06001B3E RID: 6974 RVA: 0x000126FA File Offset: 0x000108FA
	public string Update(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x17000790 RID: 1936
	// (get) Token: 0x06001B3F RID: 6975 RVA: 0x00013DBA File Offset: 0x00011FBA
	// (set) Token: 0x06001B40 RID: 6976 RVA: 0x00013DC2 File Offset: 0x00011FC2
	public int option { get; set; }

	// Token: 0x17000791 RID: 1937
	// (get) Token: 0x06001B41 RID: 6977 RVA: 0x00013DCB File Offset: 0x00011FCB
	public List<string> options { get; }

	// Token: 0x17000792 RID: 1938
	// (get) Token: 0x06001B42 RID: 6978 RVA: 0x00013DD3 File Offset: 0x00011FD3
	// (set) Token: 0x06001B43 RID: 6979 RVA: 0x00013DDB File Offset: 0x00011FDB
	public bool enabled { get; set; }

	// Token: 0x17000793 RID: 1939
	// (get) Token: 0x06001B44 RID: 6980 RVA: 0x00013DE4 File Offset: 0x00011FE4
	// (set) Token: 0x06001B45 RID: 6981 RVA: 0x00013DEC File Offset: 0x00011FEC
	public string optionsTitle { get; set; }

	// Token: 0x17000794 RID: 1940
	// (get) Token: 0x06001B46 RID: 6982 RVA: 0x00013DF5 File Offset: 0x00011FF5
	// (set) Token: 0x06001B47 RID: 6983 RVA: 0x00013DFD File Offset: 0x00011FFD
	public bool enableSplits { get; set; }

	// Token: 0x06001B48 RID: 6984 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001B49 RID: 6985 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x04001263 RID: 4707
	private JointAngleLimits2D limitsDefault;
}
