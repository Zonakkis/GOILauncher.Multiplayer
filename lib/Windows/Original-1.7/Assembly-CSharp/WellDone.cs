using System;
using UnityEngine;

// Token: 0x02000056 RID: 86
public class WellDone : MonoBehaviour
{
	// Token: 0x0600029F RID: 671 RVA: 0x00018943 File Offset: 0x00016B43
	private void Start()
	{
		this.done = false;
		this.aud = base.GetComponent<AudioSource>();
	}

	// Token: 0x060002A0 RID: 672 RVA: 0x00018958 File Offset: 0x00016B58
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

	// Token: 0x04000447 RID: 1095
	private AudioSource aud;

	// Token: 0x04000448 RID: 1096
	private bool done;
}
