using System;
using UnityEngine;

// Token: 0x02000036 RID: 54
public class ToggleBehaviourByTrigger : MonoBehaviour
{
	// Token: 0x06000157 RID: 343 RVA: 0x0000337C File Offset: 0x0000157C
	private void OnTriggerEnter()
	{
		if (this.UIElement)
		{
			this.UIElement.enabled = !this.UIElement.enabled;
		}
	}

	// Token: 0x0400024E RID: 590
	public Behaviour UIElement;
}
