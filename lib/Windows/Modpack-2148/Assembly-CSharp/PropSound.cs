using System;
using UnityEngine;

// Token: 0x02000069 RID: 105
public class PropSound : MonoBehaviour
{
	// Token: 0x06000291 RID: 657 RVA: 0x00003DE8 File Offset: 0x00001FE8
	private void Start()
	{
		this.audioSource = base.GetComponent<AudioSource>();
		this.audioSource.playOnAwake = false;
		this.audioSource.loop = false;
	}

	// Token: 0x06000292 RID: 658 RVA: 0x00026CF8 File Offset: 0x00024EF8
	public void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.audioSource == null)
		{
			return;
		}
		if (coll.relativeVelocity.magnitude > 0.2f && !this.audioSource.isPlaying)
		{
			this.audioSource.volume = Mathf.Log(coll.relativeVelocity.magnitude + 1f);
			this.audioSource.pitch = 1f + global::UnityEngine.Random.Range(-0.1f, 0.1f);
			this.audioSource.Play();
		}
	}

	// Token: 0x0400043E RID: 1086
	private AudioSource audioSource;
}
