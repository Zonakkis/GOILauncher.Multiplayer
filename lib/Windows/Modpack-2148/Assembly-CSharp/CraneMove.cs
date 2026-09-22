using System;
using UnityEngine;

// Token: 0x02000041 RID: 65
public class CraneMove : MonoBehaviour
{
	// Token: 0x0600018C RID: 396 RVA: 0x0000361D File Offset: 0x0000181D
	private void Start()
	{
		this.initialPos = base.transform.position;
		this.rb = base.GetComponent<Rigidbody2D>();
	}

	// Token: 0x0600018D RID: 397 RVA: 0x00003641 File Offset: 0x00001841
	private void Update()
	{
		this.rb.MovePosition(new Vector2(this.initialPos.x + Mathf.Sin(Time.time) * 0.01f, this.initialPos.y));
	}

	// Token: 0x04000281 RID: 641
	private Vector2 initialPos;

	// Token: 0x04000282 RID: 642
	private Rigidbody2D rb;
}
