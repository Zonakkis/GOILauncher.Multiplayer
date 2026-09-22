using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000FE RID: 254
public class ShaderTour : MonoBehaviour
{
	// Token: 0x060006B8 RID: 1720 RVA: 0x0003B534 File Offset: 0x00039934
	private void Awake()
	{
		if (Application.isEditor)
		{
			Application.runInBackground = true;
		}
	}

	// Token: 0x060006B9 RID: 1721 RVA: 0x0003B548 File Offset: 0x00039948
	private IEnumerator Start()
	{
		this.debugSpawner = GameObject.FindGameObjectWithTag("DebugSpawner").GetComponent<DebugSpawn>();
		this.mobileMan = GameObject.FindGameObjectWithTag("MobileManager").GetComponent<MobileManager>();
		yield return new WaitForSeconds(2f);
		if (this.doTour)
		{
			base.StartCoroutine("Tour");
		}
		yield break;
	}

	// Token: 0x060006BA RID: 1722 RVA: 0x0003B564 File Offset: 0x00039964
	private IEnumerator Tour()
	{
		while (this.j < 22)
		{
			Debug.Log("Next Quality Settings: " + this.j);
			this.mobileMan.SetNewQualitySettings(this.j);
			while (this.i < 24)
			{
				yield return new WaitForSeconds(1f);
				this.debugSpawner.GoRight();
				this.i++;
				Debug.Log("Spawn " + this.i + " of 24");
			}
			this.j++;
			this.i = 0;
		}
		Debug.Log("Tour Complete");
		yield return null;
		yield break;
	}

	// Token: 0x060006BB RID: 1723 RVA: 0x0003B57F File Offset: 0x0003997F
	private void Update()
	{
	}

	// Token: 0x040005FF RID: 1535
	public bool doTour;

	// Token: 0x04000600 RID: 1536
	private DebugSpawn debugSpawner;

	// Token: 0x04000601 RID: 1537
	private MobileManager mobileMan;

	// Token: 0x04000602 RID: 1538
	private int i;

	// Token: 0x04000603 RID: 1539
	private int j;
}
