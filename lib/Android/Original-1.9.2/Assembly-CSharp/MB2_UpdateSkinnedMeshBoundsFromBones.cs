using System;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000C3 RID: 195
public class MB2_UpdateSkinnedMeshBoundsFromBones : MonoBehaviour
{
	// Token: 0x060005A6 RID: 1446 RVA: 0x00033030 File Offset: 0x00031430
	private void Start()
	{
		this.smr = base.GetComponent<SkinnedMeshRenderer>();
		if (this.smr == null)
		{
			Debug.LogError("Need to attach MB2_UpdateSkinnedMeshBoundsFromBones script to an object with a SkinnedMeshRenderer component attached.");
			return;
		}
		this.bones = this.smr.bones;
		bool updateWhenOffscreen = this.smr.updateWhenOffscreen;
		this.smr.updateWhenOffscreen = true;
		this.smr.updateWhenOffscreen = updateWhenOffscreen;
	}

	// Token: 0x060005A7 RID: 1447 RVA: 0x0003309A File Offset: 0x0003149A
	private void Update()
	{
		if (this.smr != null)
		{
			MB3_MeshCombiner.UpdateSkinnedMeshApproximateBoundsFromBonesStatic(this.bones, this.smr);
		}
	}

	// Token: 0x040004B5 RID: 1205
	private SkinnedMeshRenderer smr;

	// Token: 0x040004B6 RID: 1206
	private Transform[] bones;
}
