using System;
using UnityEngine;

// Token: 0x0200006A RID: 106
public class RattlePot : MonoBehaviour
{
	// Token: 0x06000294 RID: 660 RVA: 0x00003E0E File Offset: 0x0000200E
	private void Start()
	{
		this.aud = base.GetComponent<AudioSource>();
	}

	// Token: 0x06000295 RID: 661 RVA: 0x00003E1C File Offset: 0x0000201C
	public void Rattle1()
	{
		this.aud.PlayOneShot(this.rattle1);
	}

	// Token: 0x06000296 RID: 662 RVA: 0x00003E2F File Offset: 0x0000202F
	public void Rattle2()
	{
		this.aud.PlayOneShot(this.rattle2);
	}

	// Token: 0x06000297 RID: 663 RVA: 0x00003E42 File Offset: 0x00002042
	public void Groan()
	{
		this.aud.PlayOneShot(this.groan);
	}

	// Token: 0x0400043F RID: 1087
	public AudioClip rattle1;

	// Token: 0x04000440 RID: 1088
	public AudioClip rattle2;

	// Token: 0x04000441 RID: 1089
	public AudioClip groan;

	// Token: 0x04000442 RID: 1090
	private AudioSource aud;
}
