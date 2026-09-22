using System;
using TMPro;
using UnityEngine;

// Token: 0x0200014F RID: 335
public class VersionReadout : MonoBehaviour
{
	// Token: 0x0600094A RID: 2378 RVA: 0x0004B878 File Offset: 0x00049C78
	private void Start()
	{
		this.text = base.GetComponent<TextMeshProUGUI>();
		this.text.text = "version " + Application.version;
	}

	// Token: 0x040008DD RID: 2269
	private TextMeshProUGUI text;
}
