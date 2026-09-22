using System;
using UnityEngine;

// Token: 0x02000045 RID: 69
public class RestartOnContact : MonoBehaviour
{
	// Token: 0x06000215 RID: 533 RVA: 0x00014E18 File Offset: 0x00013018
	private void Start()
	{
		this.resetting = false;
	}

	// Token: 0x06000216 RID: 534 RVA: 0x00014E24 File Offset: 0x00013024
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

	// Token: 0x06000217 RID: 535 RVA: 0x00014E78 File Offset: 0x00013078
	public void Reset()
	{
		this.saveManager.ResetPlayerButNotDialogue();
		this.resetting = false;
	}

	// Token: 0x040003A0 RID: 928
	public Saviour saveManager;

	// Token: 0x040003A1 RID: 929
	public CameraControl camControl;

	// Token: 0x040003A2 RID: 930
	public bool resetting;
}
