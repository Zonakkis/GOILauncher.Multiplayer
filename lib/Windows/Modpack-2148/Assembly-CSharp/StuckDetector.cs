using System;
using UnityEngine;

// Token: 0x02000087 RID: 135
public class StuckDetector : MonoBehaviour
{
	// Token: 0x06000391 RID: 913 RVA: 0x00004707 File Offset: 0x00002907
	private void Start()
	{
		this.timer = 0f;
		this.done = false;
	}

	// Token: 0x06000392 RID: 914 RVA: 0x0003260C File Offset: 0x0003080C
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

	// Token: 0x0400055F RID: 1375
	public bool[] triggers;

	// Token: 0x04000560 RID: 1376
	public Narrator narrator;

	// Token: 0x04000561 RID: 1377
	private float timer;

	// Token: 0x04000562 RID: 1378
	private bool done;
}
