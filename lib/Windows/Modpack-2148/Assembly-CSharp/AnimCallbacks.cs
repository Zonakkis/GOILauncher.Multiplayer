using System;
using UnityEngine;

// Token: 0x02000038 RID: 56
public class AnimCallbacks : MonoBehaviour
{
	// Token: 0x0600015C RID: 348 RVA: 0x0000265E File Offset: 0x0000085E
	private void Start()
	{
	}

	// Token: 0x0600015D RID: 349 RVA: 0x0000265E File Offset: 0x0000085E
	private void Update()
	{
	}

	// Token: 0x0600015E RID: 350 RVA: 0x000033FC File Offset: 0x000015FC
	public void EndIntroAnim()
	{
		this.loader.SendMessage("EndIntroAnim", SendMessageOptions.DontRequireReceiver);
		this.rocks.Play();
	}

	// Token: 0x04000251 RID: 593
	public GameObject loader;

	// Token: 0x04000252 RID: 594
	public ParticleSystem rocks;
}
