using System;
using UnityEngine;

// Token: 0x02000024 RID: 36
public class AnimCallbacks : MonoBehaviour
{
	// Token: 0x0600013A RID: 314 RVA: 0x0000D19C File Offset: 0x0000B39C
	private void Start()
	{
	}

	// Token: 0x0600013B RID: 315 RVA: 0x0000D19E File Offset: 0x0000B39E
	private void Update()
	{
	}

	// Token: 0x0600013C RID: 316 RVA: 0x0000D1A0 File Offset: 0x0000B3A0
	public void EndIntroAnim()
	{
		this.loader.SendMessage("EndIntroAnim", SendMessageOptions.DontRequireReceiver);
		this.rocks.Play();
	}

	// Token: 0x040001FE RID: 510
	public GameObject loader;

	// Token: 0x040001FF RID: 511
	public ParticleSystem rocks;
}
