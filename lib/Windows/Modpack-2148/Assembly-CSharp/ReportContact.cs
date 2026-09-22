using System;
using UnityEngine;

// Token: 0x02000037 RID: 55
public class ReportContact : MonoBehaviour
{
	// Token: 0x06000159 RID: 345 RVA: 0x000033A4 File Offset: 0x000015A4
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			this.detector.triggers[this.detectorNum] = true;
		}
	}

	// Token: 0x0600015A RID: 346 RVA: 0x000033D0 File Offset: 0x000015D0
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			this.detector.triggers[this.detectorNum] = false;
		}
	}

	// Token: 0x0400024F RID: 591
	public StuckDetector detector;

	// Token: 0x04000250 RID: 592
	public int detectorNum;
}
