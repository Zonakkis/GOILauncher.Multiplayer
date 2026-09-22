using System;
using UnityEngine;

// Token: 0x02000081 RID: 129
public class Teleporter : MonoBehaviour
{
	// Token: 0x0600037E RID: 894 RVA: 0x00004603 File Offset: 0x00002803
	private void Start()
	{
		this.shouldEnable = false;
		this.shouldTeleport = false;
		this.enableTeleport = true;
	}

	// Token: 0x0600037F RID: 895 RVA: 0x0000461A File Offset: 0x0000281A
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

	// Token: 0x06000380 RID: 896 RVA: 0x0000463F File Offset: 0x0000283F
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject == this.player)
		{
			this.shouldEnable = true;
		}
	}

	// Token: 0x06000381 RID: 897 RVA: 0x00032444 File Offset: 0x00030644
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

	// Token: 0x04000551 RID: 1361
	public Transform targetX;

	// Token: 0x04000552 RID: 1362
	public GameObject player;

	// Token: 0x04000553 RID: 1363
	public bool shouldTeleport;

	// Token: 0x04000554 RID: 1364
	public bool shouldEnable;

	// Token: 0x04000555 RID: 1365
	public bool enableTeleport;

	// Token: 0x04000556 RID: 1366
	public Transform cursor;
}
