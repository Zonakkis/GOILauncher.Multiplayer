using System;
using TMPro;
using UnityEngine;

// Token: 0x02000055 RID: 85
public class VersionReadout : MonoBehaviour
{
	// Token: 0x0600029D RID: 669 RVA: 0x00018913 File Offset: 0x00016B13
	private void Start()
	{
		this.text = base.GetComponent<TextMeshProUGUI>();
		this.text.text = "version " + Application.version;
	}

	// Token: 0x04000446 RID: 1094
	private TextMeshProUGUI text;
}
