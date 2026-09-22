using System;
using UnityEngine;

// Token: 0x0200006B RID: 107
public class RestartOnContact : MonoBehaviour
{
	// Token: 0x06000299 RID: 665 RVA: 0x00003E55 File Offset: 0x00002055
	private void Start()
	{
		this.resetting = false;
	}

	// Token: 0x0600029A RID: 666 RVA: 0x00026D88 File Offset: 0x00024F88
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.resetting)
		{
			return;
		}
		if (coll.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			this.camControl.FadeOut();
			base.Invoke("Reset", 1f);
			this.resetting = true;
		}
	}

	// Token: 0x0600029B RID: 667 RVA: 0x00003E5E File Offset: 0x0000205E
	public void Reset()
	{
		this.saveManager.ResetPlayerButNotDialogue();
		this.resetting = false;
	}

	// Token: 0x04000443 RID: 1091
	public Saviour saveManager;

	// Token: 0x04000444 RID: 1092
	public CameraControl camControl;

	// Token: 0x04000445 RID: 1093
	public bool resetting;
}
