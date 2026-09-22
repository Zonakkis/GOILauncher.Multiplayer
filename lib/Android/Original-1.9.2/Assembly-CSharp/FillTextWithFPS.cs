using System;
using TMPro;
using UnityEngine;

// Token: 0x020000E9 RID: 233
public class FillTextWithFPS : MonoBehaviour
{
	// Token: 0x06000678 RID: 1656 RVA: 0x000379BF File Offset: 0x00035DBF
	private void Start()
	{
		this.label = base.GetComponent<TextMeshProUGUI>();
		base.InvokeRepeating("UpdateFPSLabel", 0.5f, 0.5f);
	}

	// Token: 0x06000679 RID: 1657 RVA: 0x000379E2 File Offset: 0x00035DE2
	private void UpdateFPSLabel()
	{
		this.label.text = this.mobileManager.m_CurrentFps.ToString("0");
	}

	// Token: 0x0400052E RID: 1326
	public MobileManager mobileManager;

	// Token: 0x0400052F RID: 1327
	private TextMeshProUGUI label;
}
