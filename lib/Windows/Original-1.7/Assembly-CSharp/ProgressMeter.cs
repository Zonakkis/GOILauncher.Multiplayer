using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000042 RID: 66
public class ProgressMeter : MonoBehaviour
{
	// Token: 0x06000208 RID: 520 RVA: 0x00014930 File Offset: 0x00012B30
	private void Start()
	{
		this.paused = false;
		this.deltaFastProgress = new List<float>();
		this.deltaSlowProgress = new List<float>();
		this.newDeltaFastProgress = 0f;
		this.newDeltaSlowProgress = 0f;
		this.averageFastDelta = 0f;
		this.averageSlowDelta = 0f;
		this.secs = 0f;
		for (int i = 0; i < this.secondsToWatch; i++)
		{
			this.deltaSlowProgress.Add(this.progressThreshold * (float)this.secondsToWatch);
		}
		for (int j = 0; j < this.framesToWatch; j++)
		{
			this.deltaFastProgress.Add(0f);
		}
		this.lastSlowProgress = 0f;
		this.lastFastProgress = 0f;
		this.secs = 1f;
	}

	// Token: 0x06000209 RID: 521 RVA: 0x00014A0C File Offset: 0x00012C0C
	private void LateUpdate()
	{
		if (this.paused)
		{
			return;
		}
		this.secs += Time.deltaTime;
		this.narrator.SendMessage("UpdateDistance", this.progress);
		this.newDeltaFastProgress = this.progress - this.lastFastProgress;
		this.deltaFastProgress.RemoveAt(0);
		this.deltaFastProgress.Add(this.newDeltaFastProgress);
		this.averageFastDelta = 0f;
		foreach (float num in this.deltaFastProgress)
		{
			this.averageFastDelta += num;
		}
		this.averageFastDelta /= (float)this.framesToWatch;
		this.lastFastProgress = this.progress;
		if (this.secs >= 1f)
		{
			this.secs = 0f;
			this.newDeltaSlowProgress = Mathf.Max(-100f, this.progress - this.lastSlowProgress);
			this.deltaSlowProgress.RemoveAt(0);
			this.deltaSlowProgress.Add(this.newDeltaSlowProgress);
			this.averageSlowDelta = 0f;
			foreach (float num2 in this.deltaSlowProgress)
			{
				this.averageSlowDelta += num2;
			}
			this.averageSlowDelta /= (float)this.secondsToWatch;
			this.lastSlowProgress = this.progress;
			if (this.averageFastDelta > this.lossThreshold && this.averageSlowDelta < this.progressThreshold)
			{
				this.narrator.SendMessage("SlowProgress");
				this.ResetSlowProgress();
			}
		}
		if (this.averageFastDelta < this.lossThreshold)
		{
			this.narrator.SendMessage("FastRetreat");
			for (int i = 0; i < this.deltaFastProgress.Count; i++)
			{
				this.deltaFastProgress[i] = 0f;
			}
			this.ResetSlowProgress();
		}
	}

	// Token: 0x0600020A RID: 522 RVA: 0x00014C8C File Offset: 0x00012E8C
	public void Pause(bool shouldPause)
	{
		this.paused = shouldPause;
	}

	// Token: 0x0600020B RID: 523 RVA: 0x00014C98 File Offset: 0x00012E98
	private void ResetSlowProgress()
	{
		for (int i = 0; i < this.deltaSlowProgress.Count; i++)
		{
			this.deltaSlowProgress[i] = this.progressThreshold * (float)this.secondsToWatch;
		}
	}

	// Token: 0x04000388 RID: 904
	public Transform player;

	// Token: 0x04000389 RID: 905
	public float progress;

	// Token: 0x0400038A RID: 906
	public float currentTF;

	// Token: 0x0400038B RID: 907
	private float lastSlowProgress;

	// Token: 0x0400038C RID: 908
	private float lastFastProgress;

	// Token: 0x0400038D RID: 909
	private float newDeltaFastProgress;

	// Token: 0x0400038E RID: 910
	private float newDeltaSlowProgress;

	// Token: 0x0400038F RID: 911
	private float averageFastDelta;

	// Token: 0x04000390 RID: 912
	private float averageSlowDelta;

	// Token: 0x04000391 RID: 913
	private int secondsToWatch = 60;

	// Token: 0x04000392 RID: 914
	private int framesToWatch = 120;

	// Token: 0x04000393 RID: 915
	private float secs;

	// Token: 0x04000394 RID: 916
	private List<float> deltaSlowProgress;

	// Token: 0x04000395 RID: 917
	private List<float> deltaFastProgress;

	// Token: 0x04000396 RID: 918
	private float lossThreshold = -0.24f;

	// Token: 0x04000397 RID: 919
	private float progressThreshold = 0.15f;

	// Token: 0x04000398 RID: 920
	private float secondsUntilNewProgressThreshold = 90f;

	// Token: 0x04000399 RID: 921
	private bool paused;

	// Token: 0x0400039A RID: 922
	public Narrator narrator;
}
