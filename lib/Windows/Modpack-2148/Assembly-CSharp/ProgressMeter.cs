using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000068 RID: 104
public class ProgressMeter : MonoBehaviour
{
	// Token: 0x0600028C RID: 652 RVA: 0x0002695C File Offset: 0x00024B5C
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

	// Token: 0x0600028D RID: 653 RVA: 0x00026A38 File Offset: 0x00024C38
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

	// Token: 0x0600028E RID: 654 RVA: 0x00003DA6 File Offset: 0x00001FA6
	public void Pause(bool shouldPause)
	{
		this.paused = shouldPause;
	}

	// Token: 0x0600028F RID: 655 RVA: 0x00026CB8 File Offset: 0x00024EB8
	private void ResetSlowProgress()
	{
		for (int i = 0; i < this.deltaSlowProgress.Count; i++)
		{
			this.deltaSlowProgress[i] = this.progressThreshold * (float)this.secondsToWatch;
		}
	}

	// Token: 0x0400042B RID: 1067
	public Transform player;

	// Token: 0x0400042C RID: 1068
	public float progress;

	// Token: 0x0400042D RID: 1069
	public float currentTF;

	// Token: 0x0400042E RID: 1070
	private float lastSlowProgress;

	// Token: 0x0400042F RID: 1071
	private float lastFastProgress;

	// Token: 0x04000430 RID: 1072
	private float newDeltaFastProgress;

	// Token: 0x04000431 RID: 1073
	private float newDeltaSlowProgress;

	// Token: 0x04000432 RID: 1074
	private float averageFastDelta;

	// Token: 0x04000433 RID: 1075
	private float averageSlowDelta;

	// Token: 0x04000434 RID: 1076
	private int secondsToWatch = 60;

	// Token: 0x04000435 RID: 1077
	private int framesToWatch = 120;

	// Token: 0x04000436 RID: 1078
	private float secs;

	// Token: 0x04000437 RID: 1079
	private List<float> deltaSlowProgress;

	// Token: 0x04000438 RID: 1080
	private List<float> deltaFastProgress;

	// Token: 0x04000439 RID: 1081
	private float lossThreshold = -0.24f;

	// Token: 0x0400043A RID: 1082
	private float progressThreshold = 0.15f;

	// Token: 0x0400043B RID: 1083
	private float secondsUntilNewProgressThreshold = 90f;

	// Token: 0x0400043C RID: 1084
	private bool paused;

	// Token: 0x0400043D RID: 1085
	public Narrator narrator;
}
