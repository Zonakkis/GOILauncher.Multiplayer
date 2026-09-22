using System;
using UnityEngine;

// Token: 0x02000021 RID: 33
public class MontyTransform : MonoBehaviour
{
	// Token: 0x06000132 RID: 306 RVA: 0x0000CFCF File Offset: 0x0000B1CF
	private void Start()
	{
		this.left = new Vector3(-1f, 1f, 1f);
		this.right = new Vector3(1f, 1f, 1f);
	}

	// Token: 0x06000133 RID: 307 RVA: 0x0000D008 File Offset: 0x0000B208
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

	// Token: 0x040001F4 RID: 500
	public Transform leftEye;

	// Token: 0x040001F5 RID: 501
	public Transform rightEye;

	// Token: 0x040001F6 RID: 502
	public Transform head;

	// Token: 0x040001F7 RID: 503
	public Vector3 offset;

	// Token: 0x040001F8 RID: 504
	private Vector3 left;

	// Token: 0x040001F9 RID: 505
	private Vector3 right;

	// Token: 0x040001FA RID: 506
	private Vector3 avgPosition;
}
