using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200011F RID: 287
public class DebugSpawn : MonoBehaviour
{
	// Token: 0x06000770 RID: 1904 RVA: 0x0003F05C File Offset: 0x0003D45C
	private void Start()
	{
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

	// Token: 0x06000771 RID: 1905 RVA: 0x0003F10F File Offset: 0x0003D50F
	public void GoLeft()
	{
		this.timeToGo = true;
		this.currentSpawner = (this.spawners.Count + this.currentSpawner - 1) % this.spawners.Count;
	}

	// Token: 0x06000772 RID: 1906 RVA: 0x0003F13E File Offset: 0x0003D53E
	public void GoRight()
	{
		this.timeToGo = true;
		this.currentSpawner = (this.currentSpawner + 1) % this.spawners.Count;
	}

	// Token: 0x06000773 RID: 1907 RVA: 0x0003F164 File Offset: 0x0003D564
	private void Update()
	{
		if (!Application.isEditor)
		{
			return;
		}
		if (this.currentSpawner != this.prevSpawner && this.timeToGo)
		{
			this.player.transform.position = this.spawners[this.currentSpawner].position;
			this.timeToGo = false;
		}
		this.prevSpawner = this.currentSpawner;
		if (Application.isEditor)
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
	}

	// Token: 0x06000774 RID: 1908 RVA: 0x0003F20A File Offset: 0x0003D60A
	private static int CompareTransform(Transform A, Transform B)
	{
		return A.name.CompareTo(B.name);
	}

	// Token: 0x0400069A RID: 1690
	private List<Transform> spawners;

	// Token: 0x0400069B RID: 1691
	private int currentSpawner;

	// Token: 0x0400069C RID: 1692
	public GameObject player;

	// Token: 0x0400069D RID: 1693
	private int prevSpawner;

	// Token: 0x0400069E RID: 1694
	private bool timeToGo;
}
