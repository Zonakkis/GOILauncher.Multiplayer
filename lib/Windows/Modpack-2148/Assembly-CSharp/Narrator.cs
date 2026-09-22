using System;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

// Token: 0x0200005C RID: 92
public class Narrator : MonoBehaviour
{
	// Token: 0x06000220 RID: 544 RVA: 0x00003A30 File Offset: 0x00001C30
	private void AddNewObservation()
	{
		this.Observations2.Add(new Narrator.DialogBit());
	}

	// Token: 0x06000221 RID: 545 RVA: 0x00003A42 File Offset: 0x00001C42
	private void RemoveObservation(int index)
	{
		this.Observations2.RemoveAt(index);
	}

	// Token: 0x06000222 RID: 546 RVA: 0x00003A50 File Offset: 0x00001C50
	private void AddNewCondolence()
	{
		this.Condolences2.Add(new Narrator.DialogBit());
	}

	// Token: 0x06000223 RID: 547 RVA: 0x00003A62 File Offset: 0x00001C62
	private void RemoveCondolence(int index)
	{
		this.Condolences2.RemoveAt(index);
	}

	// Token: 0x06000224 RID: 548 RVA: 0x00003A70 File Offset: 0x00001C70
	public void Pause()
	{
		this.paused = true;
		if (this.DJ.isPlaying)
		{
			this.DJ.Pause();
		}
		if (this.VO.isPlaying)
		{
			this.VO.Pause();
		}
	}

	// Token: 0x06000225 RID: 549 RVA: 0x00003AA9 File Offset: 0x00001CA9
	public void UnPause()
	{
		this.DJ.UnPause();
		this.VO.UnPause();
		this.paused = false;
	}

	// Token: 0x06000226 RID: 550 RVA: 0x00003AC8 File Offset: 0x00001CC8
	public void setDialogDoneLists(byte[] keyDialogDone, byte[] observationDialogDone, byte[] condolenceDialogDone)
	{
		this.newKeyDialogDone = keyDialogDone;
		this.newObservationDialogDone = observationDialogDone;
		this.newCondolenceDialogDone = condolenceDialogDone;
	}

	// Token: 0x06000227 RID: 551 RVA: 0x000221C8 File Offset: 0x000203C8
	public byte[] getKeyDialogDoneList()
	{
		byte[] array = new byte[this.KeyDialog.Count];
		for (int i = 0; i < this.KeyDialog.Count; i++)
		{
			array[i] = (this.KeyDialog[i].heard ? 1 : 0);
		}
		return array;
	}

	// Token: 0x06000228 RID: 552 RVA: 0x00022218 File Offset: 0x00020418
	public byte[] getObservationDialogDoneList()
	{
		byte[] array = new byte[this.Observations2.Count];
		for (int i = 0; i < this.Observations2.Count; i++)
		{
			array[i] = (this.Observations2[i].heard ? 1 : 0);
		}
		return array;
	}

	// Token: 0x06000229 RID: 553 RVA: 0x00022268 File Offset: 0x00020468
	public byte[] getCondolenceDialogDoneList()
	{
		byte[] array = new byte[this.Condolences2.Count];
		for (int i = 0; i < this.Condolences2.Count; i++)
		{
			array[i] = (this.Condolences2[i].heard ? 1 : 0);
		}
		return array;
	}

	// Token: 0x0600022A RID: 554 RVA: 0x00003ADF File Offset: 0x00001CDF
	public void SetLanguage(int newlang)
	{
		this.lang = newlang;
		Debug.Log("characters per word: " + this.charactersPerWord[newlang].ToString());
	}

	// Token: 0x0600022B RID: 555 RVA: 0x000222B8 File Offset: 0x000204B8
	private void Awake()
	{
		this.charactersPerWord = new int[] { 5, 5, 3, 3, 3 };
		this.lang = 0;
		this.timePlayedThisGame = 0f;
		this.speedrun = false;
		this.observationsSinceLastThing = 0;
		this.showSubtitles = false;
		this.subtitleTimer = 0f;
		this.Subtitles.text = "";
		this.currentDistance = 0f;
		this.averageMouse = 0f;
		this.dialogueQueue = new List<Narrator.DialogBit>();
		this.currentSubtitleBuffer = new List<string>();
		this.newKeyDialogDone = new byte[this.keyDialogContainer.childCount];
		this.newObservationDialogDone = new byte[this.Observations2.Count];
		this.newCondolenceDialogDone = new byte[this.Condolences2.Count];
		this.targetPianoVolume = 0f;
		this.pianoVolumeTimer = 1f;
		this.antennaBit = new Narrator.DialogBit();
		this.antennaBit.textLine = "DIALOG_STUCK_ANTENNA";
		this.antennaBit.audioLine = this.antennaClip;
		this.mouseBit = new Narrator.DialogBit();
		this.mouseBit.textLine = "DIALOG_MOUSE_CLICKED";
		this.mouseBit.audioLine = this.mouseClip;
		this.speedBit = new Narrator.DialogBit();
		this.speedBit.textLine = "SPEEDRUN_DETECTED";
		this.speedBit.audioLine = this.speedClip;
		this.speedBit.isMusic = true;
		if (PlayerPrefs.HasKey("Clicks"))
		{
			this.mouseClicks = PlayerPrefs.GetInt("Clicks");
			if (this.mouseClicks >= 500)
			{
				this.mouseBit.heard = true;
			}
		}
		else
		{
			this.mouseClicks = 0;
			PlayerPrefs.SetInt("Clicks", 0);
			PlayerPrefs.Save();
		}
		this.currentSubtitleBuffer = new List<string>();
		this.currentSubtitleTimeBuffer = new List<float>();
	}

	// Token: 0x0600022C RID: 556 RVA: 0x00022498 File Offset: 0x00020698
	private void LateUpdate()
	{
		if (this.newKeyDialogDone != null && this.newKeyDialogDone.Length == 0)
		{
			this.observationsSinceLastThing = this.maxObservationsPerOtherThing;
		}
		if (this.pianoVolumeTimer > 0f)
		{
			this.pianoVolumeTimer -= Time.deltaTime * 0.1f;
			if (this.pianoVolumeTimer < 0f)
			{
				this.pianoVolumeTimer = 0f;
			}
			if (this.Piano.volume < this.targetPianoVolume)
			{
				this.Piano.volume = Mathf.SmoothStep(1f, 0f, this.pianoVolumeTimer);
			}
			else if (this.Piano.volume > this.targetPianoVolume)
			{
				this.Piano.volume = Mathf.SmoothStep(0f, 1f, this.pianoVolumeTimer);
			}
		}
		this.averageMouse = Mathf.Lerp(this.averageMouse, new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).sqrMagnitude, 0.01f);
		if (this.KeyDialog == null || this.KeyDialog.Count == 0)
		{
			this.KeyDialog = new List<Narrator.DialogBit>();
			foreach (DialogLine dialogLine in this.keyDialogContainer.GetComponentsInChildren<DialogLine>())
			{
				Narrator.DialogBit dialogBit = new Narrator.DialogBit();
				dialogBit.audioLine = dialogLine.clip;
				dialogBit.textLine = dialogLine.subtitles;
				dialogBit.isMusic = false;
				this.KeyDialog.Add(dialogBit);
			}
			this.KeyDialog.Sort((Narrator.DialogBit x, Narrator.DialogBit y) => x.splinePos.CompareTo(y.splinePos));
		}
		if (this.KeyDialog != null && this.newKeyDialogDone != null && this.newKeyDialogDone.Length == this.KeyDialog.Count && this.newObservationDialogDone.Length == this.Observations2.Count && this.newCondolenceDialogDone.Length == this.Condolences2.Count)
		{
			for (int j = 0; j < this.newKeyDialogDone.Length; j++)
			{
				this.KeyDialog[j].heard = this.newKeyDialogDone[j] == 1;
				if (j > 0 && this.KeyDialog[j - 1].heard && !this.KeyDialog[j].heard)
				{
					this.maxKey = j - 1;
				}
			}
			for (int k = 0; k < this.newObservationDialogDone.Length; k++)
			{
				this.Observations2[k].heard = this.newObservationDialogDone[k] == 1;
			}
			for (int l = 0; l < this.newCondolenceDialogDone.Length; l++)
			{
				this.Condolences2[l].heard = this.newCondolenceDialogDone[l] == 1;
			}
			this.newKeyDialogDone = null;
			this.newObservationDialogDone = null;
			this.newCondolenceDialogDone = null;
		}
		for (int m = 1; m < this.KeyDialog.Count; m++)
		{
			if (this.currentDistance > this.KeyDialog[m - 1].splinePos && this.currentDistance < this.KeyDialog[m].splinePos)
			{
				this.currentKey = m - 1;
				break;
			}
		}
		if (this.subtitleTimer > 0f && !this.paused)
		{
			this.subtitleTimer -= Time.deltaTime;
			if (this.subtitleTimer < 0f)
			{
				this.subtitleTimer = 0f;
			}
		}
		if (this.showSubtitles)
		{
			if (this.subtitleTimer < 1f)
			{
				this.Subtitles.color = new Color(this.Subtitles.color.r, this.Subtitles.color.g, this.Subtitles.color.b, Mathf.Lerp(this.Subtitles.color.a, 0f, 0.1f));
			}
			else if (this.subtitleTimer > 0f)
			{
				this.Subtitles.color = new Color(this.Subtitles.color.r, this.Subtitles.color.g, this.Subtitles.color.b, Mathf.Lerp(this.Subtitles.color.a, 1f, 0.1f));
			}
		}
		else
		{
			this.Subtitles.color = new Color(this.Subtitles.color.r, this.Subtitles.color.g, this.Subtitles.color.b, 0f);
		}
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.W))
			{
				this.currentSubtitleBuffer.Clear();
				this.currentSubtitleTimeBuffer.Clear();
				this.DJ.Stop();
				this.VO.Stop();
				this.subtitleTimer = 0f;
				this.KeyDialog[10].heard = false;
				this.SayDialog(this.KeyDialog[10]);
			}
			for (int n = 0; n < this.KeyDialog.Count; n++)
			{
				if (Input.GetKeyDown(KeyCode.S))
				{
					this.speedrun = false;
					if (!this.KeyDialog[n].heard)
					{
						this.currentSubtitleBuffer.Clear();
						this.currentSubtitleTimeBuffer.Clear();
						this.DJ.Stop();
						this.VO.Stop();
						this.subtitleTimer = 0f;
						this.SayDialog(this.KeyDialog[n]);
						break;
					}
				}
			}
		}
		for (int num = 0; num < this.KeyDialog.Count; num++)
		{
			if (this.currentDistance > this.KeyDialog[num].splinePos && !this.KeyDialog[num].heard && !this.KeyDialog[num].inUse)
			{
				if ((this.VO.isPlaying || this.DJ.isPlaying) && this.lastBit != null && !this.KeyDialog.Contains(this.lastBit))
				{
					this.currentSubtitleBuffer.Clear();
					this.currentSubtitleTimeBuffer.Clear();
					this.DJ.Stop();
					this.VO.Stop();
					this.subtitleTimer = 0f;
				}
				this.SayDialog(this.KeyDialog[num]);
				break;
			}
		}
		if (this.lastBit != null && !this.lastBit.heard && !this.VO.isPlaying)
		{
			this.lastBit.heard = true;
			int num2 = this.KeyDialog.IndexOf(this.lastBit);
			Debug.Log("setting dialog " + num2.ToString() + " heard");
			if (num2 != -1)
			{
				this.maxKey = Mathf.Max(this.maxKey, num2);
			}
		}
		if (this.dialogueQueue.Count > 0 && this.subtitleTimer <= 0f && !this.VO.isPlaying && !this.DJ.isPlaying)
		{
			if (ScriptLocalization.Get(this.dialogueQueue[0].textLine, true, 0, true, false, null, null) != null)
			{
				this.currentSubtitleBuffer = new List<string>(ScriptLocalization.Get(this.dialogueQueue[0].textLine, true, 0, true, false, null, null).Split(new char[] { "\n\r"[0] }));
				for (int num3 = 0; num3 < this.currentSubtitleBuffer.Count - 1; num3++)
				{
					float num4 = 100f;
					if (num3 > 0)
					{
						num4 = ((float)this.currentSubtitleBuffer[num3 - 1].Length + 2f) * (1f / this.wordsPerSecond) / (float)this.charactersPerWord[this.lang];
					}
					float num5 = ((float)this.currentSubtitleBuffer[num3 + 1].Length + 2f) * (1f / this.wordsPerSecond) / (float)this.charactersPerWord[this.lang];
					if (((float)this.currentSubtitleBuffer[num3].Length + 2f) * (1f / this.wordsPerSecond) / (float)this.charactersPerWord[this.lang] < 2.5f)
					{
						Debug.Log("consolidating two short lines");
						if (num5 < 10f)
						{
							this.currentSubtitleBuffer[num3 + 1] = this.currentSubtitleBuffer[num3] + "\n" + this.currentSubtitleBuffer[num3 + 1];
							this.currentSubtitleBuffer.RemoveAt(num3);
							num3--;
						}
						else if (num4 < 10f)
						{
							this.currentSubtitleBuffer[num3 - 1] = this.currentSubtitleBuffer[num3 - 1] + "\n" + this.currentSubtitleBuffer[num3];
							this.currentSubtitleBuffer.RemoveAt(num3);
							num3--;
						}
					}
				}
				this.currentSubtitleTimeBuffer = new List<float>();
				for (int num6 = 0; num6 < this.currentSubtitleBuffer.Count; num6++)
				{
					if (this.dialogueQueue[0].audioLine == null)
					{
						this.currentSubtitleTimeBuffer.Add(((float)this.currentSubtitleBuffer[num6].Length + 2f) * (1f / this.wordsPerSecond) / (float)this.charactersPerWord[this.lang]);
					}
					else if (!this.dialogueQueue[0].isMusic)
					{
						float num7 = (float)this.currentSubtitleBuffer[num6].Length / (float)ScriptLocalization.Get(this.dialogueQueue[0].textLine, true, 0, true, false, null, null).Length * this.dialogueQueue[0].audioLine.length;
						num7 = Mathf.Max(num7, 2f);
						if (num6 == this.currentSubtitleBuffer.Count - 1)
						{
							num7 += 2f;
						}
						this.currentSubtitleTimeBuffer.Add(num7);
					}
					else
					{
						this.currentSubtitleTimeBuffer.Add(8f);
					}
				}
			}
			if (this.dialogueQueue[0].isMusic)
			{
				if (this.dialogueQueue[0].audioLine != null)
				{
					this.DJ.clip = this.dialogueQueue[0].audioLine;
					this.DJ.Play();
					if (this.Piano.volume > 0f)
					{
						this.targetPianoVolume = 0f;
						this.pianoVolumeTimer = 1f;
					}
				}
			}
			else if (this.dialogueQueue[0].audioLine != null)
			{
				this.VO.clip = this.dialogueQueue[0].audioLine;
				this.VO.Play();
				Debug.Log("key dialog count " + this.KeyDialog.Count.ToString());
				if (this.Piano.volume < 1f && !this.dialogueQueue[0].isObservation && this.dialogueQueue[0] != this.KeyDialog[this.KeyDialog.Count - 1])
				{
					this.targetPianoVolume = 1f;
					this.pianoVolumeTimer = 1f;
				}
				else if (this.Piano.volume > 0f && this.dialogueQueue[0].isObservation)
				{
					this.targetPianoVolume = 0f;
					this.pianoVolumeTimer = 1f;
				}
			}
			this.lastBit = this.dialogueQueue[0];
			this.dialogueQueue.RemoveAt(0);
		}
		if (this.currentSubtitleBuffer.Count > 0 && this.subtitleTimer <= 0f)
		{
			this.Subtitles.text = this.currentSubtitleBuffer[0];
			this.subtitleTimer = this.currentSubtitleTimeBuffer[0];
			this.currentSubtitleBuffer.RemoveAt(0);
			this.currentSubtitleTimeBuffer.RemoveAt(0);
		}
		if (!this.DJ.isPlaying && !this.VO.isPlaying && this.dialogueQueue.Count == 0 && this.Piano.volume == 1f)
		{
			this.targetPianoVolume = 0f;
			this.pianoVolumeTimer = 1f;
		}
		this.timePlayedThisGame += Time.deltaTime;
		if (!this.mouseBit.heard && !this.paused && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
		{
			this.mouseClicks++;
			if (this.mouseClicks % 100 == 0)
			{
				PlayerPrefs.SetInt("Clicks", this.mouseClicks);
				PlayerPrefs.Save();
			}
			if (this.mouseClicks >= 500)
			{
				this.SayDialog(this.mouseBit);
			}
		}
	}

	// Token: 0x0600022D RID: 557 RVA: 0x00003B08 File Offset: 0x00001D08
	public void ToggleSubtitles(bool show)
	{
		this.showSubtitles = show;
	}

	// Token: 0x0600022E RID: 558 RVA: 0x00023224 File Offset: 0x00021424
	public void SayDialog(Narrator.DialogBit bit)
	{
		if (this.speedrun && bit != this.antennaBit && bit != this.speedBit)
		{
			bit.heard = true;
			return;
		}
		if (bit.inUse)
		{
			return;
		}
		if (!bit.isObservation)
		{
			this.observationsSinceLastThing = 0;
		}
		else
		{
			this.observationsSinceLastThing++;
		}
		this.dialogueQueue.Add(bit);
		bit.inUse = true;
	}

	// Token: 0x0600022F RID: 559 RVA: 0x00023290 File Offset: 0x00021490
	public void SlowProgress()
	{
		Debug.Log("slow prog. avg mous:" + this.averageMouse.ToString());
		if (this.Condolences2[this.Condolences2.Count - 1].heard)
		{
			return;
		}
		if (this.KeyDialog[21].heard)
		{
			return;
		}
		if (this.averageMouse < 0.05f)
		{
			return;
		}
		if (this.dialogueQueue.Count > 0 || this.VO.isPlaying || this.DJ.isPlaying)
		{
			return;
		}
		if (!this.KeyDialog[0].heard)
		{
			this.SayDialog(this.KeyDialog[0]);
			this.maxKey = 0;
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (!this.Observations2[3 * this.currentKey + i].heard && !this.Observations2[3 * this.currentKey + i].inUse)
			{
				this.SayDialog(this.Observations2[i]);
				this.Observations2[3 * this.currentKey + i].heard = true;
				return;
			}
		}
		if (!this.Observations2[3 * this.currentKey + 2].heard && !this.Observations2[3 * this.currentKey + 2].inUse && this.maxKey > this.currentKey)
		{
			this.SayDialog(this.Observations2[3 * this.currentKey + 2]);
			this.Observations2[3 * this.currentKey + 2].heard = true;
			return;
		}
	}

	// Token: 0x06000230 RID: 560 RVA: 0x00023440 File Offset: 0x00021640
	public void FastRetreat()
	{
		if (this.dialogueQueue.Count > 0)
		{
			return;
		}
		if (this.VO.isPlaying || this.DJ.isPlaying)
		{
			bool flag = false;
			foreach (Narrator.DialogBit dialogBit in this.Observations2)
			{
				if (dialogBit.inUse)
				{
					if (this.lastBit == dialogBit)
					{
						flag = true;
					}
					foreach (Narrator.DialogBit dialogBit2 in this.dialogueQueue)
					{
						if (dialogBit2 == dialogBit)
						{
							this.dialogueQueue.Remove(dialogBit2);
						}
					}
				}
			}
			if (!flag)
			{
				return;
			}
		}
		for (int i = 0; i < this.Condolences2.Count; i++)
		{
			if (!this.Condolences2[i].heard)
			{
				this.SayDialog(this.Condolences2[i]);
				this.Condolences2[i].heard = true;
				return;
			}
		}
	}

	// Token: 0x06000231 RID: 561 RVA: 0x00003B11 File Offset: 0x00001D11
	public void StuckOnAntenna()
	{
		this.SayDialog(this.antennaBit);
		this.antennaBit.heard = true;
	}

	// Token: 0x06000232 RID: 562 RVA: 0x00003B2B File Offset: 0x00001D2B
	public void UpdateDistance(float d)
	{
		this.currentDistance = d;
	}

	// Token: 0x04000349 RID: 841
	public TextMeshProUGUI Subtitles;

	// Token: 0x0400034A RID: 842
	private List<string> currentSubtitleBuffer;

	// Token: 0x0400034B RID: 843
	private List<float> currentSubtitleTimeBuffer;

	// Token: 0x0400034C RID: 844
	public List<Narrator.DialogBit> Condolences2 = new List<Narrator.DialogBit>(1);

	// Token: 0x0400034D RID: 845
	public List<Narrator.DialogBit> Observations2 = new List<Narrator.DialogBit>(1);

	// Token: 0x0400034E RID: 846
	public List<Narrator.DialogBit> KeyDialog;

	// Token: 0x04000350 RID: 848
	public Transform keyDialogContainer;

	// Token: 0x04000351 RID: 849
	private float currentDistance;

	// Token: 0x04000352 RID: 850
	private bool showSubtitles;

	// Token: 0x04000353 RID: 851
	private float subtitleTimer;

	// Token: 0x04000354 RID: 852
	private float wordsPerSecond = 2.6f;

	// Token: 0x04000355 RID: 853
	private int[] charactersPerWord;

	// Token: 0x04000356 RID: 854
	public int lang;

	// Token: 0x04000357 RID: 855
	private List<Narrator.DialogBit> dialogueQueue;

	// Token: 0x04000358 RID: 856
	private Narrator.DialogBit lastBit;

	// Token: 0x04000359 RID: 857
	private bool paused;

	// Token: 0x0400035A RID: 858
	public AudioSource DJ;

	// Token: 0x0400035B RID: 859
	public AudioSource VO;

	// Token: 0x0400035C RID: 860
	public AudioSource Piano;

	// Token: 0x0400035D RID: 861
	private float targetPianoVolume;

	// Token: 0x0400035E RID: 862
	private float pianoVolumeTimer;

	// Token: 0x0400035F RID: 863
	private byte[] newKeyDialogDone;

	// Token: 0x04000360 RID: 864
	private byte[] newObservationDialogDone;

	// Token: 0x04000361 RID: 865
	private byte[] newCondolenceDialogDone;

	// Token: 0x04000362 RID: 866
	private int observationsSinceLastThing;

	// Token: 0x04000363 RID: 867
	private int maxObservationsPerOtherThing = 3;

	// Token: 0x04000364 RID: 868
	private float averageMouse;

	// Token: 0x04000365 RID: 869
	public float timePlayedThisGame;

	// Token: 0x04000366 RID: 870
	private int maxKey;

	// Token: 0x04000367 RID: 871
	private int currentKey;

	// Token: 0x04000368 RID: 872
	private Narrator.DialogBit mouseBit;

	// Token: 0x04000369 RID: 873
	private Narrator.DialogBit antennaBit;

	// Token: 0x0400036A RID: 874
	private Narrator.DialogBit speedBit;

	// Token: 0x0400036B RID: 875
	public AudioClip mouseClip;

	// Token: 0x0400036C RID: 876
	public AudioClip antennaClip;

	// Token: 0x0400036D RID: 877
	public AudioClip speedClip;

	// Token: 0x0400036E RID: 878
	private int mouseClicks;

	// Token: 0x0400036F RID: 879
	public bool speedrun;

	// Token: 0x0200005D RID: 93
	[Serializable]
	public class DialogBit
	{
		// Token: 0x04000370 RID: 880
		public string textLine;

		// Token: 0x04000371 RID: 881
		public AudioClip audioLine;

		// Token: 0x04000372 RID: 882
		public bool heard;

		// Token: 0x04000373 RID: 883
		public float splinePos;

		// Token: 0x04000374 RID: 884
		public bool isMusic;

		// Token: 0x04000375 RID: 885
		public bool inUse;

		// Token: 0x04000376 RID: 886
		public bool isObservation;
	}
}
