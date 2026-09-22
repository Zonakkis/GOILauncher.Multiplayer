using System;
using UnityEngine;

// Token: 0x02000032 RID: 50
[ExecuteInEditMode]
public class ToggleChildren : MonoBehaviour
{
	// Token: 0x0600013B RID: 315 RVA: 0x0000D8D8 File Offset: 0x0000BCD8
	private void OnEnable()
	{
		this.Children = new GameObject[base.gameObject.transform.childCount];
		for (int i = 0; i < base.gameObject.transform.childCount; i++)
		{
			this.Children[i] = base.gameObject.transform.GetChild(i).gameObject;
		}
	}

	// Token: 0x0600013C RID: 316 RVA: 0x0000D940 File Offset: 0x0000BD40
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

	// Token: 0x0400023B RID: 571
	public KeyCode Key = KeyCode.F;

	// Token: 0x0400023C RID: 572
	private GameObject[] Children;

	// Token: 0x0400023D RID: 573
	private bool active = true;
}
