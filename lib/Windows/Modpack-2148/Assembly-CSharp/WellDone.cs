using System;
using UnityEngine;

// Token: 0x02000083 RID: 131
public class WellDone : MonoBehaviour
{
	// Token: 0x06000385 RID: 901 RVA: 0x00004683 File Offset: 0x00002883
	private void Start()
	{
		this.done = false;
		this.aud = base.GetComponent<AudioSource>();
	}

	// Token: 0x06000386 RID: 902 RVA: 0x00004698 File Offset: 0x00002898
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (this.done)
		{
			return;
		}
		if (coll.name != "PotCollider")
		{
			return;
		}
		this.done = true;
		this.aud.Play();
	}

	// Token: 0x04000558 RID: 1368
	private AudioSource aud;

	// Token: 0x04000559 RID: 1369
	private bool done;
}
