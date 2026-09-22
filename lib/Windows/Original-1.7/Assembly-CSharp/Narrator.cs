using System;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;

// Token: 0x0200003B RID: 59
public class Narrator : MonoBehaviour
{
	// Token: 0x060001B4 RID: 436 RVA: 0x00010300 File Offset: 0x0000E500
	private void AddNewObservation()
	{
		this.Observations2.Add(new Narrator.DialogBit());
	}

	// Token: 0x060001B5 RID: 437 RVA: 0x00010312 File Offset: 0x0000E512
	private void RemoveObservation(int index)
	{
		this.Observations2.RemoveAt(index);
	}

	// Token: 0x060001B6 RID: 438 RVA: 0x00010320 File Offset: 0x0000E520
	private void AddNewCondolence()
	{
		this.Condolences2.Add(new Narrator.DialogBit());
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x00010332 File Offset: 0x0000E532
	private void RemoveCondolence(int index)
	{
		this.Condolences2.RemoveAt(index);
	}

	// Token: 0x060001B8 RID: 440 RVA: 0x00010340 File Offset: 0x0000E540
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

	// Token: 0x060001B9 RID: 441 RVA: 0x00010379 File Offset: 0x0000E579
	public void UnPause()
	{
		this.DJ.UnPause();
		this.VO.UnPause();
		this.paused = false;
	}

	// Token: 0x060001BA RID: 442 RVA: 0x00010398 File Offset: 0x0000E598
	public void setDialogDoneLists(byte[] keyDialogDone, byte[] observationDialogDone, byte[] condolenceDialogDone)
	{
		this.newKeyDialogDone = keyDialogDone;
		this.newObservationDialogDone = observationDialogDone;
		this.newCondolenceDialogDone = condolenceDialogDone;
	}

	// Token: 0x060001BB RID: 443 RVA: 0x000103B0 File Offset: 0x0000E5B0
	public byte[] getKeyDialogDoneList()
	{
		byte[] array = new byte[this.KeyDialog.Count];
		for (int i = 0; i < this.KeyDialog.Count; i++)
		{
			array[i] = (this.KeyDialog[i].heard ? 1 : 0);
		}
		return array;
	}

	// Token: 0x060001BC RID: 444 RVA: 0x00010400 File Offset: 0x0000E600
	public byte[] getObservationDialogDoneList()
	{
		byte[] array = new byte[this.Observations2.Count];
		for (int i = 0; i < this.Observations2.Count; i++)
		{
			array[i] = (this.Observations2[i].heard ? 1 : 0);
		}
		return array;
	}

	// Token: 0x060001BD RID: 445 RVA: 0x00010450 File Offset: 0x0000E650
	public byte[] getCondolenceDialogDoneList()
	{
		byte[] array = new byte[this.Condolences2.Count];
		for (int i = 0; i < this.Condolences2.Count; i++)
		{
			array[i] = (this.Condolences2[i].heard ? 1 : 0);
		}
		return array;
	}

	// Token: 0x060001BE RID: 446 RVA: 0x0001049F File Offset: 0x0000E69F
	public void SetLanguage(int newlang)
	{
		this.lang = newlang;
		Debug.Log("characters per word: " + this.charactersPerWord[newlang].ToString());
	}

	// Token: 0x060001BF RID: 447 RVA: 0x000104C8 File Offset: 0x0000E6C8
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

	// Token: 0x060001C0 RID: 448 RVA: 0x000106A8 File Offset: 0x0000E8A8
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

	// Token: 0x060001C1 RID: 449 RVA: 0x00011432 File Offset: 0x0000F632
	public void ToggleSubtitles(bool show)
	{
		this.showSubtitles = show;
	}

	// Token: 0x060001C2 RID: 450 RVA: 0x0001143C File Offset: 0x0000F63C
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

	// Token: 0x060001C3 RID: 451 RVA: 0x000114A8 File Offset: 0x0000F6A8
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

	// Token: 0x060001C4 RID: 452 RVA: 0x00011658 File Offset: 0x0000F858
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

	// Token: 0x060001C5 RID: 453 RVA: 0x00011790 File Offset: 0x0000F990
	public void StuckOnAntenna()
	{
		this.SayDialog(this.antennaBit);
		this.antennaBit.heard = true;
	}

	// Token: 0x060001C6 RID: 454 RVA: 0x000117AA File Offset: 0x0000F9AA
	public void UpdateDistance(float d)
	{
		this.currentDistance = d;
	}

	// Token: 0x040002B7 RID: 695
	public TextMeshProUGUI Subtitles;

	// Token: 0x040002B8 RID: 696
	private List<string> currentSubtitleBuffer;

	// Token: 0x040002B9 RID: 697
	private List<float> currentSubtitleTimeBuffer;

	// Token: 0x040002BA RID: 698
	public List<Narrator.DialogBit> Condolences2 = new List<Narrator.DialogBit>(1);

	// Token: 0x040002BB RID: 699
	public List<Narrator.DialogBit> Observations2 = new List<Narrator.DialogBit>(1);

	// Token: 0x040002BC RID: 700
	public List<Narrator.DialogBit> KeyDialog;

	// Token: 0x040002BE RID: 702
	public Transform keyDialogContainer;

	// Token: 0x040002BF RID: 703
	private float currentDistance;

	// Token: 0x040002C0 RID: 704
	private bool showSubtitles;

	// Token: 0x040002C1 RID: 705
	private float subtitleTimer;

	// Token: 0x040002C2 RID: 706
	private float wordsPerSecond = 2.6f;

	// Token: 0x040002C3 RID: 707
	private int[] charactersPerWord;

	// Token: 0x040002C4 RID: 708
	public int lang;

	// Token: 0x040002C5 RID: 709
	private List<Narrator.DialogBit> dialogueQueue;

	// Token: 0x040002C6 RID: 710
	private Narrator.DialogBit lastBit;

	// Token: 0x040002C7 RID: 711
	private bool paused;

	// Token: 0x040002C8 RID: 712
	public AudioSource DJ;

	// Token: 0x040002C9 RID: 713
	public AudioSource VO;

	// Token: 0x040002CA RID: 714
	public AudioSource Piano;

	// Token: 0x040002CB RID: 715
	private float targetPianoVolume;

	// Token: 0x040002CC RID: 716
	private float pianoVolumeTimer;

	// Token: 0x040002CD RID: 717
	private byte[] newKeyDialogDone;

	// Token: 0x040002CE RID: 718
	private byte[] newObservationDialogDone;

	// Token: 0x040002CF RID: 719
	private byte[] newCondolenceDialogDone;

	// Token: 0x040002D0 RID: 720
	private int observationsSinceLastThing;

	// Token: 0x040002D1 RID: 721
	private int maxObservationsPerOtherThing = 3;

	// Token: 0x040002D2 RID: 722
	private float averageMouse;

	// Token: 0x040002D3 RID: 723
	public float timePlayedThisGame;

	// Token: 0x040002D4 RID: 724
	private int maxKey;

	// Token: 0x040002D5 RID: 725
	private int currentKey;

	// Token: 0x040002D6 RID: 726
	private Narrator.DialogBit mouseBit;

	// Token: 0x040002D7 RID: 727
	private Narrator.DialogBit antennaBit;

	// Token: 0x040002D8 RID: 728
	private Narrator.DialogBit speedBit;

	// Token: 0x040002D9 RID: 729
	public AudioClip mouseClip;

	// Token: 0x040002DA RID: 730
	public AudioClip antennaClip;

	// Token: 0x040002DB RID: 731
	public AudioClip speedClip;

	// Token: 0x040002DC RID: 732
	private int mouseClicks;

	// Token: 0x040002DD RID: 733
	public bool speedrun;

	// Token: 0x02000239 RID: 569
	[Serializable]
	public class DialogBit
	{
		// Token: 0x04000E5A RID: 3674
		public string textLine;

		// Token: 0x04000E5B RID: 3675
		public AudioClip audioLine;

		// Token: 0x04000E5C RID: 3676
		public bool heard;

		// Token: 0x04000E5D RID: 3677
		public float splinePos;

		// Token: 0x04000E5E RID: 3678
		public bool isMusic;

		// Token: 0x04000E5F RID: 3679
		public bool inUse;

		// Token: 0x04000E60 RID: 3680
		public bool isObservation;
	}
}
