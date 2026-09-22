using System;
using UnityEngine;

// Token: 0x02000044 RID: 68
public class RattlePot : MonoBehaviour
{
	// Token: 0x06000210 RID: 528 RVA: 0x00014DC9 File Offset: 0x00012FC9
	private void Start()
	{
		this.aud = base.GetComponent<AudioSource>();
	}

	// Token: 0x06000211 RID: 529 RVA: 0x00014DD7 File Offset: 0x00012FD7
	public void Rattle1()
	{
		this.aud.PlayOneShot(this.rattle1);
	}

	// Token: 0x06000212 RID: 530 RVA: 0x00014DEA File Offset: 0x00012FEA
	public void Rattle2()
	{
		this.aud.PlayOneShot(this.rattle2);
	}

	// Token: 0x06000213 RID: 531 RVA: 0x00014DFD File Offset: 0x00012FFD
	public void Groan()
	{
		this.aud.PlayOneShot(this.groan);
	}

	// Token: 0x0400039C RID: 924
	public AudioClip rattle1;

	// Token: 0x0400039D RID: 925
	public AudioClip rattle2;

	// Token: 0x0400039E RID: 926
	public AudioClip groan;

	// Token: 0x0400039F RID: 927
	private AudioSource aud;
}
