using System;
using UnityEngine;

// Token: 0x02000059 RID: 89
public class StopSoundWhenFar : MonoBehaviour
{
	// Token: 0x060002A8 RID: 680 RVA: 0x000189CA File Offset: 0x00016BCA
	private void Start()
	{
		this.source = base.GetComponent<AudioSource>();
		this.distanceSq = 999000000f;
	}

	// Token: 0x060002A9 RID: 681 RVA: 0x000189E4 File Offset: 0x00016BE4
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

	// Token: 0x0400044B RID: 1099
	public Transform player;

	// Token: 0x0400044C RID: 1100
	private AudioSource source;

	// Token: 0x0400044D RID: 1101
	private float distanceSq;
}
