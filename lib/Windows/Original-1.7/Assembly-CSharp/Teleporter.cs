using System;
using UnityEngine;

// Token: 0x02000054 RID: 84
public class Teleporter : MonoBehaviour
{
	// Token: 0x06000298 RID: 664 RVA: 0x00018782 File Offset: 0x00016982
	private void Start()
	{
		this.shouldEnable = false;
		this.shouldTeleport = false;
		this.enableTeleport = true;
	}

	// Token: 0x06000299 RID: 665 RVA: 0x00018799 File Offset: 0x00016999
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

	// Token: 0x0600029A RID: 666 RVA: 0x000187BE File Offset: 0x000169BE
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject == this.player)
		{
			this.shouldEnable = true;
		}
	}

	// Token: 0x0600029B RID: 667 RVA: 0x000187DC File Offset: 0x000169DC
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

	// Token: 0x04000440 RID: 1088
	public Transform targetX;

	// Token: 0x04000441 RID: 1089
	public GameObject player;

	// Token: 0x04000442 RID: 1090
	public bool shouldTeleport;

	// Token: 0x04000443 RID: 1091
	public bool shouldEnable;

	// Token: 0x04000444 RID: 1092
	public bool enableTeleport;

	// Token: 0x04000445 RID: 1093
	public Transform cursor;
}
