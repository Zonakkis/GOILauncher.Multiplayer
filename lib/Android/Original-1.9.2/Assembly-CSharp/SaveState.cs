using System;
using UnityEngine;

// Token: 0x02000140 RID: 320
[Serializable]
public class SaveState
{
	// Token: 0x06000841 RID: 2113 RVA: 0x00047FB4 File Offset: 0x000463B4
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
		this.keyDialogDone = string.Empty;
		this.condolenceDialogDone = string.Empty;
		this.observationDialogDone = string.Empty;
		this.timePlayed = 0f;
		this.version = string.Empty;
		this.speedrun = false;
	}

	// Token: 0x170000A9 RID: 169
	// (get) Token: 0x06000842 RID: 2114 RVA: 0x000480A6 File Offset: 0x000464A6
	// (set) Token: 0x06000843 RID: 2115 RVA: 0x000480AE File Offset: 0x000464AE
	public float hingePos { get; set; }

	// Token: 0x170000AA RID: 170
	// (get) Token: 0x06000844 RID: 2116 RVA: 0x000480B7 File Offset: 0x000464B7
	// (set) Token: 0x06000845 RID: 2117 RVA: 0x000480BF File Offset: 0x000464BF
	public float hingeVel { get; set; }

	// Token: 0x170000AB RID: 171
	// (get) Token: 0x06000846 RID: 2118 RVA: 0x000480C8 File Offset: 0x000464C8
	// (set) Token: 0x06000847 RID: 2119 RVA: 0x000480D0 File Offset: 0x000464D0
	public float sliderPos { get; set; }

	// Token: 0x170000AC RID: 172
	// (get) Token: 0x06000848 RID: 2120 RVA: 0x000480D9 File Offset: 0x000464D9
	// (set) Token: 0x06000849 RID: 2121 RVA: 0x000480E1 File Offset: 0x000464E1
	public float sliderVel { get; set; }

	// Token: 0x170000AD RID: 173
	// (get) Token: 0x0600084A RID: 2122 RVA: 0x000480EA File Offset: 0x000464EA
	// (set) Token: 0x0600084B RID: 2123 RVA: 0x000480F2 File Offset: 0x000464F2
	public Vector3 camPos { get; set; }

	// Token: 0x170000AE RID: 174
	// (get) Token: 0x0600084C RID: 2124 RVA: 0x000480FB File Offset: 0x000464FB
	// (set) Token: 0x0600084D RID: 2125 RVA: 0x00048103 File Offset: 0x00046503
	public Vector3 playerPos { get; set; }

	// Token: 0x170000AF RID: 175
	// (get) Token: 0x0600084E RID: 2126 RVA: 0x0004810C File Offset: 0x0004650C
	// (set) Token: 0x0600084F RID: 2127 RVA: 0x00048114 File Offset: 0x00046514
	public Quaternion playerRot { get; set; }

	// Token: 0x170000B0 RID: 176
	// (get) Token: 0x06000850 RID: 2128 RVA: 0x0004811D File Offset: 0x0004651D
	// (set) Token: 0x06000851 RID: 2129 RVA: 0x00048125 File Offset: 0x00046525
	public Vector2[] rbLinearVelocities { get; set; }

	// Token: 0x170000B1 RID: 177
	// (get) Token: 0x06000852 RID: 2130 RVA: 0x0004812E File Offset: 0x0004652E
	// (set) Token: 0x06000853 RID: 2131 RVA: 0x00048136 File Offset: 0x00046536
	public float[] rbAngularVelocities { get; set; }

	// Token: 0x170000B2 RID: 178
	// (get) Token: 0x06000854 RID: 2132 RVA: 0x0004813F File Offset: 0x0004653F
	// (set) Token: 0x06000855 RID: 2133 RVA: 0x00048147 File Offset: 0x00046547
	public Vector2[] rbPositions { get; set; }

	// Token: 0x170000B3 RID: 179
	// (get) Token: 0x06000856 RID: 2134 RVA: 0x00048150 File Offset: 0x00046550
	// (set) Token: 0x06000857 RID: 2135 RVA: 0x00048158 File Offset: 0x00046558
	public float[] rbAngles { get; set; }

	// Token: 0x170000B4 RID: 180
	// (get) Token: 0x06000858 RID: 2136 RVA: 0x00048161 File Offset: 0x00046561
	// (set) Token: 0x06000859 RID: 2137 RVA: 0x00048169 File Offset: 0x00046569
	public int saveNum { get; set; }

	// Token: 0x170000B5 RID: 181
	// (get) Token: 0x0600085A RID: 2138 RVA: 0x00048172 File Offset: 0x00046572
	// (set) Token: 0x0600085B RID: 2139 RVA: 0x0004817A File Offset: 0x0004657A
	public int currentCondolence { get; set; }

	// Token: 0x170000B6 RID: 182
	// (get) Token: 0x0600085C RID: 2140 RVA: 0x00048183 File Offset: 0x00046583
	// (set) Token: 0x0600085D RID: 2141 RVA: 0x0004818B File Offset: 0x0004658B
	public int currentObservation { get; set; }

	// Token: 0x170000B7 RID: 183
	// (get) Token: 0x0600085E RID: 2142 RVA: 0x00048194 File Offset: 0x00046594
	// (set) Token: 0x0600085F RID: 2143 RVA: 0x0004819C File Offset: 0x0004659C
	public string keyDialogDone { get; set; }

	// Token: 0x170000B8 RID: 184
	// (get) Token: 0x06000860 RID: 2144 RVA: 0x000481A5 File Offset: 0x000465A5
	// (set) Token: 0x06000861 RID: 2145 RVA: 0x000481AD File Offset: 0x000465AD
	public string condolenceDialogDone { get; set; }

	// Token: 0x170000B9 RID: 185
	// (get) Token: 0x06000862 RID: 2146 RVA: 0x000481B6 File Offset: 0x000465B6
	// (set) Token: 0x06000863 RID: 2147 RVA: 0x000481BE File Offset: 0x000465BE
	public string observationDialogDone { get; set; }

	// Token: 0x170000BA RID: 186
	// (get) Token: 0x06000864 RID: 2148 RVA: 0x000481C7 File Offset: 0x000465C7
	// (set) Token: 0x06000865 RID: 2149 RVA: 0x000481CF File Offset: 0x000465CF
	public float timePlayed { get; set; }

	// Token: 0x170000BB RID: 187
	// (get) Token: 0x06000866 RID: 2150 RVA: 0x000481D8 File Offset: 0x000465D8
	// (set) Token: 0x06000867 RID: 2151 RVA: 0x000481E0 File Offset: 0x000465E0
	public string version { get; set; }

	// Token: 0x170000BC RID: 188
	// (get) Token: 0x06000868 RID: 2152 RVA: 0x000481E9 File Offset: 0x000465E9
	// (set) Token: 0x06000869 RID: 2153 RVA: 0x000481F1 File Offset: 0x000465F1
	public bool speedrun { get; set; }
}
