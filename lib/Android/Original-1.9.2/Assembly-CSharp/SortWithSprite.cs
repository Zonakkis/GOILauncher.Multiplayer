using System;
using UnityEngine;

// Token: 0x02000149 RID: 329
public class SortWithSprite : MonoBehaviour
{
	// Token: 0x06000937 RID: 2359 RVA: 0x0004B3B7 File Offset: 0x000497B7
	private void Start()
	{
		this.ren = base.GetComponent<SkinnedMeshRenderer>();
		this.ren.sortingLayerName = "Arms";
		this.ren.sortingLayerID = SortingLayer.NameToID("Arms");
		this.ren.sortingOrder = 0;
	}

	// Token: 0x06000938 RID: 2360 RVA: 0x0004B3F6 File Offset: 0x000497F6
	private void Update()
	{
	}

	// Token: 0x040008C8 RID: 2248
	private SkinnedMeshRenderer ren;
}
