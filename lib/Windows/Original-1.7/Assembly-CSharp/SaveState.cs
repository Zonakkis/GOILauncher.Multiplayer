using System;
using System.Xml.Serialization;
using UnityEngine;

// Token: 0x02000047 RID: 71
[XmlRoot("SaveState")]
[Serializable]
public class SaveState
{
	// Token: 0x17000029 RID: 41
	// (get) Token: 0x06000227 RID: 551 RVA: 0x000154B0 File Offset: 0x000136B0
	// (set) Token: 0x06000228 RID: 552 RVA: 0x000154B8 File Offset: 0x000136B8
	[XmlElement("hingePos")]
	public float hingePos { get; set; }

	// Token: 0x1700002A RID: 42
	// (get) Token: 0x06000229 RID: 553 RVA: 0x000154C1 File Offset: 0x000136C1
	// (set) Token: 0x0600022A RID: 554 RVA: 0x000154C9 File Offset: 0x000136C9
	[XmlElement("hingeVel")]
	public float hingeVel { get; set; }

	// Token: 0x1700002B RID: 43
	// (get) Token: 0x0600022B RID: 555 RVA: 0x000154D2 File Offset: 0x000136D2
	// (set) Token: 0x0600022C RID: 556 RVA: 0x000154DA File Offset: 0x000136DA
	[XmlElement("sliderPos")]
	public float sliderPos { get; set; }

	// Token: 0x1700002C RID: 44
	// (get) Token: 0x0600022D RID: 557 RVA: 0x000154E3 File Offset: 0x000136E3
	// (set) Token: 0x0600022E RID: 558 RVA: 0x000154EB File Offset: 0x000136EB
	[XmlElement("sliderVel")]
	public float sliderVel { get; set; }

	// Token: 0x1700002D RID: 45
	// (get) Token: 0x0600022F RID: 559 RVA: 0x000154F4 File Offset: 0x000136F4
	// (set) Token: 0x06000230 RID: 560 RVA: 0x000154FC File Offset: 0x000136FC
	[XmlElement("camPos")]
	public Vector3 camPos { get; set; }

	// Token: 0x1700002E RID: 46
	// (get) Token: 0x06000231 RID: 561 RVA: 0x00015505 File Offset: 0x00013705
	// (set) Token: 0x06000232 RID: 562 RVA: 0x0001550D File Offset: 0x0001370D
	[XmlElement("playerPos")]
	public Vector3 playerPos { get; set; }

	// Token: 0x1700002F RID: 47
	// (get) Token: 0x06000233 RID: 563 RVA: 0x00015516 File Offset: 0x00013716
	// (set) Token: 0x06000234 RID: 564 RVA: 0x0001551E File Offset: 0x0001371E
	[XmlElement("playerRot")]
	public Quaternion playerRot { get; set; }

	// Token: 0x17000030 RID: 48
	// (get) Token: 0x06000235 RID: 565 RVA: 0x00015527 File Offset: 0x00013727
	// (set) Token: 0x06000236 RID: 566 RVA: 0x0001552F File Offset: 0x0001372F
	[XmlArray("rbLinearVelocities")]
	public Vector2[] rbLinearVelocities { get; set; }

	// Token: 0x17000031 RID: 49
	// (get) Token: 0x06000237 RID: 567 RVA: 0x00015538 File Offset: 0x00013738
	// (set) Token: 0x06000238 RID: 568 RVA: 0x00015540 File Offset: 0x00013740
	[XmlArray("rbAngularVelocities")]
	public float[] rbAngularVelocities { get; set; }

	// Token: 0x17000032 RID: 50
	// (get) Token: 0x06000239 RID: 569 RVA: 0x00015549 File Offset: 0x00013749
	// (set) Token: 0x0600023A RID: 570 RVA: 0x00015551 File Offset: 0x00013751
	[XmlArray("rbPositions")]
	public Vector2[] rbPositions { get; set; }

	// Token: 0x17000033 RID: 51
	// (get) Token: 0x0600023B RID: 571 RVA: 0x0001555A File Offset: 0x0001375A
	// (set) Token: 0x0600023C RID: 572 RVA: 0x00015562 File Offset: 0x00013762
	[XmlArray("rbAngles")]
	public float[] rbAngles { get; set; }

	// Token: 0x17000034 RID: 52
	// (get) Token: 0x0600023D RID: 573 RVA: 0x0001556B File Offset: 0x0001376B
	// (set) Token: 0x0600023E RID: 574 RVA: 0x00015573 File Offset: 0x00013773
	[XmlElement("saveNum")]
	public int saveNum { get; set; }

	// Token: 0x17000035 RID: 53
	// (get) Token: 0x0600023F RID: 575 RVA: 0x0001557C File Offset: 0x0001377C
	// (set) Token: 0x06000240 RID: 576 RVA: 0x00015584 File Offset: 0x00013784
	[XmlElement("currentCondolence")]
	public int currentCondolence { get; set; }

	// Token: 0x17000036 RID: 54
	// (get) Token: 0x06000241 RID: 577 RVA: 0x0001558D File Offset: 0x0001378D
	// (set) Token: 0x06000242 RID: 578 RVA: 0x00015595 File Offset: 0x00013795
	[XmlElement("currentObservation")]
	public int currentObservation { get; set; }

	// Token: 0x17000037 RID: 55
	// (get) Token: 0x06000243 RID: 579 RVA: 0x0001559E File Offset: 0x0001379E
	// (set) Token: 0x06000244 RID: 580 RVA: 0x000155A6 File Offset: 0x000137A6
	[XmlElement("keyDialogDone")]
	public string keyDialogDone { get; set; }

	// Token: 0x17000038 RID: 56
	// (get) Token: 0x06000245 RID: 581 RVA: 0x000155AF File Offset: 0x000137AF
	// (set) Token: 0x06000246 RID: 582 RVA: 0x000155B7 File Offset: 0x000137B7
	[XmlElement("condolenceDialogDone")]
	public string condolenceDialogDone { get; set; }

	// Token: 0x17000039 RID: 57
	// (get) Token: 0x06000247 RID: 583 RVA: 0x000155C0 File Offset: 0x000137C0
	// (set) Token: 0x06000248 RID: 584 RVA: 0x000155C8 File Offset: 0x000137C8
	[XmlElement("observationDialogDone")]
	public string observationDialogDone { get; set; }

	// Token: 0x1700003A RID: 58
	// (get) Token: 0x06000249 RID: 585 RVA: 0x000155D1 File Offset: 0x000137D1
	// (set) Token: 0x0600024A RID: 586 RVA: 0x000155D9 File Offset: 0x000137D9
	[XmlElement("timePlayed")]
	public float timePlayed { get; set; }

	// Token: 0x1700003B RID: 59
	// (get) Token: 0x0600024B RID: 587 RVA: 0x000155E2 File Offset: 0x000137E2
	// (set) Token: 0x0600024C RID: 588 RVA: 0x000155EA File Offset: 0x000137EA
	[XmlElement("version")]
	public string version { get; set; }

	// Token: 0x1700003C RID: 60
	// (get) Token: 0x0600024D RID: 589 RVA: 0x000155F3 File Offset: 0x000137F3
	// (set) Token: 0x0600024E RID: 590 RVA: 0x000155FB File Offset: 0x000137FB
	[XmlElement("speedrun")]
	public bool speedrun { get; set; }

	// Token: 0x0600024F RID: 591 RVA: 0x00015604 File Offset: 0x00013804
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
	}
}
