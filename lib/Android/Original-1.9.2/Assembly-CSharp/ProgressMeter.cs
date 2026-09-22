using System;
using UnityEngine;

// Token: 0x0200013A RID: 314
public class ProgressMeter : MonoBehaviour
{
	// Token: 0x06000822 RID: 2082 RVA: 0x00047584 File Offset: 0x00045984
	private void Start()
	{
		this.paused = false;
		this.deltaFastProgress = new float[this.framesToWatch];
		this.deltaSlowProgress = new float[this.secondsToWatch];
		this.newDeltaFastProgress = 0f;
		this.newDeltaSlowProgress = 0f;
		this.averageFastDelta = 0f;
		this.averageSlowDelta = 0f;
		this.secs = 0f;
		for (int i = 0; i < this.secondsToWatch; i++)
		{
			this.deltaSlowProgress[i] = this.progressThreshold * (float)this.secondsToWatch;
		}
		for (int j = 0; j < this.framesToWatch; j++)
		{
			this.deltaFastProgress[j] = 0f;
		}
		this.lastSlowProgress = 0f;
		this.lastFastProgress = 0f;
		this.secs = 1f;
	}

	// Token: 0x06000823 RID: 2083 RVA: 0x00047670 File Offset: 0x00045A70
	private void LateUpdate()
	{
		if (this.paused)
		{
			return;
		}
		this.secs += Time.deltaTime;
		this.narrator.UpdateDistance(this.progress);
		this.newDeltaFastProgress = this.progress - this.lastFastProgress;
		Array.Copy(this.deltaFastProgress, 0, this.deltaFastProgress, 1, this.deltaFastProgress.Length - 2);
		this.deltaFastProgress[0] = this.newDeltaFastProgress;
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
			Array.Copy(this.deltaSlowProgress, 0, this.deltaSlowProgress, 1, this.deltaSlowProgress.Length - 2);
			this.deltaSlowProgress[0] = this.newDeltaSlowProgress;
			this.averageSlowDelta = 0f;
			foreach (float num2 in this.deltaSlowProgress)
			{
				this.averageSlowDelta += num2;
			}
			this.averageSlowDelta /= (float)this.secondsToWatch;
			this.lastSlowProgress = this.progress;
			if (this.averageFastDelta > this.lossThreshold && this.averageSlowDelta < this.progressThreshold)
			{
				this.narrator.SlowProgress();
				this.ResetSlowProgress();
			}
		}
		if (this.averageFastDelta < this.lossThreshold)
		{
			this.LastSeriousLossTimestamp = DateTimeOffset.UtcNow;
			this.narrator.FastRetreat();
			Array.Clear(this.deltaFastProgress, 0, this.deltaFastProgress.Length);
			this.ResetSlowProgress();
		}
	}

	// Token: 0x06000824 RID: 2084 RVA: 0x00047897 File Offset: 0x00045C97
	public void Pause(bool shouldPause)
	{
		this.paused = shouldPause;
	}

	// Token: 0x06000825 RID: 2085 RVA: 0x000478A0 File Offset: 0x00045CA0
	private void ResetSlowProgress()
	{
		for (int i = 0; i < this.deltaSlowProgress.Length; i++)
		{
			this.deltaSlowProgress[i] = this.progressThreshold * (float)this.secondsToWatch;
		}
	}

	// Token: 0x04000813 RID: 2067
	public Transform player;

	// Token: 0x04000814 RID: 2068
	public CameraControl camControl;

	// Token: 0x04000815 RID: 2069
	public float progress;

	// Token: 0x04000816 RID: 2070
	private float lastSlowProgress;

	// Token: 0x04000817 RID: 2071
	private float lastFastProgress;

	// Token: 0x04000818 RID: 2072
	private float newDeltaFastProgress;

	// Token: 0x04000819 RID: 2073
	private float newDeltaSlowProgress;

	// Token: 0x0400081A RID: 2074
	private float averageFastDelta;

	// Token: 0x0400081B RID: 2075
	private float averageSlowDelta;

	// Token: 0x0400081C RID: 2076
	private int secondsToWatch = 60;

	// Token: 0x0400081D RID: 2077
	private int framesToWatch = 120;

	// Token: 0x0400081E RID: 2078
	private float secs;

	// Token: 0x0400081F RID: 2079
	private float[] deltaSlowProgress = new float[0];

	// Token: 0x04000820 RID: 2080
	private float[] deltaFastProgress = new float[0];

	// Token: 0x04000821 RID: 2081
	private float lossThreshold = -0.24f;

	// Token: 0x04000822 RID: 2082
	private float progressThreshold = 0.15f;

	// Token: 0x04000823 RID: 2083
	private bool paused;

	// Token: 0x04000824 RID: 2084
	public Narrator narrator;

	// Token: 0x04000825 RID: 2085
	public DateTimeOffset LastSeriousLossTimestamp;
}
