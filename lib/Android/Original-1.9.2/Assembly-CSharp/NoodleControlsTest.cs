using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200010B RID: 267
public class NoodleControlsTest : MonoBehaviour
{
	// Token: 0x060006F4 RID: 1780 RVA: 0x0003D678 File Offset: 0x0003BA78
	private void Start()
	{
		Debug.Log("[NewScene] dpi1 = " + Screen.dpi);
		Screen.SetResolution(Screen.width / 4, Screen.height / 4, true, 60);
		Debug.Log("[NewScene] dpi2 = " + Screen.dpi);
	}

	// Token: 0x060006F5 RID: 1781 RVA: 0x0003D6D0 File Offset: 0x0003BAD0
	private void Update()
	{
		if (Input.touchCount > 0)
		{
			Vector2 position = Input.GetTouch(0).position;
			this.label.text = string.Format("{0} x {1}", position.x, position.y);
		}
		else
		{
			this.label.text = "no touch";
		}
		this.dpiLabel.text = Screen.dpi.ToString();
	}

	// Token: 0x04000656 RID: 1622
	public Text label;

	// Token: 0x04000657 RID: 1623
	public Text dpiLabel;
}
