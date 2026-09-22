using System;
using UnityEngine;

// Token: 0x02000032 RID: 50
public class GiftSpawner : MonoBehaviour
{
	// Token: 0x06000176 RID: 374 RVA: 0x0000E36C File Offset: 0x0000C56C
	private void Start()
	{
		this.spawned = false;
		this.timeInTrigger = 0f;
	}

	// Token: 0x06000177 RID: 375 RVA: 0x0000E380 File Offset: 0x0000C580
	private void Update()
	{
	}

	// Token: 0x06000178 RID: 376 RVA: 0x0000E384 File Offset: 0x0000C584
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

	// Token: 0x04000244 RID: 580
	private float timeInTrigger;

	// Token: 0x04000245 RID: 581
	private bool spawned;

	// Token: 0x04000246 RID: 582
	public GameObject gift;

	// Token: 0x04000247 RID: 583
	public Transform player;

	// Token: 0x04000248 RID: 584
	public Transform spawn1;

	// Token: 0x04000249 RID: 585
	public Transform spawn2;
}
