using System;
using UnityEngine;

// Token: 0x0200004E RID: 78
public class SortWithSprite : MonoBehaviour
{
	// Token: 0x06000282 RID: 642 RVA: 0x00018361 File Offset: 0x00016561
	private void Start()
	{
		this.ren = base.GetComponent<SkinnedMeshRenderer>();
		this.ren.sortingLayerName = "Arms";
		this.ren.sortingLayerID = SortingLayer.NameToID("Arms");
		this.ren.sortingOrder = 0;
	}

	// Token: 0x06000283 RID: 643 RVA: 0x000183A0 File Offset: 0x000165A0
	private void Update()
	{
	}

	// Token: 0x04000431 RID: 1073
	private SkinnedMeshRenderer ren;
}
