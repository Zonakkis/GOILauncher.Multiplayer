using System;
using TMPro;
using UnityEngine;

// Token: 0x02000082 RID: 130
public class VersionReadout : MonoBehaviour
{
	// Token: 0x06000383 RID: 899 RVA: 0x0000465B File Offset: 0x0000285B
	private void Start()
	{
		this.text = base.GetComponent<TextMeshProUGUI>();
		this.text.text = "version " + Application.version;
	}

	// Token: 0x04000557 RID: 1367
	private TextMeshProUGUI text;
}
