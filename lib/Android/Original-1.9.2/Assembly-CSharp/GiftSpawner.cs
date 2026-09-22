using System;
using UnityEngine;

// Token: 0x02000125 RID: 293
public class GiftSpawner : MonoBehaviour
{
	// Token: 0x06000784 RID: 1924 RVA: 0x0003F698 File Offset: 0x0003DA98
	private void Start()
	{
		this.spawned = false;
		this.timeInTrigger = 0f;
	}

	// Token: 0x06000785 RID: 1925 RVA: 0x0003F6AC File Offset: 0x0003DAAC
	private void Update()
	{
	}

	// Token: 0x06000786 RID: 1926 RVA: 0x0003F6B0 File Offset: 0x0003DAB0
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

	// Token: 0x040006B1 RID: 1713
	private float timeInTrigger;

	// Token: 0x040006B2 RID: 1714
	private bool spawned;

	// Token: 0x040006B3 RID: 1715
	public GameObject gift;

	// Token: 0x040006B4 RID: 1716
	public Transform player;

	// Token: 0x040006B5 RID: 1717
	public Transform spawn1;

	// Token: 0x040006B6 RID: 1718
	public Transform spawn2;
}
