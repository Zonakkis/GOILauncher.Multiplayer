using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200004A RID: 74
public class Scroller : MonoBehaviour
{
	// Token: 0x0600025A RID: 602 RVA: 0x000163DC File Offset: 0x000145DC
	private void Start()
	{
		if (this.canvas == null)
		{
			this.canvas = base.transform.parent.GetComponentInParent<RectTransform>();
		}
		this.myRect = base.GetComponent<RectTransform>();
		this.fadeAlpha = 0f;
		this.aud = base.GetComponent<AudioSource>();
		if (SceneManager.GetActiveScene().name == "Mian")
		{
			this.aud.clip = this.endgameClip;
		}
		else
		{
			this.aud.clip = this.menuClip;
		}
		this.scrollDelta = 0.0071f;
		GameObject gameObject = GameObject.Find("Narrator");
		if (gameObject != null)
		{
			this.narrator = gameObject.GetComponent<Narrator>();
			PlayerPrefs.SetFloat("LastTime", this.narrator.timePlayedThisGame);
		}
	}

	// Token: 0x0600025B RID: 603 RVA: 0x000164B0 File Offset: 0x000146B0
	private void Update()
	{
		Vector3 position = base.transform.position;
		position.y += this.scrollDelta * 60f * Time.deltaTime;
		base.transform.position = position;
		if (this.myRect.TransformPoint(this.myRect.rect.min).y > this.canvas.TransformPoint(this.canvas.rect.max).y)
		{
			base.StartCoroutine("FadeOut");
		}
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
		{
			base.StartCoroutine("FadeOut");
		}
	}

	// Token: 0x0600025C RID: 604 RVA: 0x0001656E File Offset: 0x0001476E
	private IEnumerator FadeOut()
	{
		for (float f = 1f; f >= -0.001f; f -= 0.01f)
		{
			this.fadeAlpha = 1f - f;
			this.aud.volume = f;
			yield return null;
		}
		this.QuitToTitle();
		yield break;
	}

	// Token: 0x0600025D RID: 605 RVA: 0x00016580 File Offset: 0x00014780
	private void QuitToTitle()
	{
		this.aud.Stop();
		if (SceneManager.GetActiveScene().name == "Mian")
		{
			PlayerPrefs.DeleteKey("NumSaves");
			PlayerPrefs.DeleteKey("SaveGame0");
			PlayerPrefs.DeleteKey("SaveGame1");
			int num = PlayerPrefs.GetInt("NumWins");
			num++;
			PlayerPrefs.SetInt("NumWins", num);
			Debug.Log("Game Done, deleting saves");
			PlayerPrefs.Save();
			SceneManager.LoadScene("Reward Loader");
			return;
		}
		SceneManager.LoadScene("Loader");
	}

	// Token: 0x0600025E RID: 606 RVA: 0x00016610 File Offset: 0x00014810
	private void OnGUI()
	{
		if (this.fadeAlpha <= 0f)
		{
			return;
		}
		GUI.color = new Color(1f - Mathf.Pow(this.fadeAlpha, 0.25f), 0f, 0f, this.fadeAlpha);
		GUI.depth = -1000;
		GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.fadeTex);
	}

	// Token: 0x040003E7 RID: 999
	private RectTransform canvas;

	// Token: 0x040003E8 RID: 1000
	private RectTransform myRect;

	// Token: 0x040003E9 RID: 1001
	public bool inGame;

	// Token: 0x040003EA RID: 1002
	private float scrollDelta;

	// Token: 0x040003EB RID: 1003
	private float fadeAlpha;

	// Token: 0x040003EC RID: 1004
	private AudioSource aud;

	// Token: 0x040003ED RID: 1005
	public Texture2D fadeTex;

	// Token: 0x040003EE RID: 1006
	public Narrator narrator;

	// Token: 0x040003EF RID: 1007
	public AudioClip endgameClip;

	// Token: 0x040003F0 RID: 1008
	public AudioClip menuClip;
}
