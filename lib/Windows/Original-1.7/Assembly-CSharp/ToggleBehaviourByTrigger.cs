using System;
using UnityEngine;

// Token: 0x02000022 RID: 34
public class ToggleBehaviourByTrigger : MonoBehaviour
{
	// Token: 0x06000135 RID: 309 RVA: 0x0000D10C File Offset: 0x0000B30C
	private void OnTriggerEnter()
	{
		if (this.UIElement)
		{
			this.UIElement.enabled = !this.UIElement.enabled;
		}
	}

	// Token: 0x040001FB RID: 507
	public Behaviour UIElement;
}
