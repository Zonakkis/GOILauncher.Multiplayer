using System;
using UnityEngine;

// Token: 0x0200001D RID: 29
public class Rotator : MonoBehaviour
{
	// Token: 0x06000116 RID: 278 RVA: 0x0000C755 File Offset: 0x0000A955
	private void Start()
	{
	}

	// Token: 0x06000117 RID: 279 RVA: 0x0000C758 File Offset: 0x0000A958
	private void FixedUpdate()
	{
		this.time += Time.deltaTime;
		base.transform.Rotate(this.Axis, this.SpinSpeed, Space.World);
		if (this.rotateSky && this.Skybox)
		{
			this.Skybox.SetFloat("_Rotation", -base.transform.eulerAngles.y);
		}
	}

	// Token: 0x040001DC RID: 476
	public float SpinSpeed = 1f;

	// Token: 0x040001DD RID: 477
	public Vector3 Axis = new Vector3(0f, 1f, 0f);

	// Token: 0x040001DE RID: 478
	[SerializeField]
	private Material Skybox;

	// Token: 0x040001DF RID: 479
	public bool rotateSky;

	// Token: 0x040001E0 RID: 480
	private float time;
}
