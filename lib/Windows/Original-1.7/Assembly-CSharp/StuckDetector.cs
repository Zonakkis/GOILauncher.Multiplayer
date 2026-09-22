using System;
using UnityEngine;

// Token: 0x0200005A RID: 90
public class StuckDetector : MonoBehaviour
{
	// Token: 0x060002AB RID: 683 RVA: 0x00018A81 File Offset: 0x00016C81
	private void Start()
	{
		this.timer = 0f;
		this.done = false;
	}

	// Token: 0x060002AC RID: 684 RVA: 0x00018A98 File Offset: 0x00016C98
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

	// Token: 0x0400044E RID: 1102
	public bool[] triggers;

	// Token: 0x0400044F RID: 1103
	public Narrator narrator;

	// Token: 0x04000450 RID: 1104
	private float timer;

	// Token: 0x04000451 RID: 1105
	private bool done;
}
