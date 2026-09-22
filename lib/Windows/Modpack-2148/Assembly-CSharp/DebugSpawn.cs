using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000042 RID: 66
public class DebugSpawn : MonoBehaviour
{
	// Token: 0x0600018F RID: 399 RVA: 0x0001EFC0 File Offset: 0x0001D1C0
	private void Start()
	{
		if (!Application.isEditor && !this.cheat)
		{
			return;
		}
		this.spawners = new List<Transform>(base.transform.GetComponentsInChildren<Transform>());
		this.spawners.Sort(new Comparison<Transform>(DebugSpawn.CompareTransform));
		this.currentSpawner = 0;
		float num = 9999f;
		for (int i = 0; i < this.spawners.Count; i++)
		{
			float sqrMagnitude = (this.player.transform.position - this.spawners[i].position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				this.currentSpawner = i;
			}
		}
	}

	// Token: 0x06000190 RID: 400 RVA: 0x0000367A File Offset: 0x0000187A
	public void GoLeft()
	{
		this.currentSpawner = (this.spawners.Count + this.currentSpawner - 1) % this.spawners.Count;
	}

	// Token: 0x06000191 RID: 401 RVA: 0x000036A2 File Offset: 0x000018A2
	public void GoRight()
	{
		this.currentSpawner = (this.currentSpawner + 1) % this.spawners.Count;
	}

	// Token: 0x06000192 RID: 402 RVA: 0x0001F06C File Offset: 0x0001D26C
	private void Update()
	{
		if (!Application.isEditor && !this.cheat)
		{
			return;
		}
		int num = this.currentSpawner;
		if (Application.isEditor || this.cheat)
		{
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				this.GoLeft();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				this.GoRight();
			}
		}
		if (this.currentSpawner != num)
		{
			this.player.transform.position = this.spawners[this.currentSpawner].position;
		}
	}

	// Token: 0x06000193 RID: 403 RVA: 0x000036BE File Offset: 0x000018BE
	private static int CompareTransform(Transform A, Transform B)
	{
		return A.name.CompareTo(B.name);
	}

	// Token: 0x04000283 RID: 643
	private List<Transform> spawners;

	// Token: 0x04000284 RID: 644
	private int currentSpawner;

	// Token: 0x04000285 RID: 645
	public GameObject player;

	// Token: 0x04000286 RID: 646
	private bool cheat;
}
