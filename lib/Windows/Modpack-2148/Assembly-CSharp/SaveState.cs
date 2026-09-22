using System;
using System.Xml.Serialization;
using UnityEngine;

// Token: 0x0200006F RID: 111
[XmlRoot("SaveState")]
[Serializable]
public class SaveState
{
	// Token: 0x1700005A RID: 90
	// (get) Token: 0x060002B8 RID: 696 RVA: 0x00003F5D File Offset: 0x0000215D
	// (set) Token: 0x060002B9 RID: 697 RVA: 0x00003F65 File Offset: 0x00002165
	[XmlElement("hingePos")]
	public float hingePos { get; set; }

	// Token: 0x1700005B RID: 91
	// (get) Token: 0x060002BA RID: 698 RVA: 0x00003F6E File Offset: 0x0000216E
	// (set) Token: 0x060002BB RID: 699 RVA: 0x00003F76 File Offset: 0x00002176
	[XmlElement("hingeVel")]
	public float hingeVel { get; set; }

	// Token: 0x1700005C RID: 92
	// (get) Token: 0x060002BC RID: 700 RVA: 0x00003F7F File Offset: 0x0000217F
	// (set) Token: 0x060002BD RID: 701 RVA: 0x00003F87 File Offset: 0x00002187
	[XmlElement("sliderPos")]
	public float sliderPos { get; set; }

	// Token: 0x1700005D RID: 93
	// (get) Token: 0x060002BE RID: 702 RVA: 0x00003F90 File Offset: 0x00002190
	// (set) Token: 0x060002BF RID: 703 RVA: 0x00003F98 File Offset: 0x00002198
	[XmlElement("sliderVel")]
	public float sliderVel { get; set; }

	// Token: 0x1700005E RID: 94
	// (get) Token: 0x060002C0 RID: 704 RVA: 0x00003FA1 File Offset: 0x000021A1
	// (set) Token: 0x060002C1 RID: 705 RVA: 0x00003FA9 File Offset: 0x000021A9
	[XmlElement("camPos")]
	public Vector3 camPos { get; set; }

	// Token: 0x1700005F RID: 95
	// (get) Token: 0x060002C2 RID: 706 RVA: 0x00003FB2 File Offset: 0x000021B2
	// (set) Token: 0x060002C3 RID: 707 RVA: 0x00003FBA File Offset: 0x000021BA
	[XmlElement("playerPos")]
	public Vector3 playerPos { get; set; }

	// Token: 0x17000060 RID: 96
	// (get) Token: 0x060002C4 RID: 708 RVA: 0x00003FC3 File Offset: 0x000021C3
	// (set) Token: 0x060002C5 RID: 709 RVA: 0x00003FCB File Offset: 0x000021CB
	[XmlElement("playerRot")]
	public Quaternion playerRot { get; set; }

	// Token: 0x17000061 RID: 97
	// (get) Token: 0x060002C6 RID: 710 RVA: 0x00003FD4 File Offset: 0x000021D4
	// (set) Token: 0x060002C7 RID: 711 RVA: 0x00003FDC File Offset: 0x000021DC
	[XmlArray("rbLinearVelocities")]
	public Vector2[] rbLinearVelocities { get; set; }

	// Token: 0x17000062 RID: 98
	// (get) Token: 0x060002C8 RID: 712 RVA: 0x00003FE5 File Offset: 0x000021E5
	// (set) Token: 0x060002C9 RID: 713 RVA: 0x00003FED File Offset: 0x000021ED
	[XmlArray("rbAngularVelocities")]
	public float[] rbAngularVelocities { get; set; }

	// Token: 0x17000063 RID: 99
	// (get) Token: 0x060002CA RID: 714 RVA: 0x00003FF6 File Offset: 0x000021F6
	// (set) Token: 0x060002CB RID: 715 RVA: 0x00003FFE File Offset: 0x000021FE
	[XmlArray("rbPositions")]
	public Vector2[] rbPositions { get; set; }

	// Token: 0x17000064 RID: 100
	// (get) Token: 0x060002CC RID: 716 RVA: 0x00004007 File Offset: 0x00002207
	// (set) Token: 0x060002CD RID: 717 RVA: 0x0000400F File Offset: 0x0000220F
	[XmlArray("rbAngles")]
	public float[] rbAngles { get; set; }

	// Token: 0x17000065 RID: 101
	// (get) Token: 0x060002CE RID: 718 RVA: 0x00004018 File Offset: 0x00002218
	// (set) Token: 0x060002CF RID: 719 RVA: 0x00004020 File Offset: 0x00002220
	[XmlElement("saveNum")]
	public int saveNum { get; set; }

	// Token: 0x17000066 RID: 102
	// (get) Token: 0x060002D0 RID: 720 RVA: 0x00004029 File Offset: 0x00002229
	// (set) Token: 0x060002D1 RID: 721 RVA: 0x00004031 File Offset: 0x00002231
	[XmlElement("currentCondolence")]
	public int currentCondolence { get; set; }

	// Token: 0x17000067 RID: 103
	// (get) Token: 0x060002D2 RID: 722 RVA: 0x0000403A File Offset: 0x0000223A
	// (set) Token: 0x060002D3 RID: 723 RVA: 0x00004042 File Offset: 0x00002242
	[XmlElement("currentObservation")]
	public int currentObservation { get; set; }

	// Token: 0x17000068 RID: 104
	// (get) Token: 0x060002D4 RID: 724 RVA: 0x0000404B File Offset: 0x0000224B
	// (set) Token: 0x060002D5 RID: 725 RVA: 0x00004053 File Offset: 0x00002253
	[XmlElement("keyDialogDone")]
	public string keyDialogDone { get; set; }

	// Token: 0x17000069 RID: 105
	// (get) Token: 0x060002D6 RID: 726 RVA: 0x0000405C File Offset: 0x0000225C
	// (set) Token: 0x060002D7 RID: 727 RVA: 0x00004064 File Offset: 0x00002264
	[XmlElement("condolenceDialogDone")]
	public string condolenceDialogDone { get; set; }

	// Token: 0x1700006A RID: 106
	// (get) Token: 0x060002D8 RID: 728 RVA: 0x0000406D File Offset: 0x0000226D
	// (set) Token: 0x060002D9 RID: 729 RVA: 0x00004075 File Offset: 0x00002275
	[XmlElement("observationDialogDone")]
	public string observationDialogDone { get; set; }

	// Token: 0x1700006B RID: 107
	// (get) Token: 0x060002DA RID: 730 RVA: 0x0000407E File Offset: 0x0000227E
	// (set) Token: 0x060002DB RID: 731 RVA: 0x00004086 File Offset: 0x00002286
	[XmlElement("timePlayed")]
	public float timePlayed { get; set; }

	// Token: 0x1700006C RID: 108
	// (get) Token: 0x060002DC RID: 732 RVA: 0x0000408F File Offset: 0x0000228F
	// (set) Token: 0x060002DD RID: 733 RVA: 0x00004097 File Offset: 0x00002297
	[XmlElement("version")]
	public string version { get; set; }

	// Token: 0x1700006D RID: 109
	// (get) Token: 0x060002DE RID: 734 RVA: 0x000040A0 File Offset: 0x000022A0
	// (set) Token: 0x060002DF RID: 735 RVA: 0x000040A8 File Offset: 0x000022A8
	[XmlElement("speedrun")]
	public bool speedrun { get; set; }

	// Token: 0x060002E0 RID: 736 RVA: 0x000275C0 File Offset: 0x000257C0
	public SaveState()
	{
		this.hingePos = 0f;
		this.hingeVel = 0f;
		this.sliderPos = 0f;
		this.sliderVel = 0f;
		this.camPos = new Vector3(-43.57285f, -1.2893757f, -20f);
		this.playerPos = Vector3.zero;
		this.playerRot = Quaternion.identity;
		this.rbLinearVelocities = new Vector2[1];
		this.rbPositions = new Vector2[1];
		this.rbAngularVelocities = new float[1];
		this.rbAngles = new float[1];
		this.saveNum = -1;
		this.currentCondolence = 0;
		this.currentObservation = 0;
		this.keyDialogDone = "";
		this.condolenceDialogDone = "";
		this.observationDialogDone = "";
		this.timePlayed = 0f;
		this.version = "";
		this.speedrun = false;
		this.sidewaysEnabled = false;
		this.sidewaysAngle = 0f;
	}

	// Token: 0x1700006E RID: 110
	// (get) Token: 0x060002E1 RID: 737 RVA: 0x000040B1 File Offset: 0x000022B1
	// (set) Token: 0x060002E2 RID: 738 RVA: 0x000040B9 File Offset: 0x000022B9
	[XmlElement("sidewaysEnabled")]
	public bool sidewaysEnabled { get; set; }

	// Token: 0x1700006F RID: 111
	// (get) Token: 0x060002E3 RID: 739 RVA: 0x000040C2 File Offset: 0x000022C2
	// (set) Token: 0x060002E4 RID: 740 RVA: 0x000040CA File Offset: 0x000022CA
	[XmlElement("sidewaysAngle")]
	public float sidewaysAngle { get; set; }
}
