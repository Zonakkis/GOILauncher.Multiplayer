using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002E9 RID: 745
public interface Map
{
	// Token: 0x06001940 RID: 6464
	bool Load();

	// Token: 0x06001941 RID: 6465
	bool UnLoad();

	// Token: 0x170006BE RID: 1726
	// (get) Token: 0x06001942 RID: 6466
	string name { get; }

	// Token: 0x170006BF RID: 1727
	// (get) Token: 0x06001943 RID: 6467
	string version { get; }

	// Token: 0x170006C0 RID: 1728
	// (get) Token: 0x06001944 RID: 6468
	string author { get; }

	// Token: 0x170006C1 RID: 1729
	// (get) Token: 0x06001945 RID: 6469
	string web { get; }

	// Token: 0x06001946 RID: 6470
	string Update(float deltaTime, GameObject player);

	// Token: 0x170006C2 RID: 1730
	// (get) Token: 0x06001947 RID: 6471
	Dictionary<string, Rect> splits { get; }

	// Token: 0x170006C3 RID: 1731
	// (get) Token: 0x06001948 RID: 6472
	Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x170006C4 RID: 1732
	// (get) Token: 0x06001949 RID: 6473
	Dictionary<string, SaveState> teleportSaveStates { get; }

	// Token: 0x0600194A RID: 6474
	string FixedUpdate(float deltaTime, GameObject player);

	// Token: 0x0600194B RID: 6475
	string onGUI(float deltaTime, GameObject player);
}
