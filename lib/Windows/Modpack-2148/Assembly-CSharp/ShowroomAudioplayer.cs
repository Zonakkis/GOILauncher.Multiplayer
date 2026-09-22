using System;
using UnityEngine;

// Token: 0x02000005 RID: 5
[ExecuteInEditMode]
public class ShowroomAudioplayer : MonoBehaviour
{
	// Token: 0x06000016 RID: 22 RVA: 0x000145B4 File Offset: 0x000127B4
	private void OnEnable()
	{
		this.ShowroomAudio = base.GetComponent<AudioSource>();
		float length = this.ShowroomAudio.clip.length;
		float num = global::UnityEngine.Random.Range(2f, length);
		this.ShowroomAudio.time = num;
	}

	// Token: 0x06000017 RID: 23 RVA: 0x0000265E File Offset: 0x0000085E
	private void Update()
	{
	}

	// Token: 0x0400000C RID: 12
	public AudioSource ShowroomAudio;
}
