using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// Token: 0x02000048 RID: 72
public class GravityControl : MonoBehaviour
{
	// Token: 0x060001A4 RID: 420 RVA: 0x000036FD File Offset: 0x000018FD
	private void Start()
	{
		Physics2D.gravity = new Vector2(0f, -30f);
		this.creditsUp = false;
	}

	// Token: 0x060001A5 RID: 421 RVA: 0x0000265E File Offset: 0x0000085E
	private void FixedUpdate()
	{
	}

	// Token: 0x060001A6 RID: 422 RVA: 0x0001F610 File Offset: 0x0001D810
	private void OnTriggerStay2D(Collider2D coll)
	{
		if (coll.gameObject.name != "Player" && coll.gameObject.name != "Tip" && coll.gameObject.name != "Sides" && coll.gameObject.name != "PotCollider")
		{
			return;
		}
		if (coll.attachedRigidbody == null)
		{
			return;
		}
		if (!this.creditsUp)
		{
			foreach (Transform transform in this.gravityWells)
			{
				this.gvec = transform.position - coll.attachedRigidbody.position;
				coll.attachedRigidbody.AddForce(2500f / this.gvec.sqrMagnitude * this.gvec.normalized);
			}
		}
	}

	// Token: 0x060001A7 RID: 423 RVA: 0x0001F6F8 File Offset: 0x0001D8F8
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.name != "Player" && coll.gameObject.name != "Tip" && coll.gameObject.name != "Sides" && coll.gameObject.name != "PotCollider")
		{
			return;
		}
		Physics2D.gravity = new Vector2(0f, 0f);
	}

	// Token: 0x060001A8 RID: 424 RVA: 0x0001F778 File Offset: 0x0001D978
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject.name != "Player" && coll.gameObject.name != "Tip" && coll.gameObject.name != "Sides" && coll.gameObject.name != "PotCollider")
		{
			return;
		}
		if (coll.attachedRigidbody == null)
		{
			return;
		}
		if (coll.attachedRigidbody.position.y > base.GetComponent<BoxCollider2D>().bounds.max.y - 5f)
		{
			if (!this.creditsUp)
			{
				SettingsManager.timer.FinishRun();
				Physics2D.gravity = new Vector2(0f, 1.2f);
				global::UnityEngine.Object.Instantiate<GameObject>(this.creditsPrefab, this.creditsParent);
				this.starNest.SetActive(true);
				this.starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", 0f);
				base.StartCoroutine("FadeUpStarNest");
				this.creditsUp = true;
				SettingsManager.reachedSpace = true;
				this.fgCam.GetComponent<PostProcessVolume>().profile = this.blurOffProfile;
				this.progressMeter.Pause(true);
				this.settingsMenu.canMenu = false;
				PlayerPrefs.DeleteKey("NumSaves");
				PlayerPrefs.DeleteKey("SaveGame0");
				PlayerPrefs.DeleteKey("SaveGame1");
				PlayerPrefs.Save();
				return;
			}
		}
		else
		{
			global::UnityEngine.Object.FindObjectOfType<SettingsManager>().reapplyGravity();
			SettingsManager.reachedSpace = false;
		}
	}

	// Token: 0x060001A9 RID: 425 RVA: 0x0000371A File Offset: 0x0000191A
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

	// Token: 0x040002A0 RID: 672
	public Transform[] gravityWells;

	// Token: 0x040002A1 RID: 673
	private Vector2 gvec;

	// Token: 0x040002A2 RID: 674
	public GameObject creditsPrefab;

	// Token: 0x040002A3 RID: 675
	public Transform creditsParent;

	// Token: 0x040002A4 RID: 676
	private bool creditsUp;

	// Token: 0x040002A5 RID: 677
	public ProgressMeter progressMeter;

	// Token: 0x040002A6 RID: 678
	public GameObject starNest;

	// Token: 0x040002A7 RID: 679
	public PostProcessProfile blurOffProfile;

	// Token: 0x040002A8 RID: 680
	public SettingsManager settingsMenu;

	// Token: 0x040002A9 RID: 681
	public Camera fgCam;
}
