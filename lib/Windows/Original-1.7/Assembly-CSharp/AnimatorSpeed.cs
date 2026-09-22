using System;
using UnityEngine;

// Token: 0x02000004 RID: 4
public class AnimatorSpeed : MonoBehaviour
{
	// Token: 0x06000013 RID: 19 RVA: 0x000025D0 File Offset: 0x000007D0
	private void Start()
	{
	}

	// Token: 0x06000014 RID: 20 RVA: 0x000025D2 File Offset: 0x000007D2
	private void Update()
	{
		base.gameObject.GetComponent<Animator>().speed = this.Speed;
	}

	// Token: 0x0400000B RID: 11
	[Range(0f, 5f)]
	public float Speed;
}
