using System;
using UnityEngine;

// Token: 0x02000031 RID: 49
[ExecuteInEditMode]
public class ToggleChildren : MonoBehaviour
{
	// Token: 0x0600013B RID: 315 RVA: 0x0001DE7C File Offset: 0x0001C07C
	private void OnEnable()
	{
		this.Children = new GameObject[base.gameObject.transform.childCount];
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			this.Children[i] = base.gameObject.transform.GetChild(i).gameObject;
		}
	}

	// Token: 0x0600013C RID: 316 RVA: 0x0001DEE0 File Offset: 0x0001C0E0
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

	// Token: 0x0400022E RID: 558
	public KeyCode Key = KeyCode.F;

	// Token: 0x0400022F RID: 559
	private GameObject[] Children;

	// Token: 0x04000230 RID: 560
	private bool active = true;
}
