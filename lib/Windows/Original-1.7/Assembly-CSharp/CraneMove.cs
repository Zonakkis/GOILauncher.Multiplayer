using System;
using UnityEngine;

// Token: 0x0200002C RID: 44
public class CraneMove : MonoBehaviour
{
	// Token: 0x06000162 RID: 354 RVA: 0x0000DCDA File Offset: 0x0000BEDA
	private void Start()
	{
		this.initialPos = base.transform.position;
		this.rb = base.GetComponent<Rigidbody2D>();
	}

	// Token: 0x06000163 RID: 355 RVA: 0x0000DCFE File Offset: 0x0000BEFE
	private void Update()
	{
		this.rb.MovePosition(new Vector2(this.initialPos.x + Mathf.Sin(Time.time) * 0.01f, this.initialPos.y));
	}

	// Token: 0x0400022B RID: 555
	private Vector2 initialPos;

	// Token: 0x0400022C RID: 556
	private Rigidbody2D rb;
}
