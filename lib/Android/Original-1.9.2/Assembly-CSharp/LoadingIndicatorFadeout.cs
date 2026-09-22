using System;
using TMPro;
using UnityEngine;

// Token: 0x020000EB RID: 235
public class LoadingIndicatorFadeout : MonoBehaviour
{
	// Token: 0x0600068E RID: 1678 RVA: 0x00037EA1 File Offset: 0x000362A1
	private void Awake()
	{
		this.loadingLabel = base.GetComponent<TextMeshProUGUI>();
	}

	// Token: 0x0600068F RID: 1679 RVA: 0x00037EB0 File Offset: 0x000362B0
	private void Update()
	{
		float num = this.loadingLabel.alpha - Time.deltaTime / this.duration;
		this.loadingLabel.alpha = Mathf.Max(0f, num);
		if (this.loadingLabel.alpha < 0.1f)
		{
			base.gameObject.SetActive(false);
		}
	}

	// Token: 0x04000541 RID: 1345
	private TextMeshProUGUI loadingLabel;

	// Token: 0x04000542 RID: 1346
	private float duration = 0.25f;
}
