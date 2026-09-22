using System;
using UnityEngine;

// Token: 0x0200013C RID: 316
public class RattlePot : MonoBehaviour
{
	// Token: 0x0600082A RID: 2090 RVA: 0x000479AA File Offset: 0x00045DAA
	private void Start()
	{
		this.aud = base.GetComponent<AudioSource>();
	}

	// Token: 0x0600082B RID: 2091 RVA: 0x000479B8 File Offset: 0x00045DB8
	public void Rattle1()
	{
		this.aud.PlayOneShot(this.rattle1);
	}

	// Token: 0x0600082C RID: 2092 RVA: 0x000479CB File Offset: 0x00045DCB
	public void Rattle2()
	{
		this.aud.PlayOneShot(this.rattle2);
	}

	// Token: 0x0600082D RID: 2093 RVA: 0x000479DE File Offset: 0x00045DDE
	public void Groan()
	{
		this.aud.PlayOneShot(this.groan);
	}

	// Token: 0x04000827 RID: 2087
	public AudioClip rattle1;

	// Token: 0x04000828 RID: 2088
	public AudioClip rattle2;

	// Token: 0x04000829 RID: 2089
	public AudioClip groan;

	// Token: 0x0400082A RID: 2090
	private AudioSource aud;
}
