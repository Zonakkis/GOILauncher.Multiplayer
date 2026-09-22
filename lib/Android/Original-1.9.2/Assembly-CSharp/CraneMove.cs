using System;
using UnityEngine;

// Token: 0x0200011E RID: 286
public class CraneMove : MonoBehaviour
{
	// Token: 0x0600076D RID: 1901 RVA: 0x0003EFF7 File Offset: 0x0003D3F7
	private void Start()
	{
		this.initialPos = base.transform.position;
		this.rb = base.GetComponent<Rigidbody2D>();
	}

	// Token: 0x0600076E RID: 1902 RVA: 0x0003F01B File Offset: 0x0003D41B
	private void Update()
	{
		this.rb.MovePosition(new Vector2(this.initialPos.x + Mathf.Sin(Time.time) * 0.01f, this.initialPos.y));
	}

	// Token: 0x04000698 RID: 1688
	private Vector2 initialPos;

	// Token: 0x04000699 RID: 1689
	private Rigidbody2D rb;
}
