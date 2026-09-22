using System;
using UnityEngine;

// Token: 0x02000118 RID: 280
public class BatControl : MonoBehaviour
{
	// Token: 0x06000754 RID: 1876 RVA: 0x0003E735 File Offset: 0x0003CB35
	private void Start()
	{
		base.transform.rotation = Quaternion.Euler(global::UnityEngine.Random.Range(-25f, 25f), global::UnityEngine.Random.Range(-25f, 25f), global::UnityEngine.Random.Range(-25f, 25f));
	}

	// Token: 0x06000755 RID: 1877 RVA: 0x0003E774 File Offset: 0x0003CB74
	private void Update()
	{
		base.transform.position += base.transform.forward * -this.speed;
		if (base.transform.position.z < this.mainCam.transform.position.z)
		{
			global::UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x0400067C RID: 1660
	public Camera mainCam;

	// Token: 0x0400067D RID: 1661
	private float speed = 3f;
}
