using System;
using UnityEngine;

// Token: 0x0200013E RID: 318
public class RestartOnContact : MonoBehaviour
{
	// Token: 0x06000832 RID: 2098 RVA: 0x00047A5F File Offset: 0x00045E5F
	private void Start()
	{
		this.resetting = false;
		if (this.ScreenFader == null)
		{
			this.ScreenFader = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenFader>();
		}
	}

	// Token: 0x06000833 RID: 2099 RVA: 0x00047A90 File Offset: 0x00045E90
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.resetting)
		{
			return;
		}
		if (coll.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			this.ScreenFader.EndScene(ScreenFader.ScreenFaderExitType.ResetPlayerButNotDialogue);
			this.resetting = true;
			base.Invoke("ResetResetting", 1f);
		}
	}

	// Token: 0x06000834 RID: 2100 RVA: 0x00047AEB File Offset: 0x00045EEB
	private void ResetResetting()
	{
		this.resetting = false;
	}

	// Token: 0x0400082D RID: 2093
	public ScreenFader ScreenFader;

	// Token: 0x0400082E RID: 2094
	private bool resetting;
}
