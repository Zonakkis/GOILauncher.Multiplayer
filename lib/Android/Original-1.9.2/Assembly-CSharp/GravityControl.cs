using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000126 RID: 294
public class GravityControl : MonoBehaviour
{
	// Token: 0x06000788 RID: 1928 RVA: 0x0003F790 File Offset: 0x0003DB90
	private void Start()
	{
		Physics2D.gravity = new Vector2(0f, -30f);
		this.creditsUp = false;
		GameObject[] array = GameObject.FindGameObjectsWithTag("GravityWell");
		this.gravityWells = new Transform[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			this.gravityWells[i] = array[i].transform;
		}
		if (this.narrator == null)
		{
			this.narrator = GameObject.FindGameObjectWithTag("Narrator").GetComponent<Narrator>();
		}
	}

	// Token: 0x06000789 RID: 1929 RVA: 0x0003F81C File Offset: 0x0003DC1C
	private void FixedUpdate()
	{
		if (Physics2D.gravity.y != 0f)
		{
			return;
		}
	}

	// Token: 0x0600078A RID: 1930 RVA: 0x0003F844 File Offset: 0x0003DC44
	private void OnTriggerStay2D(Collider2D coll)
	{
		if (!this.creditsUp)
		{
			foreach (Transform transform in this.gravityWells)
			{
				if (coll.attachedRigidbody != null)
				{
					this.gvec = transform.position - coll.attachedRigidbody.position;
					coll.attachedRigidbody.AddForce(2500f / this.gvec.sqrMagnitude * this.gvec.normalized);
				}
			}
		}
	}

	// Token: 0x0600078B RID: 1931 RVA: 0x0003F8D9 File Offset: 0x0003DCD9
	private void OnTriggerEnter2D(Collider2D coll)
	{
		Physics2D.gravity = new Vector2(0f, 0f);
	}

	// Token: 0x0600078C RID: 1932 RVA: 0x0003F8F0 File Offset: 0x0003DCF0
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.attachedRigidbody.position.y > base.GetComponent<BoxCollider2D>().bounds.max.y - 5f)
		{
			if (!this.creditsUp)
			{
				Physics2D.gravity = new Vector2(0f, 1.2f);
				GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(this.creditsPrefab, this.creditsParent);
				gameObject.transform.SetAsFirstSibling();
				this.starNest.SetActive(true);
				this.starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", 0f);
				base.StartCoroutine("FadeUpStarNest");
				this.creditsUp = true;
				this.progressMeter.Pause(true);
				PlayerPrefs.DeleteKey("NumSaves");
				PlayerPrefs.DeleteKey("SaveGame0");
				PlayerPrefs.DeleteKey("SaveGame1");
				PlayerPrefs.SetFloat("LastTime", this.narrator.timePlayedThisGame);
				int num = PlayerPrefs.GetInt("NumWins");
				num++;
				PlayerPrefs.SetInt("NumWins", num);
				if (PlayerPrefs.HasKey("BestTime"))
				{
					float @float = PlayerPrefs.GetFloat("BestTime");
					if (this.narrator.timePlayedThisGame < @float)
					{
						PlayerPrefs.SetFloat("BestTime", this.narrator.timePlayedThisGame);
					}
				}
				else
				{
					PlayerPrefs.SetFloat("BestTime", this.narrator.timePlayedThisGame);
				}
				PlayerPrefs.Save();
				this.settingsMenu.EnableSkipCredits();
			}
		}
		else
		{
			Physics2D.gravity = new Vector2(0f, -30f);
		}
	}

	// Token: 0x0600078D RID: 1933 RVA: 0x0003FA94 File Offset: 0x0003DE94
	private IEnumerator FadeUpStarNest()
	{
		float step = 2.0000001E-05f;
		for (float f = 0f; f <= 0.01f; f += step)
		{
			this.starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", f);
			yield return null;
		}
		yield break;
	}

	// Token: 0x040006B7 RID: 1719
	public Transform[] gravityWells;

	// Token: 0x040006B8 RID: 1720
	private Vector2 gvec;

	// Token: 0x040006B9 RID: 1721
	public GameObject creditsPrefab;

	// Token: 0x040006BA RID: 1722
	public Transform creditsParent;

	// Token: 0x040006BB RID: 1723
	public bool creditsUp;

	// Token: 0x040006BC RID: 1724
	public ProgressMeter progressMeter;

	// Token: 0x040006BD RID: 1725
	public GameObject starNest;

	// Token: 0x040006BE RID: 1726
	public Narrator narrator;

	// Token: 0x040006BF RID: 1727
	public SettingsManager settingsMenu;

	// Token: 0x040006C0 RID: 1728
	public Camera fgCam;
}
