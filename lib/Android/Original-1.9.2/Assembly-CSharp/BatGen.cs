using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000119 RID: 281
public class BatGen : MonoBehaviour
{
	// Token: 0x06000757 RID: 1879 RVA: 0x0003E7FC File Offset: 0x0003CBFC
	private void Start()
	{
		this.done = false;
		this.batsSpawned = 0;
		this.source = base.GetComponent<AudioSource>();
	}

	// Token: 0x06000758 RID: 1880 RVA: 0x0003E818 File Offset: 0x0003CC18
	private void Update()
	{
		if (!this.done)
		{
			return;
		}
		if (base.transform.childCount <= 1)
		{
			this.batCam.gameObject.SetActive(false);
			global::UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000759 RID: 1881 RVA: 0x0003E854 File Offset: 0x0003CC54
	private IEnumerator Spawn()
	{
		while (this.batsSpawned < this.numBats)
		{
			for (int i = 0; i < 3; i++)
			{
				GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(this.batPrefab, base.transform.position + global::UnityEngine.Random.insideUnitCircle, Quaternion.identity, base.transform);
				gameObject.GetComponent<BatControl>().mainCam = this.batCam;
				this.batsSpawned++;
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x0600075A RID: 1882 RVA: 0x0003E870 File Offset: 0x0003CC70
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (this.done)
		{
			return;
		}
		if (coll.name != "PotCollider")
		{
			return;
		}
		this.done = true;
		this.batCam.gameObject.SetActive(true);
		base.StartCoroutine("Spawn");
		int @int = PlayerPrefs.GetInt("NumWins");
		bool flag = @int <= 0;
		if (flag)
		{
			this.source.Play();
		}
	}

	// Token: 0x0400067E RID: 1662
	public GameObject batPrefab;

	// Token: 0x0400067F RID: 1663
	private bool done;

	// Token: 0x04000680 RID: 1664
	private int numBats = 210;

	// Token: 0x04000681 RID: 1665
	private int batsSpawned;

	// Token: 0x04000682 RID: 1666
	private AudioSource source;

	// Token: 0x04000683 RID: 1667
	public Camera batCam;
}
