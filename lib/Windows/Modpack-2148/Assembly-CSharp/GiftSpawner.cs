using System;
using UnityEngine;

// Token: 0x02000047 RID: 71
public class GiftSpawner : MonoBehaviour
{
	// Token: 0x060001A0 RID: 416 RVA: 0x000036E9 File Offset: 0x000018E9
	private void Start()
	{
		this.spawned = false;
		this.timeInTrigger = 0f;
	}

	// Token: 0x060001A1 RID: 417 RVA: 0x0000265E File Offset: 0x0000085E
	private void Update()
	{
	}

	// Token: 0x060001A2 RID: 418 RVA: 0x0001F54C File Offset: 0x0001D74C
	private void OnTriggerStay2D(Collider2D coll)
	{
		if (coll.name != "PotCollider")
		{
			return;
		}
		if (this.spawned)
		{
			return;
		}
		this.timeInTrigger += Time.fixedDeltaTime;
		if (this.timeInTrigger > 480f)
		{
			if (this.player.position.x < base.transform.position.x)
			{
				this.gift.transform.position = this.spawn1.position;
				this.gift.SetActive(true);
			}
			else
			{
				this.gift.transform.position = this.spawn2.position;
				this.gift.SetActive(true);
			}
			this.spawned = true;
		}
	}

	// Token: 0x0400029A RID: 666
	private float timeInTrigger;

	// Token: 0x0400029B RID: 667
	private bool spawned;

	// Token: 0x0400029C RID: 668
	public GameObject gift;

	// Token: 0x0400029D RID: 669
	public Transform player;

	// Token: 0x0400029E RID: 670
	public Transform spawn1;

	// Token: 0x0400029F RID: 671
	public Transform spawn2;
}
