using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000072 RID: 114
public class Scroller : MonoBehaviour
{
	// Token: 0x060002F1 RID: 753 RVA: 0x00028928 File Offset: 0x00026B28
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

	// Token: 0x060002F2 RID: 754 RVA: 0x000289FC File Offset: 0x00026BFC
	private void Update()
	{
		Vector3 position = base.transform.position;
		position.y += this.scrollDelta * 60f * Time.deltaTime * Mathf.Sin((90f + SettingsManager.sideWayDegrees) * 0.017453292f);
		position.x += this.scrollDelta * 60f * Time.deltaTime * Mathf.Cos((90f + SettingsManager.sideWayDegrees) * 0.017453292f);
		base.transform.position = position;
		this.startSpaceRect += this.scrollDelta * 60f * Time.deltaTime;
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
		{
			base.StartCoroutine("FadeOut");
			return;
		}
		if (this.startSpaceRect >= 68.83864f || (this.myRect.TransformPoint(this.myRect.rect.min).y > this.canvas.TransformPoint(this.canvas.rect.max).y && this.startSpaceRect >= 67.83864f))
		{
			base.StartCoroutine("FadeOut");
		}
	}

	// Token: 0x060002F3 RID: 755 RVA: 0x00004102 File Offset: 0x00002302
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

	// Token: 0x060002F4 RID: 756 RVA: 0x00028B40 File Offset: 0x00026D40
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

	// Token: 0x060002F5 RID: 757 RVA: 0x00028BD0 File Offset: 0x00026DD0
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

	// Token: 0x04000494 RID: 1172
	private RectTransform canvas;

	// Token: 0x04000495 RID: 1173
	private RectTransform myRect;

	// Token: 0x04000496 RID: 1174
	public bool inGame;

	// Token: 0x04000497 RID: 1175
	private float scrollDelta;

	// Token: 0x04000498 RID: 1176
	private float fadeAlpha;

	// Token: 0x04000499 RID: 1177
	private AudioSource aud;

	// Token: 0x0400049A RID: 1178
	public Texture2D fadeTex;

	// Token: 0x0400049B RID: 1179
	public Narrator narrator;

	// Token: 0x0400049C RID: 1180
	public AudioClip endgameClip;

	// Token: 0x0400049D RID: 1181
	public AudioClip menuClip;

	// Token: 0x0400049E RID: 1182
	private float startSpaceRect;
}
