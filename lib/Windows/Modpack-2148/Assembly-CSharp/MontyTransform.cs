using System;
using UnityEngine;

// Token: 0x02000035 RID: 53
public class MontyTransform : MonoBehaviour
{
	// Token: 0x06000154 RID: 340 RVA: 0x00003346 File Offset: 0x00001546
	private void Start()
	{
		this.left = new Vector3(-1f, 1f, 1f);
		this.right = new Vector3(1f, 1f, 1f);
	}

	// Token: 0x06000155 RID: 341 RVA: 0x0001E4E4 File Offset: 0x0001C6E4
	private void LateUpdate()
	{
		this.avgPosition = 0.5f * (this.leftEye.position + this.rightEye.position);
		base.transform.position = this.avgPosition + this.offset;
		float num = Mathf.Atan2(this.head.up.y, this.head.up.x) * 57.29578f - 90f;
		base.transform.localEulerAngles = new Vector3(0f, 0f, num);
		if ((double)this.head.forward.x > 0.1)
		{
			base.transform.localScale = this.right;
			return;
		}
		if ((double)this.head.forward.x < -0.1)
		{
			base.transform.localScale = this.left;
		}
	}

	// Token: 0x04000247 RID: 583
	public Transform leftEye;

	// Token: 0x04000248 RID: 584
	public Transform rightEye;

	// Token: 0x04000249 RID: 585
	public Transform head;

	// Token: 0x0400024A RID: 586
	public Vector3 offset;

	// Token: 0x0400024B RID: 587
	private Vector3 left;

	// Token: 0x0400024C RID: 588
	private Vector3 right;

	// Token: 0x0400024D RID: 589
	private Vector3 avgPosition;
}
