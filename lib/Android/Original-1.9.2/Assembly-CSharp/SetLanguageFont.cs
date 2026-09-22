using System;
using TMPro;
using UnityEngine;

// Token: 0x020000FC RID: 252
public class SetLanguageFont : MonoBehaviour
{
	// Token: 0x060006B4 RID: 1716 RVA: 0x0003B464 File Offset: 0x00039864
	private void Start()
	{
		this.Text = base.GetComponent<TextMeshProUGUI>();
	}

	// Token: 0x060006B5 RID: 1717 RVA: 0x0003B472 File Offset: 0x00039872
	private void Update()
	{
		if (this.Text.text != this.lastText)
		{
			this.UpdateFont();
			this.lastText = this.Text.text;
		}
	}

	// Token: 0x060006B6 RID: 1718 RVA: 0x0003B4A8 File Offset: 0x000398A8
	private void UpdateFont()
	{
		foreach (SetLanguageFont.TextFont textFont in this.Fonts)
		{
			if (this.Text.text == textFont.Text && this.Text.font != textFont.Font)
			{
				this.Text.font = textFont.Font;
				break;
			}
		}
	}

	// Token: 0x040005FA RID: 1530
	public SetLanguageFont.TextFont[] Fonts;

	// Token: 0x040005FB RID: 1531
	private TextMeshProUGUI Text;

	// Token: 0x040005FC RID: 1532
	private string lastText = string.Empty;

	// Token: 0x020000FD RID: 253
	[Serializable]
	public struct TextFont
	{
		// Token: 0x040005FD RID: 1533
		public string Text;

		// Token: 0x040005FE RID: 1534
		public TMP_FontAsset Font;
	}
}
