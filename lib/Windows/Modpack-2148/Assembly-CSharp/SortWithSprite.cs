using System;
using UnityEngine;

// Token: 0x0200007B RID: 123
public class SortWithSprite : MonoBehaviour
{
	// Token: 0x06000368 RID: 872 RVA: 0x0000453C File Offset: 0x0000273C
	private void Start()
	{
		this.ren = base.GetComponent<SkinnedMeshRenderer>();
		this.ren.sortingLayerName = "Arms";
		this.ren.sortingLayerID = SortingLayer.NameToID("Arms");
		this.ren.sortingOrder = 0;
	}

	// Token: 0x06000369 RID: 873 RVA: 0x0000265E File Offset: 0x0000085E
	private void Update()
	{
	}

	// Token: 0x04000542 RID: 1346
	private SkinnedMeshRenderer ren;
}
