using System;
using UnityEngine;

// Token: 0x0200014C RID: 332
public class StuckDetector : MonoBehaviour
{
	// Token: 0x06000940 RID: 2368 RVA: 0x0004B627 File Offset: 0x00049A27
	private void Start()
	{
		this.timer = 0f;
		this.done = false;
	}

	// Token: 0x06000941 RID: 2369 RVA: 0x0004B63C File Offset: 0x00049A3C
	private void Update()
	{
		if (this.triggers[0] && this.triggers[1])
		{
			this.timer += Time.deltaTime;
		}
		else
		{
			this.timer = 0f;
		}
		if (this.timer > 2f && !this.done)
		{
			this.narrator.StuckOnAntenna();
			this.done = true;
		}
	}

	// Token: 0x040008D3 RID: 2259
	public bool[] triggers;

	// Token: 0x040008D4 RID: 2260
	public Narrator narrator;

	// Token: 0x040008D5 RID: 2261
	private float timer;

	// Token: 0x040008D6 RID: 2262
	private bool done;
}
