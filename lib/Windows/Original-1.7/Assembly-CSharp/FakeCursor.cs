using System;
using UnityEngine;

// Token: 0x0200002F RID: 47
public class FakeCursor : MonoBehaviour
{
	// Token: 0x0600016D RID: 365 RVA: 0x0000DEDD File Offset: 0x0000C0DD
	private void Start()
	{
		this.rb.MovePosition(this.tip.position);
	}

	// Token: 0x04000235 RID: 565
	public Rigidbody2D tip;

	// Token: 0x04000236 RID: 566
	public Rigidbody2D rb;
}
