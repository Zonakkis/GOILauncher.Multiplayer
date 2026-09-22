using System;
using UnityEngine;

// Token: 0x02000043 RID: 67
public class PropSound : MonoBehaviour
{
	// Token: 0x0600020D RID: 525 RVA: 0x00014D0E File Offset: 0x00012F0E
	private void Start()
	{
		this.audioSource = base.GetComponent<AudioSource>();
		this.audioSource.playOnAwake = false;
		this.audioSource.loop = false;
	}

	// Token: 0x0600020E RID: 526 RVA: 0x00014D34 File Offset: 0x00012F34
	public void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.audioSource == null)
		{
			return;
		}
		if (coll.relativeVelocity.magnitude > 0.2f && !this.audioSource.isPlaying)
		{
			this.audioSource.volume = Mathf.Log(coll.relativeVelocity.magnitude + 1f);
			this.audioSource.pitch = 1f + Random.Range(-0.1f, 0.1f);
			this.audioSource.Play();
		}
	}

	// Token: 0x0400039B RID: 923
	private AudioSource audioSource;
}
