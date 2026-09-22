using System;
using UnityEngine;

// Token: 0x02000025 RID: 37
public class BatControl : MonoBehaviour
{
	// Token: 0x0600013E RID: 318 RVA: 0x0000D1C6 File Offset: 0x0000B3C6
	private void Start()
	{
		base.transform.rotation = Quaternion.Euler(Random.Range(-25f, 25f), Random.Range(-25f, 25f), Random.Range(-25f, 25f));
	}

	// Token: 0x0600013F RID: 319 RVA: 0x0000D208 File Offset: 0x0000B408
	private void Update()
	{
		base.transform.position += base.transform.forward * -this.speed;
		if (base.transform.position.z < this.mainCam.transform.position.z)
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x04000200 RID: 512
	public Camera mainCam;

	// Token: 0x04000201 RID: 513
	private float speed = 2.5f;
}
