using System;
using UnityEngine;

// Token: 0x02000028 RID: 40
public class Rotator : MonoBehaviour
{
	// Token: 0x0600010B RID: 267 RVA: 0x0000C4B4 File Offset: 0x0000A8B4
	private void Start()
	{
	}

	// Token: 0x0600010C RID: 268 RVA: 0x0000C4B8 File Offset: 0x0000A8B8
	private void FixedUpdate()
	{
		this.time += Time.deltaTime;
		base.transform.Rotate(this.Axis, this.SpinSpeed, Space.World);
		if (this.rotateSky && this.Skybox)
		{
			this.Skybox.SetFloat("_Rotation", -base.transform.eulerAngles.y);
		}
	}

	// Token: 0x040001FA RID: 506
	public float SpinSpeed = 1f;

	// Token: 0x040001FB RID: 507
	public Vector3 Axis = new Vector3(0f, 1f, 0f);

	// Token: 0x040001FC RID: 508
	[SerializeField]
	private Material Skybox;

	// Token: 0x040001FD RID: 509
	public bool rotateSky;

	// Token: 0x040001FE RID: 510
	private float time;
}
