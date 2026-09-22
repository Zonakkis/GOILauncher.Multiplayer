using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002D RID: 45
public class DebugSpawn : MonoBehaviour
{
	// Token: 0x06000165 RID: 357 RVA: 0x0000DD40 File Offset: 0x0000BF40
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

	// Token: 0x06000166 RID: 358 RVA: 0x0000DDE9 File Offset: 0x0000BFE9
	public void GoLeft()
	{
		this.currentSpawner = (this.spawners.Count + this.currentSpawner - 1) % this.spawners.Count;
	}

	// Token: 0x06000167 RID: 359 RVA: 0x0000DE11 File Offset: 0x0000C011
	public void GoRight()
	{
		this.currentSpawner = (this.currentSpawner + 1) % this.spawners.Count;
	}

	// Token: 0x06000168 RID: 360 RVA: 0x0000DE30 File Offset: 0x0000C030
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

	// Token: 0x06000169 RID: 361 RVA: 0x0000DEB8 File Offset: 0x0000C0B8
	private static int CompareTransform(Transform A, Transform B)
	{
		return A.name.CompareTo(B.name);
	}

	// Token: 0x0400022D RID: 557
	private List<Transform> spawners;

	// Token: 0x0400022E RID: 558
	private int currentSpawner;

	// Token: 0x0400022F RID: 559
	public GameObject player;

	// Token: 0x04000230 RID: 560
	private bool cheat;
}
