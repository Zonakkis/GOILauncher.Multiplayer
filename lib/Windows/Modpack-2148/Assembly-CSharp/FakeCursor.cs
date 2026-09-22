using System;
using UnityEngine;

// Token: 0x02000044 RID: 68
public class FakeCursor : MonoBehaviour
{
	// Token: 0x06000197 RID: 407 RVA: 0x000036D1 File Offset: 0x000018D1
	private void Start()
	{
		this.rb.MovePosition(this.tip.position);
	}

	// Token: 0x0400028B RID: 651
	public Rigidbody2D tip;

	// Token: 0x0400028C RID: 652
	public Rigidbody2D rb;
}
