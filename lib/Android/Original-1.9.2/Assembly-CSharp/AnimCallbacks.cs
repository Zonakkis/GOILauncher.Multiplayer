using System;
using UnityEngine;

// Token: 0x02000117 RID: 279
public class AnimCallbacks : MonoBehaviour
{
	// Token: 0x06000750 RID: 1872 RVA: 0x0003E700 File Offset: 0x0003CB00
	private void Start()
	{
	}

	// Token: 0x06000751 RID: 1873 RVA: 0x0003E702 File Offset: 0x0003CB02
	private void Update()
	{
	}

	// Token: 0x06000752 RID: 1874 RVA: 0x0003E704 File Offset: 0x0003CB04
	public void EndIntroAnim()
	{
		this.loader.SendMessage("EndIntroAnim", SendMessageOptions.DontRequireReceiver);
		this.rocks.Play();
	}

	// Token: 0x0400067A RID: 1658
	public GameObject loader;

	// Token: 0x0400067B RID: 1659
	public ParticleSystem rocks;
}
