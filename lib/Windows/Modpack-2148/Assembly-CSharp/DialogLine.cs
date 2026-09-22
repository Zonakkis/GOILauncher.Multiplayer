using System;
using UnityEngine;

// Token: 0x02000043 RID: 67
public class DialogLine : MonoBehaviour
{
	// Token: 0x06000195 RID: 405 RVA: 0x0000265E File Offset: 0x0000085E
	[HideInInspector]
	private void Start()
	{
	}

	// Token: 0x04000287 RID: 647
	[HideInInspector]
	public float splinePosition;

	// Token: 0x04000288 RID: 648
	[TextArea(8, 12)]
	public string subtitles;

	// Token: 0x04000289 RID: 649
	public AudioClip clip;
}
