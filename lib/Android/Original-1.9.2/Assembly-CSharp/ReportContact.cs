using System;
using UnityEngine;

// Token: 0x0200013D RID: 317
public class ReportContact : MonoBehaviour
{
	// Token: 0x0600082F RID: 2095 RVA: 0x000479F9 File Offset: 0x00045DF9
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			this.detector.triggers[this.detectorNum] = true;
		}
	}

	// Token: 0x06000830 RID: 2096 RVA: 0x00047A28 File Offset: 0x00045E28
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			this.detector.triggers[this.detectorNum] = false;
		}
	}

	// Token: 0x0400082B RID: 2091
	public StuckDetector detector;

	// Token: 0x0400082C RID: 2092
	public int detectorNum;
}
