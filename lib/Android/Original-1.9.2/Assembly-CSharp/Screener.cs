using System;
using UnityEngine;

// Token: 0x02000142 RID: 322
public class Screener : MonoBehaviour
{
	// Token: 0x0600087A RID: 2170 RVA: 0x00049378 File Offset: 0x00047778
	private void Start()
	{
	}

	// Token: 0x0600087B RID: 2171 RVA: 0x0004937C File Offset: 0x0004777C
	private void Update()
	{
		if (!Application.isEditor)
		{
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			Debug.Log("trying to capture screen");
			ScreenCapture.CaptureScreenshot("gettingitscreen" + this.num);
			this.num++;
		}
	}

	// Token: 0x04000879 RID: 2169
	private int num;
}
