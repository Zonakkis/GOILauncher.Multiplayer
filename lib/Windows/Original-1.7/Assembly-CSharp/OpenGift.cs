using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200003C RID: 60
public class OpenGift : MonoBehaviour
{
	// Token: 0x060001C8 RID: 456 RVA: 0x000117E5 File Offset: 0x0000F9E5
	private void Start()
	{
		this.done = false;
		this.batsSpawned = 0;
	}

	// Token: 0x060001C9 RID: 457 RVA: 0x000117F8 File Offset: 0x0000F9F8
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
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x060001CA RID: 458 RVA: 0x0001184D File Offset: 0x0000FA4D
	private IEnumerator Spawn()
	{
		while (this.batsSpawned < this.numBats)
		{
			for (int i = 0; i < 3; i++)
			{
				Object.Instantiate<GameObject>(this.batPrefab, base.transform.position + Random.insideUnitCircle, Quaternion.identity, base.transform).GetComponent<BatControl>().mainCam = this.batCam;
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

	// Token: 0x060001CB RID: 459 RVA: 0x0001185C File Offset: 0x0000FA5C
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

	// Token: 0x040002DE RID: 734
	public GameObject batPrefab;

	// Token: 0x040002DF RID: 735
	private bool done;

	// Token: 0x040002E0 RID: 736
	private int numBats = 150;

	// Token: 0x040002E1 RID: 737
	private int batsSpawned;

	// Token: 0x040002E2 RID: 738
	public AudioSource source;

	// Token: 0x040002E3 RID: 739
	public Camera batCam;
}
