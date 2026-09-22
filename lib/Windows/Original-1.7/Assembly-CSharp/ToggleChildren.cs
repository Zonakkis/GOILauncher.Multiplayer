using System;
using UnityEngine;

// Token: 0x0200001E RID: 30
[ExecuteInEditMode]
public class ToggleChildren : MonoBehaviour
{
	// Token: 0x06000119 RID: 281 RVA: 0x0000C7F4 File Offset: 0x0000A9F4
	private void OnEnable()
	{
		this.Children = new GameObject[base.gameObject.transform.childCount];
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			this.Children[i] = base.gameObject.transform.GetChild(i).gameObject;
		}
	}

	// Token: 0x0600011A RID: 282 RVA: 0x0000C858 File Offset: 0x0000AA58
	private void Update()
	{
		if (Input.GetKeyDown(this.Key))
		{
			this.active = !this.active;
			for (int i = 0; i < base.gameObject.transform.childCount; i++)
			{
				this.Children[i].SetActive(this.active);
			}
		}
	}

	// Token: 0x040001E1 RID: 481
	public KeyCode Key = KeyCode.F;

	// Token: 0x040001E2 RID: 482
	private GameObject[] Children;

	// Token: 0x040001E3 RID: 483
	private bool active = true;
}
