using System;
using UnityEngine;

// Token: 0x02000023 RID: 35
public class ReportContact : MonoBehaviour
{
	// Token: 0x06000137 RID: 311 RVA: 0x0000D13C File Offset: 0x0000B33C
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			this.detector.triggers[this.detectorNum] = true;
		}
	}

	// Token: 0x06000138 RID: 312 RVA: 0x0000D168 File Offset: 0x0000B368
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			this.detector.triggers[this.detectorNum] = false;
		}
	}

	// Token: 0x040001FC RID: 508
	public StuckDetector detector;

	// Token: 0x040001FD RID: 509
	public int detectorNum;
}
