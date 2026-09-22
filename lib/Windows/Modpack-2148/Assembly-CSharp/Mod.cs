using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002EE RID: 750
internal interface Mod
{
	// Token: 0x06001988 RID: 6536
	bool Load();

	// Token: 0x06001989 RID: 6537
	bool UnLoad();

	// Token: 0x170006E1 RID: 1761
	// (get) Token: 0x0600198A RID: 6538
	string name { get; }

	// Token: 0x170006E2 RID: 1762
	// (get) Token: 0x0600198B RID: 6539
	string version { get; }

	// Token: 0x170006E3 RID: 1763
	// (get) Token: 0x0600198C RID: 6540
	string author { get; }

	// Token: 0x170006E4 RID: 1764
	// (get) Token: 0x0600198D RID: 6541
	string web { get; }

	// Token: 0x0600198E RID: 6542
	string Update(float deltaTime, GameObject player);

	// Token: 0x170006E5 RID: 1765
	// (get) Token: 0x0600198F RID: 6543
	List<string> options { get; }

	// Token: 0x170006E6 RID: 1766
	// (get) Token: 0x06001990 RID: 6544
	// (set) Token: 0x06001991 RID: 6545
	int option { get; set; }

	// Token: 0x170006E7 RID: 1767
	// (get) Token: 0x06001992 RID: 6546
	// (set) Token: 0x06001993 RID: 6547
	bool enabled { get; set; }

	// Token: 0x170006E8 RID: 1768
	// (get) Token: 0x06001994 RID: 6548
	// (set) Token: 0x06001995 RID: 6549
	string optionsTitle { get; set; }

	// Token: 0x170006E9 RID: 1769
	// (get) Token: 0x06001996 RID: 6550
	// (set) Token: 0x06001997 RID: 6551
	bool enableSplits { get; set; }

	// Token: 0x06001998 RID: 6552
	string FixedUpdate(float deltaTime, GameObject player);

	// Token: 0x06001999 RID: 6553
	string onGUI(float deltaTime, GameObject player);
}
