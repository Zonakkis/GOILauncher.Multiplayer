using System;
using UnityEngine;

// Token: 0x0200013B RID: 315
public class PropSound : MonoBehaviour
{
	// Token: 0x06000827 RID: 2087 RVA: 0x000478E4 File Offset: 0x00045CE4
	private void Start()
	{
		this.audioSource = base.GetComponent<AudioSource>();
		this.audioSource.playOnAwake = false;
		this.audioSource.loop = false;
	}

	// Token: 0x06000828 RID: 2088 RVA: 0x0004790C File Offset: 0x00045D0C
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

	// Token: 0x04000826 RID: 2086
	private AudioSource audioSource;
}
