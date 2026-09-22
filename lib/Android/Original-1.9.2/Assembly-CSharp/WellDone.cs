using System;
using UnityEngine;

// Token: 0x02000150 RID: 336
public class WellDone : MonoBehaviour
{
	// Token: 0x0600094C RID: 2380 RVA: 0x0004B8A8 File Offset: 0x00049CA8
	private void Start()
	{
		this.done = false;
		this.aud = base.GetComponent<AudioSource>();
	}

	// Token: 0x0600094D RID: 2381 RVA: 0x0004B8BD File Offset: 0x00049CBD
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

	// Token: 0x040008DE RID: 2270
	private AudioSource aud;

	// Token: 0x040008DF RID: 2271
	private bool done;
}
