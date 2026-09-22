using System;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;

// Token: 0x02000132 RID: 306
public class Narrator : MonoBehaviour
{
	// Token: 0x060007C4 RID: 1988 RVA: 0x000423B5 File Offset: 0x000407B5
	private void AddNewObservation()
	{
		this.Observations2.Add(new Narrator.DialogBit());
	}

	// Token: 0x060007C5 RID: 1989 RVA: 0x000423C7 File Offset: 0x000407C7
	private void RemoveObservation(int index)
	{
		this.Observations2.RemoveAt(index);
	}

	// Token: 0x060007C6 RID: 1990 RVA: 0x000423D5 File Offset: 0x000407D5
	private void AddNewCondolence()
	{
		this.Condolences2.Add(new Narrator.DialogBit());
	}

	// Token: 0x060007C7 RID: 1991 RVA: 0x000423E7 File Offset: 0x000407E7
	private void RemoveCondolence(int index)
	{
		this.Condolences2.RemoveAt(index);
	}

	// Token: 0x060007C8 RID: 1992 RVA: 0x000423F5 File Offset: 0x000407F5
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

	// Token: 0x060007C9 RID: 1993 RVA: 0x00042434 File Offset: 0x00040834
	public void UnPause()
	{
		this.DJ.UnPause();
		this.VO.UnPause();
		this.paused = false;
	}

	// Token: 0x060007CA RID: 1994 RVA: 0x00042453 File Offset: 0x00040853
	public void setDialogDoneLists(byte[] keyDialogDone, byte[] observationDialogDone, byte[] condolenceDialogDone)
	{
		this.newKeyDialogDone = keyDialogDone;
		this.newObservationDialogDone = observationDialogDone;
		this.newCondolenceDialogDone = condolenceDialogDone;
	}

	// Token: 0x060007CB RID: 1995 RVA: 0x0004246C File Offset: 0x0004086C
	public byte[] getKeyDialogDoneList()
	{
		for (int i = 0; i < this.KeyDialog.Count; i++)
		{
			this.DialogDoneReturnval[i] = ((!this.KeyDialog[i].heard) ? 0 : 1);
		}
		return this.DialogDoneReturnval;
	}

	// Token: 0x060007CC RID: 1996 RVA: 0x000424C0 File Offset: 0x000408C0
	public byte[] getObservationDialogDoneList()
	{
		for (int i = 0; i < this.Observations2.Count; i++)
		{
			this.ObservationReturnval[i] = ((!this.Observations2[i].heard) ? 0 : 1);
		}
		return this.ObservationReturnval;
	}

	// Token: 0x060007CD RID: 1997 RVA: 0x00042514 File Offset: 0x00040914
	public byte[] getCondolenceDialogDoneList()
	{
		for (int i = 0; i < this.Condolences2.Count; i++)
		{
			this.CondoluenceReturnval[i] = ((!this.Condolences2[i].heard) ? 0 : 1);
		}
		return this.CondoluenceReturnval;
	}

	// Token: 0x060007CE RID: 1998 RVA: 0x00042568 File Offset: 0x00040968
	public void SetLanguage(int newlang)
	{
		this.lang = newlang;
	}

	// Token: 0x060007CF RID: 1999 RVA: 0x00042574 File Offset: 0x00040974
	private void Awake()
	{
		this.charactersPerWord = new int[] { 5, 5, 3, 3, 3 };
		this.lang = 0;
		this.timePlayedThisGame = 0f;
		this.speedrun = false;
		this.observationsSinceLastThing = 0;
		this.showSubtitles = false;
		this.subtitleTimer = 0f;
		this.Subtitles.text = string.Empty;
		this.currentDistance = 0f;
		this.averageMouse = 0f;
		this.dialogueQueue = new List<Narrator.DialogBit>();
		this.currentSubtitleBuffer = new List<string>();
		this.newKeyDialogDone = new byte[this.keyDialogContainer.childCount];
		this.newObservationDialogDone = new byte[this.Observations2.Count];
		this.newCondolenceDialogDone = new byte[this.Condolences2.Count];
		this.CondoluenceReturnval = new byte[this.Condolences2.Count];
		this.ObservationReturnval = new byte[this.Observations2.Count];
		this.targetPianoVolume = 0f;
		this.pianoVolumeTimer = 1f;
		this.antennaBit = new Narrator.DialogBit();
		this.antennaBit.textLine = "DIALOG_STUCK_ANTENNA";
		this.antennaBit.audioLine = this.antennaClip;
		this.mouseBit = new Narrator.DialogBit();
		this.mouseBit.textLine = "DIALOG_MOUSE_CLICKED";
		this.mouseBit.audioLine = this.mouseClip;
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
		this.mouseBit.heard = true;
		this.Observations2[5].heard = true;
	}

	// Token: 0x060007D0 RID: 2000 RVA: 0x00042770 File Offset: 0x00040B70
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
		float num = this.averageMouse;
		Vector2 vector = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		this.averageMouse = Mathf.Lerp(num, vector.sqrMagnitude, 0.01f);
		if (this.KeyDialog == null)
		{
			this.KeyDialog = new List<Narrator.DialogBit>();
			foreach (DialogLine dialogLine in this.keyDialogContainer.GetComponentsInChildren<DialogLine>())
			{
				Narrator.DialogBit dialogBit = new Narrator.DialogBit();
				dialogBit.audioLine = dialogLine.clip;
				dialogBit.textLine = dialogLine.subtitles;
				dialogBit.isMusic = false;
				dialogBit.splinePos = this.levelSpline.TFToDistance(this.levelSpline.GetNearestPointTF(dialogLine.transform.position, 0, -1));
				this.KeyDialog.Add(dialogBit);
			}
			this.KeyDialog.Sort((Narrator.DialogBit x, Narrator.DialogBit y) => x.splinePos.CompareTo(y.splinePos));
			this.DialogDoneReturnval = new byte[this.KeyDialog.Count];
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
		for (int num2 = 0; num2 < this.KeyDialog.Count; num2++)
		{
			if (this.currentDistance > this.KeyDialog[num2].splinePos && !this.KeyDialog[num2].heard && !this.KeyDialog[num2].inUse)
			{
				if ((this.VO.isPlaying || this.DJ.isPlaying) && this.lastBit != null && !this.KeyDialog.Contains(this.lastBit))
				{
					this.currentSubtitleBuffer.Clear();
					this.currentSubtitleTimeBuffer.Clear();
					this.DJ.Stop();
					this.VO.Stop();
					this.subtitleTimer = 0f;
				}
				this.SayDialog(this.KeyDialog[num2]);
				try
				{
					Analytics.CustomEvent("Checkpoint", new Dictionary<string, object>
					{
						{ "index", num2 },
						{ "time", this.timePlayedThisGame }
					});
					break;
				}
				catch
				{
					Debug.LogWarning("Analytics failed");
				}
			}
		}
		if (this.lastBit != null && !this.lastBit.heard && !this.VO.isPlaying)
		{
			this.lastBit.heard = true;
			int num3 = this.KeyDialog.IndexOf(this.lastBit);
			if (num3 != -1)
			{
				this.maxKey = Mathf.Max(this.maxKey, num3);
			}
		}
		if (this.dialogueQueue.Count > 0 && this.subtitleTimer <= 0f && !this.VO.isPlaying && !this.DJ.isPlaying)
		{
			if (ScriptLocalization.Get(this.dialogueQueue[0].textLine, true, 0, true, false, null, null) != null)
			{
				this.currentSubtitleBuffer = new List<string>(ScriptLocalization.Get(this.dialogueQueue[0].textLine, true, 0, true, false, null, null).Split(new char[] { "\n\r"[0] }));
				for (int num4 = 0; num4 < this.currentSubtitleBuffer.Count - 1; num4++)
				{
					float num5 = 100f;
					if (num4 > 0)
					{
						num5 = ((float)this.currentSubtitleBuffer[num4 - 1].Length + 2f) * (1f / this.wordsPerSecond) / (float)this.charactersPerWord[this.lang];
					}
					float num6 = ((float)this.currentSubtitleBuffer[num4 + 1].Length + 2f) * (1f / this.wordsPerSecond) / (float)this.charactersPerWord[this.lang];
					float num7 = ((float)this.currentSubtitleBuffer[num4].Length + 2f) * (1f / this.wordsPerSecond) / (float)this.charactersPerWord[this.lang];
					if (num7 < 2.5f)
					{
						if (num6 < 10f)
						{
							this.currentSubtitleBuffer[num4 + 1] = this.currentSubtitleBuffer[num4] + "\n" + this.currentSubtitleBuffer[num4 + 1];
							this.currentSubtitleBuffer.RemoveAt(num4);
							num4--;
						}
						else if (num5 < 10f)
						{
							this.currentSubtitleBuffer[num4 - 1] = this.currentSubtitleBuffer[num4 - 1] + "\n" + this.currentSubtitleBuffer[num4];
							this.currentSubtitleBuffer.RemoveAt(num4);
							num4--;
						}
					}
				}
				this.currentSubtitleTimeBuffer = new List<float>();
				for (int num8 = 0; num8 < this.currentSubtitleBuffer.Count; num8++)
				{
					if (this.dialogueQueue[0].audioLine == null)
					{
						this.currentSubtitleTimeBuffer.Add(((float)this.currentSubtitleBuffer[num8].Length + 2f) * (1f / this.wordsPerSecond) / (float)this.charactersPerWord[this.lang]);
					}
					else if (!this.dialogueQueue[0].isMusic)
					{
						float num9 = (float)this.currentSubtitleBuffer[num8].Length / (float)ScriptLocalization.Get(this.dialogueQueue[0].textLine, true, 0, true, false, null, null).Length * this.dialogueQueue[0].audioLine.length;
						num9 = Mathf.Max(num9, 2f);
						if (num8 == this.currentSubtitleBuffer.Count - 1)
						{
							num9 += 2f;
						}
						this.currentSubtitleTimeBuffer.Add(num9);
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

	// Token: 0x060007D1 RID: 2001 RVA: 0x00043628 File Offset: 0x00041A28
	public void ToggleSubtitles(bool show)
	{
		this.showSubtitles = show;
	}

	// Token: 0x060007D2 RID: 2002 RVA: 0x00043634 File Offset: 0x00041A34
	public void SayDialog(Narrator.DialogBit bit)
	{
		if (this.speedrun && bit != this.antennaBit)
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

	// Token: 0x060007D3 RID: 2003 RVA: 0x000436A4 File Offset: 0x00041AA4
	public void SlowProgress()
	{
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
			Analytics.CustomEvent("Checkpoint", new Dictionary<string, object>
			{
				{ "index", 0 },
				{ "time", this.timePlayedThisGame }
			});
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

	// Token: 0x060007D4 RID: 2004 RVA: 0x0004389C File Offset: 0x00041C9C
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
				break;
			}
		}
	}

	// Token: 0x060007D5 RID: 2005 RVA: 0x00043A00 File Offset: 0x00041E00
	public void StuckOnAntenna()
	{
		this.SayDialog(this.antennaBit);
		this.antennaBit.heard = true;
	}

	// Token: 0x060007D6 RID: 2006 RVA: 0x00043A1A File Offset: 0x00041E1A
	public void UpdateDistance(float d)
	{
		this.currentDistance = d;
	}

	// Token: 0x0400072F RID: 1839
	public TextMeshProUGUI Subtitles;

	// Token: 0x04000730 RID: 1840
	private List<string> currentSubtitleBuffer;

	// Token: 0x04000731 RID: 1841
	private List<float> currentSubtitleTimeBuffer;

	// Token: 0x04000732 RID: 1842
	public List<Narrator.DialogBit> Condolences2 = new List<Narrator.DialogBit>(1);

	// Token: 0x04000733 RID: 1843
	public List<Narrator.DialogBit> Observations2 = new List<Narrator.DialogBit>(1);

	// Token: 0x04000734 RID: 1844
	private List<Narrator.DialogBit> KeyDialog;

	// Token: 0x04000736 RID: 1846
	public Transform keyDialogContainer;

	// Token: 0x04000737 RID: 1847
	private float currentDistance;

	// Token: 0x04000738 RID: 1848
	private bool showSubtitles;

	// Token: 0x04000739 RID: 1849
	private float subtitleTimer;

	// Token: 0x0400073A RID: 1850
	private float wordsPerSecond = 2.6f;

	// Token: 0x0400073B RID: 1851
	private int[] charactersPerWord;

	// Token: 0x0400073C RID: 1852
	public int lang;

	// Token: 0x0400073D RID: 1853
	private List<Narrator.DialogBit> dialogueQueue;

	// Token: 0x0400073E RID: 1854
	private Narrator.DialogBit lastBit;

	// Token: 0x0400073F RID: 1855
	private bool paused;

	// Token: 0x04000740 RID: 1856
	public AudioSource DJ;

	// Token: 0x04000741 RID: 1857
	public AudioSource VO;

	// Token: 0x04000742 RID: 1858
	public AudioSource Piano;

	// Token: 0x04000743 RID: 1859
	private float targetPianoVolume;

	// Token: 0x04000744 RID: 1860
	private float pianoVolumeTimer;

	// Token: 0x04000745 RID: 1861
	private byte[] newKeyDialogDone;

	// Token: 0x04000746 RID: 1862
	private byte[] newObservationDialogDone;

	// Token: 0x04000747 RID: 1863
	private byte[] newCondolenceDialogDone;

	// Token: 0x04000748 RID: 1864
	private int observationsSinceLastThing;

	// Token: 0x04000749 RID: 1865
	private int maxObservationsPerOtherThing = 3;

	// Token: 0x0400074A RID: 1866
	private float averageMouse;

	// Token: 0x0400074B RID: 1867
	public float timePlayedThisGame;

	// Token: 0x0400074C RID: 1868
	private int maxKey;

	// Token: 0x0400074D RID: 1869
	private int currentKey;

	// Token: 0x0400074E RID: 1870
	private Narrator.DialogBit mouseBit;

	// Token: 0x0400074F RID: 1871
	private Narrator.DialogBit antennaBit;

	// Token: 0x04000750 RID: 1872
	public AudioClip mouseClip;

	// Token: 0x04000751 RID: 1873
	public AudioClip antennaClip;

	// Token: 0x04000752 RID: 1874
	private int mouseClicks;

	// Token: 0x04000753 RID: 1875
	public bool speedrun;

	// Token: 0x04000754 RID: 1876
	private byte[] CondoluenceReturnval = new byte[0];

	// Token: 0x04000755 RID: 1877
	private byte[] DialogDoneReturnval = new byte[0];

	// Token: 0x04000756 RID: 1878
	private byte[] ObservationReturnval = new byte[0];

	// Token: 0x02000133 RID: 307
	[Serializable]
	public class DialogBit
	{
		// Token: 0x04000758 RID: 1880
		public string textLine;

		// Token: 0x04000759 RID: 1881
		public AudioClip audioLine;

		// Token: 0x0400075A RID: 1882
		public bool heard;

		// Token: 0x0400075B RID: 1883
		public float splinePos;

		// Token: 0x0400075C RID: 1884
		public bool isMusic;

		// Token: 0x0400075D RID: 1885
		public bool inUse;

		// Token: 0x0400075E RID: 1886
		public bool isObservation;
	}
}
