using System;
using UnityEngine;

// Token: 0x02000078 RID: 120
public class MB2_TestShowHide : MonoBehaviour
{
	// Token: 0x06000373 RID: 883 RVA: 0x0001B418 File Offset: 0x00019818
	private void Update()
	{
		if (Time.frameCount == 100)
		{
			this.mb.ShowHide(null, this.objs);
			this.mb.ApplyShowHide();
			Debug.Log("should have disappeared");
		}
		if (Time.frameCount == 200)
		{
			this.mb.ShowHide(this.objs, null);
			this.mb.ApplyShowHide();
			Debug.Log("should show");
		}
	}

	// Token: 0x04000352 RID: 850
	public MB3_MeshBaker mb;

	// Token: 0x04000353 RID: 851
	public GameObject[] objs;
}
