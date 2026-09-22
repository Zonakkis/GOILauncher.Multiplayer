using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200005F RID: 95
public class OpenGift : MonoBehaviour
{
	// Token: 0x06000238 RID: 568 RVA: 0x00003B85 File Offset: 0x00001D85
	private void Start()
	{
		this.done = false;
		this.batsSpawned = 0;
	}

	// Token: 0x06000239 RID: 569 RVA: 0x00023578 File Offset: 0x00021778
	private void Update()
	{
		if (!this.done)
		{
			return;
		}
		Debug.Log(base.transform.childCount);
		if (base.transform.childCount <= 2)
		{
			this.batCam.gameObject.SetActive(false);
			global::UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x0600023A RID: 570 RVA: 0x00003B95 File Offset: 0x00001D95
	private IEnumerator Spawn()
	{
		while (this.batsSpawned < this.numBats)
		{
			for (int i = 0; i < 3; i++)
			{
				global::UnityEngine.Object.Instantiate<GameObject>(this.batPrefab, base.transform.position + global::UnityEngine.Random.insideUnitCircle, Quaternion.identity, base.transform).GetComponent<BatControl>().mainCam = this.batCam;
				this.batsSpawned++;
			}
			yield return null;
		}
		MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].enabled = false;
		}
		yield break;
	}

	// Token: 0x0600023B RID: 571 RVA: 0x000235D0 File Offset: 0x000217D0
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.done)
		{
			return;
		}
		if (coll.collider.name != "Tip" && coll.collider.name != "PotCollider")
		{
			return;
		}
		this.done = true;
		this.batCam.gameObject.SetActive(true);
		base.StartCoroutine("Spawn");
		base.GetComponent<Rigidbody2D>().isKinematic = true;
		base.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		base.GetComponent<Rigidbody2D>().angularVelocity = 0f;
		this.source.Play();
	}

	// Token: 0x04000379 RID: 889
	public GameObject batPrefab;

	// Token: 0x0400037A RID: 890
	private bool done;

	// Token: 0x0400037B RID: 891
	private int numBats = 150;

	// Token: 0x0400037C RID: 892
	private int batsSpawned;

	// Token: 0x0400037D RID: 893
	public AudioSource source;

	// Token: 0x0400037E RID: 894
	public Camera batCam;
}
