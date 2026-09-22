using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000134 RID: 308
public class OpenGift : MonoBehaviour
{
	// Token: 0x060007DA RID: 2010 RVA: 0x00043A51 File Offset: 0x00041E51
	private void Start()
	{
		this.done = false;
		this.batsSpawned = 0;
	}

	// Token: 0x060007DB RID: 2011 RVA: 0x00043A64 File Offset: 0x00041E64
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

	// Token: 0x060007DC RID: 2012 RVA: 0x00043AC0 File Offset: 0x00041EC0
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
		MeshRenderer[] rens = base.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in rens)
		{
			meshRenderer.enabled = false;
		}
		yield break;
	}

	// Token: 0x060007DD RID: 2013 RVA: 0x00043ADC File Offset: 0x00041EDC
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

	// Token: 0x0400075F RID: 1887
	public GameObject batPrefab;

	// Token: 0x04000760 RID: 1888
	private bool done;

	// Token: 0x04000761 RID: 1889
	private int numBats = 150;

	// Token: 0x04000762 RID: 1890
	private int batsSpawned;

	// Token: 0x04000763 RID: 1891
	public AudioSource source;

	// Token: 0x04000764 RID: 1892
	public Camera batCam;
}
