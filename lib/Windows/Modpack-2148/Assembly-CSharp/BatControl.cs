using System;
using UnityEngine;

// Token: 0x02000039 RID: 57
public class BatControl : MonoBehaviour
{
	// Token: 0x06000160 RID: 352 RVA: 0x0000341A File Offset: 0x0000161A
	private void Start()
	{
		base.transform.rotation = Quaternion.Euler(global::UnityEngine.Random.Range(-25f, 25f), global::UnityEngine.Random.Range(-25f, 25f), global::UnityEngine.Random.Range(-25f, 25f));
	}

	// Token: 0x06000161 RID: 353 RVA: 0x0001E5E0 File Offset: 0x0001C7E0
	private void Update()
	{
		base.transform.position += base.transform.forward * -this.speed;
		if (base.transform.position.z < this.mainCam.transform.position.z)
		{
			global::UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x04000253 RID: 595
	public Camera mainCam;

	// Token: 0x04000254 RID: 596
	private float speed = 2.5f;
}
