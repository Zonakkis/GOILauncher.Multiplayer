using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x02000100 RID: 256
public class CheatDragControl : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	// Token: 0x060006C1 RID: 1729 RVA: 0x0003B97E File Offset: 0x00039D7E
	private void OnEnable()
	{
		this.tapCount = 0;
	}

	// Token: 0x060006C2 RID: 1730 RVA: 0x0003B988 File Offset: 0x00039D88
	public void OnPointerClick(PointerEventData eventData)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		this.tapCount++;
		if (this.tapCount >= 10)
		{
			this.tapCount = 0;
			this.cheatDragRuntime.gameObject.SetActive(!this.cheatDragRuntime.gameObject.activeSelf);
			Debug.LogFormat("[NoodleDebug] Drag cheat is {0}", new object[] { (!this.cheatDragRuntime.gameObject.activeSelf) ? "DEACTIVATED" : "ACTIVATED" });
			base.StartCoroutine(this.IndicateActivated());
		}
	}

	// Token: 0x060006C3 RID: 1731 RVA: 0x0003BA2C File Offset: 0x00039E2C
	private IEnumerator IndicateActivated()
	{
		Vector3 s = base.transform.localScale;
		base.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
		yield return new WaitForSecondsRealtime(0.5f);
		base.transform.localScale = s;
		yield break;
	}

	// Token: 0x0400060C RID: 1548
	public CheatDragRuntime cheatDragRuntime;

	// Token: 0x0400060D RID: 1549
	private int tapCount;
}
