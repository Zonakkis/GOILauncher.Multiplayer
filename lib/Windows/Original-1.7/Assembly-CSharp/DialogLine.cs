using System;
using UnityEngine;

// Token: 0x0200002E RID: 46
public class DialogLine : MonoBehaviour
{
	// Token: 0x0600016B RID: 363 RVA: 0x0000DED3 File Offset: 0x0000C0D3
	[HideInInspector]
	private void Start()
	{
	}

	// Token: 0x04000231 RID: 561
	[HideInInspector]
	public float splinePosition;

	// Token: 0x04000232 RID: 562
	[TextArea(8, 12)]
	public string subtitles;

	// Token: 0x04000233 RID: 563
	public AudioClip clip;
}
