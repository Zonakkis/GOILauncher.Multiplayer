using System;
using UnityEngine;

// Token: 0x0200014E RID: 334
public class Teleporter : MonoBehaviour
{
	// Token: 0x06000945 RID: 2373 RVA: 0x0004B6D0 File Offset: 0x00049AD0
	private void Start()
	{
		this.shouldEnable = false;
		this.shouldTeleport = false;
		this.enableTeleport = true;
	}

	// Token: 0x06000946 RID: 2374 RVA: 0x0004B6E7 File Offset: 0x00049AE7
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (!this.enableTeleport)
		{
			return;
		}
		if (coll.gameObject == this.player)
		{
			this.shouldTeleport = true;
		}
	}

	// Token: 0x06000947 RID: 2375 RVA: 0x0004B712 File Offset: 0x00049B12
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject == this.player)
		{
			this.shouldEnable = true;
		}
	}

	// Token: 0x06000948 RID: 2376 RVA: 0x0004B734 File Offset: 0x00049B34
	private void LateUpdate()
	{
		if (this.shouldEnable)
		{
			this.enableTeleport = true;
			this.shouldEnable = false;
		}
		if (this.shouldTeleport)
		{
			this.targetX.GetComponent<Teleporter>().enableTeleport = false;
			Vector3 vector = Camera.main.transform.position - this.player.transform.position;
			Vector3 vector2 = this.cursor.position - this.player.transform.position;
			this.player.transform.position = new Vector3(this.targetX.position.x, this.player.transform.position.y, this.player.transform.position.z);
			Camera.main.transform.position = this.player.transform.position + vector;
			this.cursor.position = this.player.transform.position + vector2;
			this.shouldTeleport = false;
			this.enableTeleport = false;
			this.shouldEnable = true;
		}
	}

	// Token: 0x040008D7 RID: 2263
	public Transform targetX;

	// Token: 0x040008D8 RID: 2264
	public GameObject player;

	// Token: 0x040008D9 RID: 2265
	public bool shouldTeleport;

	// Token: 0x040008DA RID: 2266
	public bool shouldEnable;

	// Token: 0x040008DB RID: 2267
	public bool enableTeleport;

	// Token: 0x040008DC RID: 2268
	public Transform cursor;
}
