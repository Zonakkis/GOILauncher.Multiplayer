using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000C8 RID: 200
public class MB3_DisableHiddenAnimations : MonoBehaviour
{
	// Token: 0x060005AF RID: 1455 RVA: 0x0003321F File Offset: 0x0003161F
	private void Start()
	{
		if (base.GetComponent<SkinnedMeshRenderer>() == null)
		{
			Debug.LogError("The MB3_CullHiddenAnimations script was placed on and object " + base.name + " which has no SkinnedMeshRenderer attached");
		}
	}

	// Token: 0x060005B0 RID: 1456 RVA: 0x0003324C File Offset: 0x0003164C
	private void OnBecameVisible()
	{
		for (int i = 0; i < this.animationsToCull.Count; i++)
		{
			if (this.animationsToCull[i] != null)
			{
				this.animationsToCull[i].enabled = true;
			}
		}
	}

	// Token: 0x060005B1 RID: 1457 RVA: 0x000332A0 File Offset: 0x000316A0
	private void OnBecameInvisible()
	{
		for (int i = 0; i < this.animationsToCull.Count; i++)
		{
			if (this.animationsToCull[i] != null)
			{
				this.animationsToCull[i].enabled = false;
			}
		}
	}

	// Token: 0x040004C2 RID: 1218
	public List<Animation> animationsToCull = new List<Animation>();
}
