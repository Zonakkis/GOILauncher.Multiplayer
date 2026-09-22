using System;
using UnityEngine;

// Token: 0x02000005 RID: 5
[ExecuteInEditMode]
public class ShowroomAudioplayer : MonoBehaviour
{
	// Token: 0x06000016 RID: 22 RVA: 0x000025F4 File Offset: 0x000007F4
	private void OnEnable()
	{
		this.ShowroomAudio = base.GetComponent<AudioSource>();
		float length = this.ShowroomAudio.clip.length;
		float num = Random.Range(2f, length);
		this.ShowroomAudio.time = num;
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00002636 File Offset: 0x00000836
	private void Update()
	{
	}

	// Token: 0x0400000C RID: 12
	public AudioSource ShowroomAudio;
}
