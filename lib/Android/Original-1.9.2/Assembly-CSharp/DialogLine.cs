using System;
using UnityEngine;

// Token: 0x02000120 RID: 288
public class DialogLine : MonoBehaviour
{
	// Token: 0x06000776 RID: 1910 RVA: 0x0003F225 File Offset: 0x0003D625
	private void Start()
	{
	}

	// Token: 0x040006A0 RID: 1696
	[HideInInspector]
	public float splinePosition;

	// Token: 0x040006A1 RID: 1697
	[TextArea(8, 12)]
	public string subtitles;

	// Token: 0x040006A2 RID: 1698
	public AudioClip clip;
}
