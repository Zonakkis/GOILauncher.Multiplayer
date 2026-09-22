using System;
using UnityEngine;

// Token: 0x02000030 RID: 48
public class Rotator : MonoBehaviour
{
	// Token: 0x06000138 RID: 312 RVA: 0x0000265E File Offset: 0x0000085E
	private void Start()
	{
	}

	// Token: 0x06000139 RID: 313 RVA: 0x0001DE0C File Offset: 0x0001C00C
	private void FixedUpdate()
	{
		this.time += Time.deltaTime;
		base.transform.Rotate(this.Axis, this.SpinSpeed, Space.World);
		if (this.rotateSky && this.Skybox)
		{
			this.Skybox.SetFloat("_Rotation", -base.transform.eulerAngles.y);
		}
	}

	// Token: 0x04000229 RID: 553
	public float SpinSpeed = 1f;

	// Token: 0x0400022A RID: 554
	public Vector3 Axis = new Vector3(0f, 1f, 0f);

	// Token: 0x0400022B RID: 555
	[SerializeField]
	private Material Skybox;

	// Token: 0x0400022C RID: 556
	public bool rotateSky;

	// Token: 0x0400022D RID: 557
	private float time;
}
