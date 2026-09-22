using System;
using UnityEngine;

// Token: 0x02000086 RID: 134
public class StopSoundWhenFar : MonoBehaviour
{
	// Token: 0x0600038E RID: 910 RVA: 0x000046EE File Offset: 0x000028EE
	private void Start()
	{
		this.source = base.GetComponent<AudioSource>();
		this.distanceSq = 999000000f;
	}

	// Token: 0x0600038F RID: 911 RVA: 0x00032574 File Offset: 0x00030774
	private void Update()
	{
		this.distanceSq = Vector3.SqrMagnitude(this.player.position - base.transform.position);
		if (this.source.isPlaying)
		{
			if (this.distanceSq > this.source.maxDistance * this.source.maxDistance)
			{
				this.source.Stop();
				return;
			}
		}
		else if (this.distanceSq < this.source.maxDistance * this.source.maxDistance)
		{
			this.source.Play();
		}
	}

	// Token: 0x0400055C RID: 1372
	public Transform player;

	// Token: 0x0400055D RID: 1373
	private AudioSource source;

	// Token: 0x0400055E RID: 1374
	private float distanceSq;
}
