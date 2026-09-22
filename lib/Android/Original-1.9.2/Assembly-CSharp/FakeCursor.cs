using System;
using UnityEngine;

// Token: 0x02000121 RID: 289
public class FakeCursor : MonoBehaviour
{
	// Token: 0x06000778 RID: 1912 RVA: 0x0003F22F File Offset: 0x0003D62F
	private void Start()
	{
		this.rb.MovePosition(this.tip.position);
	}

	// Token: 0x040006A4 RID: 1700
	public Rigidbody2D tip;

	// Token: 0x040006A5 RID: 1701
	public Rigidbody2D rb;
}
