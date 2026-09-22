using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

// Token: 0x020002FB RID: 763
public class Timer
{
	// Token: 0x060019EF RID: 6639 RVA: 0x0007D728 File Offset: 0x0007B928
	public Timer()
	{
		this.runName = "";
		this.changed = false;
		this.pb = false;
		this.enabled = false;
		this.started = false;
		this.shiftEnabled = false;
		this.debug = false;
		this.screenHeight = 0;
		this.optionsTitle = new Dictionary<int, string>();
		this.optionsTitle.Add(0, "Splits");
		this.optionsTitle.Add(1, "Position");
		this.optionsTitle.Add(2, "Segments");
		this.optionsTitle.Add(3, "Display On");
		this.optionsTitle.Add(4, "Only Timer Color");
		this.optionsTitle.Add(5, "Hide Segments");
		this.optionsTitle.Add(6, "Show Golds On...");
		this.optionsTitle.Add(7, "Better Delta Color");
		this.options = new Dictionary<int, List<string>>();
		this.options.Add(0, new List<string> { "Disabled", "Hidden", "Enabled" });
		this.options.Add(1, new List<string> { "Top Left", "Top Right" });
		this.options.Add(2, new List<string> { "Personal Best", "Best Segment" });
		this.options.Add(3, new List<string> { "Gameplay Only", "Reward Screen", "Start Menu", "ALL" });
		this.options.Add(4, new List<string> { "Splits based", "White" });
		this.options.Add(5, new List<string> { "No", "Yes" });
		this.options.Add(6, new List<string> { "Segments", "Delta", "Both" });
		this.options.Add(7, new List<string> { "No", "Yes" });
		this.optionsSelect = new Dictionary<int, int>();
		foreach (KeyValuePair<int, string> keyValuePair in this.optionsTitle)
		{
			if (PlayerPrefs.HasKey(keyValuePair.Value.Trim()))
			{
				this.optionsSelect.Add(keyValuePair.Key, PlayerPrefs.GetInt(keyValuePair.Value.Trim()));
			}
			else if (keyValuePair.Key == 3)
			{
				this.optionsSelect.Add(keyValuePair.Key, 3);
			}
			else if (keyValuePair.Key == 0)
			{
				this.optionsSelect.Add(keyValuePair.Key, 2);
			}
			else if (keyValuePair.Key == 7)
			{
				this.optionsSelect.Add(keyValuePair.Key, 1);
			}
			else
			{
				this.optionsSelect.Add(keyValuePair.Key, 0);
			}
		}
	}

	// Token: 0x060019F0 RID: 6640 RVA: 0x0000265E File Offset: 0x0000085E
	public void Load()
	{
	}

	// Token: 0x170006FE RID: 1790
	// (get) Token: 0x060019F1 RID: 6641 RVA: 0x000133C5 File Offset: 0x000115C5
	// (set) Token: 0x060019F2 RID: 6642 RVA: 0x000133CD File Offset: 0x000115CD
	private string runName { get; set; }

	// Token: 0x170006FF RID: 1791
	// (get) Token: 0x060019F3 RID: 6643 RVA: 0x000133D6 File Offset: 0x000115D6
	// (set) Token: 0x060019F4 RID: 6644 RVA: 0x000133DE File Offset: 0x000115DE
	public Timer.Run run { get; set; }

	// Token: 0x060019F5 RID: 6645 RVA: 0x0007DA70 File Offset: 0x0007BC70
	public bool LoadRun(Map map, string setting, List<string> mods)
	{
		if (this.changed || this.pb)
		{
			this.SaveRun();
		}
		this.changed = false;
		this.pb = false;
		this.enabled = false;
		this.stopTimer = false;
		this.started = false;
		if (map.name.Length <= 0)
		{
			this.enabled = false;
			return false;
		}
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(Timer.Run));
		this.runName = this.getRunName(map, setting, mods);
		ModPackManager.createDir(string.Format("modpack\\splits\\{0}\\", map.name));
		try
		{
			if (!ModPackManager.FileInUse(string.Format("modpack\\splits\\{0}\\{1}.lss", map.name, this.runName)) && File.Exists(string.Format("modpack\\splits\\{0}\\{1}.lss", map.name, this.runName)))
			{
				StreamReader streamReader = new StreamReader(string.Format("modpack\\splits\\{0}\\{1}.lss", map.name, this.runName), new UTF8Encoding(false));
				this.run = (Timer.Run)xmlSerializer.Deserialize(streamReader);
				streamReader.Close();
			}
			else
			{
				this.run = this.createRun(map);
			}
		}
		catch (Exception ex)
		{
			this.run = this.createRun(map);
			Debug.Log(ex.ToString());
		}
		if (this.run == null)
		{
			Debug.Log("Needs to create a new run");
		}
		else
		{
			if ((this.run.segments.Count != 1 || map.splits.Count != 0) && map.splits.Count != this.run.segments.Count)
			{
				this.run = this.createRun(map);
			}
			if (map.name == "Default")
			{
				if (mods.Count == 0)
				{
					this.run.CategoryName = setting;
				}
				else
				{
					this.run.CategoryName = string.Format("{0}_{1}", setting, string.Join("_", mods.ToArray()));
				}
			}
			else
			{
				this.run.CategoryName = this.runName;
			}
			this.run.metaData.platform.Value = "PC";
			this.run.AttemptHistory = new List<Timer.Run.Attempt>();
			this.run.AttemptCount = 0;
			this.run.version = "1.7.0";
		}
		this.map = map;
		this.mods = mods;
		this.setting = setting;
		this.shiftEnabled = false;
		this.finishedSplits = 0;
		this.finish = false;
		this.enabled = true;
		this.currentSplit = 0;
		this.splitsCurrent = new Dictionary<int, string>();
		this.cacheSegments = new Dictionary<int, string>();
		this.cacheSegmentsHistory = new Dictionary<int, string>();
		this.cacheDeltaGoldSplits = new Dictionary<int, string>();
		this.cacheDeltaRedSplits = new Dictionary<int, string>();
		this.cacheDeltaLightRedSplits = new Dictionary<int, string>();
		this.cacheDeltaGreenSplits = new Dictionary<int, string>();
		this.cacheDeltaLightGreenSplits = new Dictionary<int, string>();
		this.cacheSplits = new Dictionary<int, string>();
		this.cacheSplitsHistory = new Dictionary<int, string>();
		this.cacheTimer = new Dictionary<int, TimeSpan>();
		for (int i = 0; i < this.run.segments.Count; i++)
		{
			try
			{
				if (this.run.segments[i].splits.split.RealTime.Length > 0)
				{
					this.cacheTimer.Add(i, TimeSpan.Parse(this.run.segments[i].splits.split.RealTime));
				}
			}
			catch
			{
			}
		}
		this.updateSOB();
		return true;
	}

	// Token: 0x060019F6 RID: 6646 RVA: 0x000133E7 File Offset: 0x000115E7
	private string getRunName(Map map, string setting, List<string> mods)
	{
		if (mods.Count > 0)
		{
			return string.Format("{0} - {1}_{2}", map.name, setting, string.Join("_", mods.ToArray()));
		}
		return string.Format("{0} - {1}", map.name, setting);
	}

	// Token: 0x060019F7 RID: 6647 RVA: 0x0007DDF8 File Offset: 0x0007BFF8
	private Timer.Run createRun(Map map)
	{
		this.changed = false;
		if (map.splits.Count == 0)
		{
			return new Timer.Run
			{
				segments = 
				{
					new Timer.Run.Segment
					{
						Name = "",
						splits = new Timer.Run.Segment.SplitTimes(),
						best = new Timer.Run.Segment.BestSegmentTime()
					}
				}
			};
		}
		List<Timer.Run.Segment> list = new List<Timer.Run.Segment>();
		foreach (string text in map.splits.Keys)
		{
			list.Add(new Timer.Run.Segment
			{
				Name = text,
				splits = new Timer.Run.Segment.SplitTimes(),
				best = new Timer.Run.Segment.BestSegmentTime()
			});
		}
		return new Timer.Run
		{
			segments = list
		};
	}

	// Token: 0x060019F8 RID: 6648 RVA: 0x0007DED0 File Offset: 0x0007C0D0
	public bool Update(float t, Vector2 player)
	{
		if (this.started && SettingsManager.fastStart)
		{
			t += 6f - SettingsManager.fastStartTime;
		}
		if (this.enabled && !this.stopTimer && this.optionsSelect[0] > 0)
		{
			if (this.map.splits.Count > 0 && this.currentSplit < this.map.splits.Count - 1)
			{
				int num = 0;
				foreach (Rect rect in (SettingsManager.sideWay ? this.map.splitsSideways.Values : this.map.splits.Values))
				{
					if (num < this.currentSplit)
					{
						num++;
					}
					else
					{
						if (rect.Contains(player))
						{
							this.Split(t, num);
							this.currentSplit = num + 1;
							return true;
						}
						num++;
					}
				}
				return true;
			}
			if (this.finish)
			{
				this.officialFinish = PlayerPrefs.GetFloat("LastTime");
				if (SettingsManager.fastStart)
				{
					this.officialFinish += 6f - SettingsManager.fastStartTime;
				}
				if (Math.Abs(this.officialFinish - t) < 1f && this.started)
				{
					t = this.officialFinish;
				}
				else
				{
					this.officialFinish = t;
				}
				this.Split(t, (this.map.splits.Count > 0) ? (this.map.splits.Count - 1) : 0);
				this.stopTimer = true;
				if (this.started)
				{
					try
					{
						if (this.map.splits.Count > 0)
						{
							TimeSpan timeSpan = TimeSpan.Parse(this.splitsCurrent[this.map.splits.Count - 1]);
							if (this.run.segments[this.map.splits.Count - 1].splits.split.RealTime == "" || TimeSpan.Compare(TimeSpan.Parse(this.run.segments[this.map.splits.Count - 1].splits.split.RealTime), timeSpan) == 1)
							{
								this.changed = true;
								this.pb = true;
							}
						}
						else
						{
							TimeSpan timeSpan2 = TimeSpan.Parse(this.splitsCurrent[0]);
							if (this.run.segments[0].splits.split.RealTime == "" || TimeSpan.Compare(TimeSpan.Parse(this.run.segments[0].splits.split.RealTime), timeSpan2) == 1)
							{
								this.changed = true;
								this.pb = true;
							}
						}
					}
					catch
					{
					}
				}
			}
		}
		return true;
	}

	// Token: 0x060019F9 RID: 6649 RVA: 0x0007E210 File Offset: 0x0007C410
	public bool SaveRun()
	{
		if (this.pb)
		{
			this.pb = false;
			foreach (KeyValuePair<int, string> keyValuePair in this.splitsCurrent)
			{
				this.run.segments[keyValuePair.Key].splits.split.RealTime = keyValuePair.Value;
				this.run.segments[keyValuePair.Key].splits.split.GameTime = keyValuePair.Value;
			}
		}
		this.changed = false;
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(Timer.Run));
		this.runName = this.getRunName(this.map, this.setting, this.mods);
		ModPackManager.createDir(string.Format("modpack\\splits\\{0}\\", this.map.name));
		try
		{
			if (!ModPackManager.FileInUse(string.Format("modpack\\splits\\{0}\\{1}.lss", this.map.name, this.runName)))
			{
				TextWriter textWriter = new StreamWriter(string.Format("modpack\\splits\\{0}\\{1}.lss", this.map.name, this.runName));
				xmlSerializer.Serialize(textWriter, this.run);
				textWriter.Close();
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		Debug.Log(string.Format("Failed to save splits for {0}", this.runName));
		return false;
	}

	// Token: 0x060019FA RID: 6650 RVA: 0x00013425 File Offset: 0x00011625
	public bool FinishRun()
	{
		if (this.enabled && this.optionsSelect[0] > 0)
		{
			this.finish = true;
		}
		return true;
	}

	// Token: 0x060019FB RID: 6651 RVA: 0x0007E3A8 File Offset: 0x0007C5A8
	private bool Split(float t, int _split)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)t);
		string text = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
		this.splitsCurrent[_split] = text;
		int finishedSplits = this.finishedSplits;
		this.finishedSplits = finishedSplits + 1;
		if (_split == 0 || this.splitsCurrent.ContainsKey(_split - 1))
		{
			if (this.run.segments[_split].best.RealTime == "")
			{
				if (_split > 0)
				{
					try
					{
						TimeSpan timeSpan2 = TimeSpan.Parse(this.splitsCurrent[_split - 1]);
						TimeSpan timeSpan3 = timeSpan - timeSpan2;
						text = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", new object[] { timeSpan3.Hours, timeSpan3.Minutes, timeSpan3.Seconds, timeSpan3.Milliseconds });
						this.run.segments[_split].best.RealTime = text;
						this.run.segments[_split].best.GameTime = text;
						this.updateSOB();
						this.changed = true;
						return true;
					}
					catch
					{
						return true;
					}
				}
				if (this.started)
				{
					this.run.segments[_split].best.RealTime = text;
					this.run.segments[_split].best.GameTime = text;
					this.updateSOB();
					this.changed = true;
				}
			}
			else
			{
				try
				{
					if (_split > 0)
					{
						TimeSpan timeSpan4 = TimeSpan.Parse(this.splitsCurrent[_split - 1]);
						TimeSpan timeSpan5 = timeSpan - timeSpan4;
						if (TimeSpan.Compare(TimeSpan.Parse(this.run.segments[_split].best.RealTime), timeSpan5) == 1)
						{
							text = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", new object[] { timeSpan5.Hours, timeSpan5.Minutes, timeSpan5.Seconds, timeSpan5.Milliseconds });
							this.run.segments[_split].best.RealTime = text;
							this.run.segments[_split].best.GameTime = text;
							this.updateSOB();
							this.changed = true;
						}
					}
					else if (this.started && TimeSpan.Compare(TimeSpan.Parse(this.run.segments[_split].best.RealTime), timeSpan) == 1)
					{
						this.run.segments[_split].best.RealTime = text;
						this.run.segments[_split].best.GameTime = text;
						this.updateSOB();
						this.changed = true;
					}
				}
				catch
				{
				}
			}
		}
		return true;
	}

	// Token: 0x17000700 RID: 1792
	// (get) Token: 0x060019FC RID: 6652 RVA: 0x00013446 File Offset: 0x00011646
	// (set) Token: 0x060019FD RID: 6653 RVA: 0x0001344E File Offset: 0x0001164E
	public bool enabled { get; set; }

	// Token: 0x17000701 RID: 1793
	// (get) Token: 0x060019FE RID: 6654 RVA: 0x00013457 File Offset: 0x00011657
	// (set) Token: 0x060019FF RID: 6655 RVA: 0x0001345F File Offset: 0x0001165F
	public bool changed { get; set; }

	// Token: 0x17000702 RID: 1794
	// (get) Token: 0x06001A00 RID: 6656 RVA: 0x00013468 File Offset: 0x00011668
	// (set) Token: 0x06001A01 RID: 6657 RVA: 0x00013470 File Offset: 0x00011670
	private bool displayTimer { get; set; }

	// Token: 0x17000703 RID: 1795
	// (get) Token: 0x06001A02 RID: 6658 RVA: 0x00013479 File Offset: 0x00011679
	// (set) Token: 0x06001A03 RID: 6659 RVA: 0x00013481 File Offset: 0x00011681
	private int currentSplit { get; set; }

	// Token: 0x17000704 RID: 1796
	// (get) Token: 0x06001A04 RID: 6660 RVA: 0x0001348A File Offset: 0x0001168A
	// (set) Token: 0x06001A05 RID: 6661 RVA: 0x00013492 File Offset: 0x00011692
	private int finishedSplits { get; set; }

	// Token: 0x17000705 RID: 1797
	// (get) Token: 0x06001A06 RID: 6662 RVA: 0x0001349B File Offset: 0x0001169B
	// (set) Token: 0x06001A07 RID: 6663 RVA: 0x000134A3 File Offset: 0x000116A3
	private Dictionary<int, string> splitsCurrent { get; set; }

	// Token: 0x17000706 RID: 1798
	// (get) Token: 0x06001A08 RID: 6664 RVA: 0x000134AC File Offset: 0x000116AC
	// (set) Token: 0x06001A09 RID: 6665 RVA: 0x000134B4 File Offset: 0x000116B4
	private bool finish { get; set; }

	// Token: 0x17000707 RID: 1799
	// (get) Token: 0x06001A0A RID: 6666 RVA: 0x000134BD File Offset: 0x000116BD
	// (set) Token: 0x06001A0B RID: 6667 RVA: 0x000134C5 File Offset: 0x000116C5
	public bool pb { get; set; }

	// Token: 0x17000708 RID: 1800
	// (get) Token: 0x06001A0C RID: 6668 RVA: 0x000134CE File Offset: 0x000116CE
	// (set) Token: 0x06001A0D RID: 6669 RVA: 0x000134D6 File Offset: 0x000116D6
	public bool stopTimer { get; set; }

	// Token: 0x06001A0E RID: 6670 RVA: 0x0007E724 File Offset: 0x0007C924
	private string convertTime(string s)
	{
		string text2;
		try
		{
			TimeSpan timeSpan = TimeSpan.Parse(s);
			string text;
			if (timeSpan.Hours > 0)
			{
				text = "{0:D1}:{1:D1}:{2:D2}.{3:D3}";
			}
			else
			{
				text = "{1:D1}:{2:D2}.{3:D3}";
			}
			text2 = string.Format(text, new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
		}
		catch
		{
			text2 = s;
		}
		return text2;
	}

	// Token: 0x17000709 RID: 1801
	// (get) Token: 0x06001A0F RID: 6671 RVA: 0x000134DF File Offset: 0x000116DF
	// (set) Token: 0x06001A10 RID: 6672 RVA: 0x000134E7 File Offset: 0x000116E7
	public bool started { get; set; }

	// Token: 0x06001A11 RID: 6673 RVA: 0x0007E7B4 File Offset: 0x0007C9B4
	private string convertDiffTime(string s1, string s2)
	{
		string text2;
		try
		{
			TimeSpan timeSpan = TimeSpan.Parse(s1) - TimeSpan.Parse(s2);
			string text;
			if (timeSpan.Hours > 0)
			{
				text = "{0:D1}:{1:D1}:{2:D2}.{3:D3}";
			}
			else
			{
				text = "{1:D1}:{2:D2}.{3:D3}";
			}
			text2 = string.Format(text, new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
		}
		catch
		{
			text2 = "";
		}
		return text2;
	}

	// Token: 0x06001A12 RID: 6674 RVA: 0x0007E850 File Offset: 0x0007CA50
	private string convertDeltaTime(TimeSpan t, TimeSpan t2)
	{
		string text2;
		try
		{
			if (TimeSpan.Compare(t, t2) == -1)
			{
				TimeSpan timeSpan = t2 - t;
				string text;
				if (timeSpan.Hours > 0)
				{
					text = "-{0:D1}:{1:D1}:{2:D2}";
				}
				else if (timeSpan.Minutes > 0)
				{
					text = "-{1:D1}:{2:D2}";
				}
				else
				{
					text = "-{2:D1}.{3:D3}";
				}
				text2 = string.Format(text, new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
			}
			else
			{
				TimeSpan timeSpan2 = t - t2;
				string text3;
				if (timeSpan2.Hours > 0)
				{
					text3 = "+{0:D1}:{1:D1}:{2:D2}";
				}
				else if (timeSpan2.Minutes > 0)
				{
					text3 = "+{1:D1}:{2:D2}";
				}
				else
				{
					text3 = "+{2:D1}.{3:D3}";
				}
				text2 = string.Format(text3, new object[] { timeSpan2.Hours, timeSpan2.Minutes, timeSpan2.Seconds, timeSpan2.Milliseconds });
			}
		}
		catch
		{
			text2 = "";
		}
		return text2;
	}

	// Token: 0x06001A13 RID: 6675 RVA: 0x0007E98C File Offset: 0x0007CB8C
	public void Display(float t = -1f)
	{
		if (t != -1f)
		{
			if (this.stopTimer)
			{
				t = this.officialFinish;
			}
			else if (this.started && SettingsManager.fastStart)
			{
				t += 6f - SettingsManager.fastStartTime;
			}
		}
		if (this.screenHeight != Screen.height)
		{
			this.width = Math.Max(1280f, (float)Screen.width);
			this.height = Math.Max(720f, (float)Screen.height);
			this.timerWidth = this.width * 0.37f / (this.width / this.height);
			this.timeWidth = this.width * 0.0693f / (this.width / this.height);
			this.deltaWidth = this.width * 0.0649f / (this.width / this.height);
			this.segmentwidth = this.timerWidth - this.timeWidth * 2f - this.deltaWidth - 30f;
			this.timerSize = new Vector2(this.timerWidth, this.height * 0.9f);
		}
		try
		{
			if (Timer.backgroundStyle == null || this.screenHeight != Screen.height)
			{
				Timer.backgroundStyle = new GUIStyle(GUI.skin.box)
				{
					normal = 
					{
						background = Texture2D.blackTexture
					}
				};
				Timer.timerTitleStyle = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.MiddleCenter,
					fontStyle = FontStyle.Bold,
					normal = 
					{
						textColor = Color.white
					},
					wordWrap = false
				};
				Timer.leftAlignedLabel = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperLeft,
					fontSize = (int)(this.height * 0.018f),
					wordWrap = false
				};
				Timer.rightAlignedLabel = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					fontSize = (int)(this.height * 0.016f),
					wordWrap = false
				};
				Timer.rightAlignedYellowLabel = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					normal = 
					{
						textColor = new Color(0.8f, 0.65f, 0f, 1f)
					},
					fontSize = (int)(this.height * 0.016f),
					wordWrap = false
				};
				Timer.rightAlignedRedLabel = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					normal = 
					{
						textColor = new Color(0.99f, 0.2f, 0.2f)
					},
					fontSize = (int)(this.height * 0.016f),
					wordWrap = false
				};
				Timer.rightAlignedLightRedLabel = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					normal = 
					{
						textColor = new Color(0.8f, 0.45f, 0.45f, 1f)
					},
					fontSize = (int)(this.height * 0.016f),
					wordWrap = false
				};
				Timer.rightAlignedGreenLabel = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					normal = 
					{
						textColor = new Color(0f, 0.75f, 0f, 1f)
					},
					fontSize = (int)(this.height * 0.016f),
					wordWrap = false
				};
				Timer.rightAlignedLightGreenLabel = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					normal = 
					{
						textColor = Color.green
					},
					fontSize = (int)(this.height * 0.016f),
					wordWrap = false
				};
				Timer.rowStyle = new GUIStyle
				{
					normal = 
					{
						background = Texture2D.whiteTexture
					},
					fontSize = (int)(this.height * 0.016f),
					wordWrap = false
				};
				Timer.whiteTimer = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					fontSize = (int)(this.height * 0.07f),
					normal = 
					{
						textColor = Color.white
					},
					wordWrap = false
				};
				Timer.redTimer = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					fontSize = (int)(this.height * 0.07f),
					normal = 
					{
						textColor = new Color(0.99f, 0.2f, 0.2f)
					},
					wordWrap = false
				};
				Timer.greenTimer = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperRight,
					fontSize = (int)(this.height * 0.07f),
					normal = 
					{
						textColor = Color.green
					},
					wordWrap = false
				};
				Timer.whiteTimerLeft = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperLeft,
					fontSize = (int)(this.height * 0.07f),
					normal = 
					{
						textColor = Color.white
					},
					wordWrap = false
				};
				Timer.redTimerLeft = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperLeft,
					fontSize = (int)(this.height * 0.07f),
					normal = 
					{
						textColor = new Color(0.99f, 0.2f, 0.2f)
					},
					wordWrap = false
				};
				Timer.greenTimerLeft = new GUIStyle(GUI.skin.label)
				{
					alignment = TextAnchor.UpperLeft,
					fontSize = (int)(this.height * 0.07f),
					normal = 
					{
						textColor = Color.green
					},
					wordWrap = false
				};
				this.screenHeight = Screen.height;
			}
		}
		catch
		{
		}
		if (this.optionsSelect[0] >= 1 && (t == -1f || (this.enabled && (this.optionsSelect[0] == 2 || this.shiftEnabled))))
		{
			if (this.run != null)
			{
				try
				{
					Color backgroundColor = GUI.backgroundColor;
					Color textColor = GUI.skin.label.normal.textColor;
					GUI.backgroundColor = new Color(0f, 0f, 0f, 0.95f);
					GUILayout.BeginArea(new Rect((this.optionsSelect[1] == 0) ? (-4f) : ((float)Screen.width - this.timerSize.x + ((this.optionsSelect[5] == 0) ? 0f : this.timeWidth)), -6f, this.timerSize.x - ((this.optionsSelect[5] == 0) ? 0f : this.timeWidth), this.timerSize.y), Timer.backgroundStyle);
					GUILayout.BeginHorizontal(Timer.rowStyle, new GUILayoutOption[] { GUILayout.Width(this.timerSize.x - ((this.optionsSelect[5] == 0) ? 0f : this.timeWidth)) });
					GUILayout.Label("", new GUILayoutOption[] { GUILayout.Width(this.segmentwidth) });
					if (this.optionsSelect[5] == 0)
					{
						GUILayout.Label("Segment", Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
					}
					GUILayout.Label("+/-", Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.deltaWidth) });
					GUILayout.Label("Time", Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
					GUILayout.Space(10f);
					GUILayout.EndHorizontal();
					GUILayout.Space(-1f);
					for (int i = 0; i < this.run.segments.Count; i++)
					{
						GUILayout.Space(-1f);
						this.DrawRow(i, t == -1f);
						GUILayout.Space(-1f);
					}
					GUI.backgroundColor = new Color(0f, 0f, 0f, 0.75f);
					GUILayout.BeginHorizontal(Timer.rowStyle, new GUILayoutOption[] { GUILayout.Width(this.timerSize.x - ((this.optionsSelect[5] == 0) ? 0f : this.timeWidth)) });
					GUILayout.Label("Sum of Best Segments", Timer.leftAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timerSize.x - this.timeWidth - 25f - ((this.optionsSelect[5] == 0) ? 0f : this.timeWidth)) });
					GUILayout.FlexibleSpace();
					GUILayout.Label(this.cacheSOB, Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth - ((this.optionsSelect[5] == 0) ? 0f : this.timeWidth)) });
					GUILayout.Space(10f);
					GUILayout.EndHorizontal();
					if (t != -1f)
					{
						TimeSpan timeSpan = TimeSpan.FromSeconds((double)t);
						string text;
						if (timeSpan.Hours > 0)
						{
							if (this.optionsSelect[5] == 1)
							{
								text = string.Format("{0:D1}:{1:D2}:{2:D2}", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
							}
							else if (timeSpan.Hours >= 10)
							{
								text = string.Format("{0:D1}:{1:D2}:{2:D2}.{3}", new object[]
								{
									timeSpan.Hours,
									timeSpan.Minutes,
									timeSpan.Seconds,
									string.Format("{0:D3}", timeSpan.Milliseconds).Substring(0, 1)
								});
							}
							else
							{
								text = string.Format("{0:D1}:{1:D2}:{2:D2}.{3}", new object[]
								{
									timeSpan.Hours,
									timeSpan.Minutes,
									timeSpan.Seconds,
									string.Format("{0:D3}", timeSpan.Milliseconds).Substring(0, 2)
								});
							}
						}
						else if (timeSpan.Minutes >= 10 && this.optionsSelect[5] == 1)
						{
							text = string.Format("{1:D1}:{2:D2}.{3}", new object[]
							{
								timeSpan.Hours,
								timeSpan.Minutes,
								timeSpan.Seconds,
								string.Format("{0:D3}", timeSpan.Milliseconds).Substring(0, 2)
							});
						}
						else if (timeSpan.Minutes > 0)
						{
							text = "{1:D1}:{2:D2}.{3:D3}";
							text = string.Format(text, new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
						}
						else
						{
							text = "{2:D1}.{3:D3}";
							text = string.Format(text, new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
						}
						if (this.cacheTimer.ContainsKey(this.currentSplit) && (this.optionsSelect[0] == 2 || this.shiftEnabled || this.optionsSelect[4] == 0))
						{
							if (TimeSpan.Compare(this.cacheTimer[this.currentSplit], timeSpan) == -1)
							{
								GUILayout.Label(text, (this.optionsSelect[1] != 2) ? Timer.redTimer : Timer.redTimerLeft, new GUILayoutOption[0]);
							}
							else
							{
								GUILayout.Label(text, (this.optionsSelect[1] != 2) ? Timer.greenTimer : Timer.greenTimerLeft, new GUILayoutOption[0]);
							}
						}
						else
						{
							GUILayout.Label(text, (this.optionsSelect[1] != 2) ? Timer.whiteTimer : Timer.whiteTimerLeft, new GUILayoutOption[0]);
						}
					}
					GUILayout.EndArea();
					GUI.backgroundColor = backgroundColor;
				}
				catch (Exception ex)
				{
					Debug.Log(ex.ToString());
				}
			}
			return;
		}
		if (this.run != null && t != -1f)
		{
			TimeSpan timeSpan2 = TimeSpan.FromSeconds((double)t);
			string text2;
			if (timeSpan2.Hours > 0)
			{
				text2 = "{0:D2}h:{1:D2}m:{2:D2}.{3:D3}s";
			}
			else if (timeSpan2.Minutes > 0)
			{
				text2 = "{1:D2}m:{2:D2}.{3:D3}s";
			}
			else
			{
				text2 = "{2:D2}.{3:D3}s";
			}
			text2 = string.Format(text2, new object[] { timeSpan2.Hours, timeSpan2.Minutes, timeSpan2.Seconds, timeSpan2.Milliseconds });
			if (this.optionsSelect[0] == 0 || !this.enabled || !this.cacheTimer.ContainsKey(this.currentSplit) || this.optionsSelect[4] != 0)
			{
				GUI.Label(new Rect((float)Screen.width - Timer.whiteTimer.CalcSize(new GUIContent(text2)).x, 5f, Timer.whiteTimer.CalcSize(new GUIContent(text2)).x, Timer.whiteTimer.CalcSize(new GUIContent(text2)).y), text2, Timer.whiteTimer);
				return;
			}
			if (TimeSpan.Compare(this.cacheTimer[this.currentSplit], timeSpan2) == -1)
			{
				GUI.Label(new Rect((float)Screen.width - Timer.redTimer.CalcSize(new GUIContent(text2)).x, 5f, Timer.redTimer.CalcSize(new GUIContent(text2)).x, Timer.redTimer.CalcSize(new GUIContent(text2)).y), text2, Timer.redTimer);
				return;
			}
			GUI.Label(new Rect((float)Screen.width - Timer.greenTimer.CalcSize(new GUIContent(text2)).x, 5f, Timer.greenTimer.CalcSize(new GUIContent(text2)).x, Timer.greenTimer.CalcSize(new GUIContent(text2)).y), text2, Timer.greenTimer);
		}
	}

	// Token: 0x06001A14 RID: 6676 RVA: 0x0007F80C File Offset: 0x0007DA0C
	private bool updateSOB()
	{
		if (this.run != null)
		{
			TimeSpan timeSpan = default(TimeSpan);
			foreach (Timer.Run.Segment segment in this.run.segments)
			{
				try
				{
					if (segment.best.RealTime.Length > 0)
					{
						timeSpan += TimeSpan.Parse(segment.best.RealTime);
					}
					else if (segment.best.GameTime.Length > 0)
					{
						timeSpan += TimeSpan.Parse(segment.best.GameTime);
					}
				}
				catch
				{
				}
			}
			if (timeSpan.TotalSeconds > 0.0)
			{
				try
				{
					string text;
					if (timeSpan.Hours > 0)
					{
						text = "{0:D1}:{1:D1}:{2:D2}.{3:D3}";
					}
					else
					{
						text = "{1:D1}:{2:D2}.{3:D3}";
					}
					this.cacheSOB = string.Format(text, new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
					return true;
				}
				catch
				{
					return true;
				}
			}
			this.cacheSOB = "---";
		}
		return true;
	}

	// Token: 0x06001A15 RID: 6677 RVA: 0x000134F0 File Offset: 0x000116F0
	public void resetCache()
	{
		this.cacheSegmentsHistory = new Dictionary<int, string>();
	}

	// Token: 0x06001A16 RID: 6678 RVA: 0x0007F978 File Offset: 0x0007DB78
	private void DrawRow(int index, bool menu = false)
	{
		float num = ((index % 2 == 0) ? 0.1f : 0.2f);
		GUI.backgroundColor = new Color(num, num, num, 0.75f);
		GUILayout.BeginHorizontal(Timer.rowStyle, new GUILayoutOption[] { GUILayout.Width(this.timerSize.x - ((this.optionsSelect[5] == 0) ? 0f : this.timeWidth)) });
		GUILayout.Label((this.run.segments[index].Name.Length > 16) ? string.Format("{0}...", this.run.segments[index].Name.Substring(0, 13)) : this.run.segments[index].Name, Timer.leftAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.segmentwidth) });
		GUILayout.FlexibleSpace();
		if (((this.started && index == 0) || index > 0) && (index < this.currentSplit || this.stopTimer) && this.splitsCurrent.ContainsKey(index) && this.run.segments[index].best.RealTime.Length > 0)
		{
			try
			{
				if (!this.cacheDeltaGoldSplits.ContainsKey(index))
				{
					if (index == 0)
					{
						TimeSpan timeSpan = TimeSpan.Parse(this.splitsCurrent[index]);
						TimeSpan timeSpan2 = TimeSpan.Parse(this.run.segments[index].best.RealTime);
						if (TimeSpan.Compare(timeSpan, timeSpan2) <= 0)
						{
							this.cacheDeltaGoldSplits.Add(index, this.convertDeltaTime(timeSpan, timeSpan2));
						}
					}
					else if (index > 0 && this.splitsCurrent.ContainsKey(index - 1))
					{
						TimeSpan timeSpan3 = TimeSpan.Parse(this.splitsCurrent[index]);
						TimeSpan timeSpan4 = TimeSpan.Parse(this.splitsCurrent[index - 1]);
						TimeSpan timeSpan5 = TimeSpan.Parse(this.run.segments[index].best.RealTime);
						if (TimeSpan.Compare(timeSpan3 - timeSpan4, timeSpan5) <= 0)
						{
							this.cacheDeltaGoldSplits.Add(index, this.convertDeltaTime(timeSpan3 - timeSpan4, timeSpan5));
						}
					}
				}
			}
			catch
			{
			}
		}
		if (this.optionsSelect[5] == 0)
		{
			if (index == 0 && this.splitsCurrent.ContainsKey(0))
			{
				if (!this.cacheSegments.ContainsKey(index))
				{
					this.cacheSegments.Add(index, this.convertTime(this.splitsCurrent[index]));
				}
				GUILayout.Label(this.cacheSegments[index], (this.cacheDeltaGoldSplits.ContainsKey(index) && (this.optionsSelect[6] == 0 || this.optionsSelect[6] == 2)) ? Timer.rightAlignedYellowLabel : Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
			}
			else if (index > 0 && this.splitsCurrent.ContainsKey(index - 1) && this.splitsCurrent.ContainsKey(index))
			{
				if (!this.cacheSegments.ContainsKey(index))
				{
					this.cacheSegments.Add(index, this.convertDiffTime(this.splitsCurrent[index], this.splitsCurrent[index - 1]));
				}
				GUILayout.Label(this.cacheSegments[index], (this.cacheDeltaGoldSplits.ContainsKey(index) && (this.optionsSelect[6] == 0 || this.optionsSelect[6] == 2)) ? Timer.rightAlignedYellowLabel : Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
			}
			else if (this.currentSplit > index)
			{
				GUILayout.Label("---", Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
			}
			else if (index == 0 && this.run.segments[index].splits.split.RealTime.Length > 0)
			{
				if (!this.cacheSegmentsHistory.ContainsKey(index))
				{
					this.cacheSegmentsHistory.Add(index, this.convertTime((this.optionsSelect[2] == 0) ? this.run.segments[index].splits.split.RealTime : this.run.segments[index].best.RealTime));
				}
				GUILayout.Label(this.cacheSegmentsHistory[index], (this.cacheDeltaGoldSplits.ContainsKey(index) && (this.optionsSelect[6] == 0 || this.optionsSelect[6] == 2)) ? Timer.rightAlignedYellowLabel : Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
			}
			else if (index > 0 && this.run.segments[index].splits.split.RealTime.Length > 0 && this.run.segments[index - 1].splits.split.RealTime.Length > 0)
			{
				if (!this.cacheSegmentsHistory.ContainsKey(index))
				{
					this.cacheSegmentsHistory.Add(index, (this.optionsSelect[2] == 0) ? this.convertDiffTime(this.run.segments[index].splits.split.RealTime, this.run.segments[index - 1].splits.split.RealTime) : this.convertTime(this.run.segments[index].best.RealTime));
				}
				GUILayout.Label(this.cacheSegmentsHistory[index], (this.cacheDeltaGoldSplits.ContainsKey(index) && (this.optionsSelect[6] == 0 || this.optionsSelect[6] == 2)) ? Timer.rightAlignedYellowLabel : Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
			}
			else
			{
				GUILayout.Label("", Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
			}
		}
		if (this.started && (index < this.currentSplit || this.stopTimer) && this.splitsCurrent.ContainsKey(index) && this.run.segments[index].splits.split.RealTime.Length > 0)
		{
			try
			{
				if (!this.cacheDeltaRedSplits.ContainsKey(index) && !this.cacheDeltaGreenSplits.ContainsKey(index) && !this.cacheDeltaLightRedSplits.ContainsKey(index) && !this.cacheDeltaLightGreenSplits.ContainsKey(index))
				{
					TimeSpan timeSpan6 = TimeSpan.Parse(this.splitsCurrent[index]);
					TimeSpan timeSpan7 = TimeSpan.Parse(this.run.segments[index].splits.split.RealTime);
					TimeSpan timeSpan8 = timeSpan7;
					TimeSpan timeSpan9 = timeSpan6;
					bool flag = false;
					if (index > 0 && this.run.segments[index - 1].splits.split.RealTime.Length > 0 && this.splitsCurrent.ContainsKey(index - 1))
					{
						timeSpan8 = TimeSpan.Parse(this.run.segments[index - 1].splits.split.RealTime);
						timeSpan9 = TimeSpan.Parse(this.splitsCurrent[index - 1]);
						flag = true;
					}
					if (TimeSpan.Compare(timeSpan6, timeSpan7) <= 0)
					{
						if (flag && TimeSpan.Compare(timeSpan6 - timeSpan9, timeSpan7 - timeSpan8) > 0)
						{
							this.cacheDeltaLightGreenSplits.Add(index, this.convertDeltaTime(timeSpan6, timeSpan7));
						}
						else
						{
							this.cacheDeltaGreenSplits.Add(index, this.convertDeltaTime(timeSpan6, timeSpan7));
						}
					}
					else if (flag && TimeSpan.Compare(timeSpan6 - timeSpan9, timeSpan7 - timeSpan8) < 0)
					{
						this.cacheDeltaLightRedSplits.Add(index, this.convertDeltaTime(timeSpan6, timeSpan7));
					}
					else
					{
						this.cacheDeltaRedSplits.Add(index, this.convertDeltaTime(timeSpan6, timeSpan7));
					}
				}
				if (this.cacheDeltaGreenSplits.ContainsKey(index))
				{
					GUILayout.Label(this.cacheDeltaGreenSplits[index], (this.cacheDeltaGoldSplits.ContainsKey(index) && (this.optionsSelect[6] == 1 || this.optionsSelect[6] == 2)) ? Timer.rightAlignedYellowLabel : Timer.rightAlignedGreenLabel, new GUILayoutOption[] { GUILayout.Width(this.deltaWidth) });
				}
				else if (this.cacheDeltaRedSplits.ContainsKey(index))
				{
					GUILayout.Label(this.cacheDeltaRedSplits[index], (this.cacheDeltaGoldSplits.ContainsKey(index) && (this.optionsSelect[6] == 1 || this.optionsSelect[6] == 2)) ? Timer.rightAlignedYellowLabel : Timer.rightAlignedRedLabel, new GUILayoutOption[] { GUILayout.Width(this.deltaWidth) });
				}
				else if (this.cacheDeltaLightRedSplits.ContainsKey(index))
				{
					GUILayout.Label(this.cacheDeltaLightRedSplits[index], (this.cacheDeltaGoldSplits.ContainsKey(index) && (this.optionsSelect[6] == 1 || this.optionsSelect[6] == 2)) ? Timer.rightAlignedYellowLabel : ((this.optionsSelect[7] == 1) ? Timer.rightAlignedLightRedLabel : Timer.rightAlignedRedLabel), new GUILayoutOption[] { GUILayout.Width(this.deltaWidth) });
				}
				else if (this.cacheDeltaLightGreenSplits.ContainsKey(index))
				{
					GUILayout.Label(this.cacheDeltaLightGreenSplits[index], (this.cacheDeltaGoldSplits.ContainsKey(index) && (this.optionsSelect[6] == 1 || this.optionsSelect[6] == 2)) ? Timer.rightAlignedYellowLabel : ((this.optionsSelect[7] == 1) ? Timer.rightAlignedLightGreenLabel : Timer.rightAlignedGreenLabel), new GUILayoutOption[] { GUILayout.Width(this.deltaWidth) });
				}
				else
				{
					GUILayout.Label("", Timer.rightAlignedRedLabel, new GUILayoutOption[] { GUILayout.Width(this.deltaWidth) });
				}
				goto IL_0AC5;
			}
			catch (Exception ex)
			{
				Debug.Log(ex.ToString());
				GUILayout.Label("", Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.deltaWidth) });
				goto IL_0AC5;
			}
		}
		GUILayout.Label("", Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.deltaWidth) });
		IL_0AC5:
		if (index < this.currentSplit || this.stopTimer)
		{
			if (!this.cacheSplits.ContainsKey(index))
			{
				this.cacheSplits.Add(index, this.splitsCurrent.ContainsKey(index) ? this.convertTime(this.splitsCurrent[index]) : "???");
			}
			GUILayout.Label(this.cacheSplits[index], Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
		}
		else if (this.started || menu)
		{
			if (!this.cacheSplitsHistory.ContainsKey(index))
			{
				this.cacheSplitsHistory.Add(index, (this.run.segments[index].splits.split.RealTime.Length == 0) ? "---" : this.convertTime(this.run.segments[index].splits.split.RealTime));
			}
			GUILayout.Label(this.cacheSplitsHistory[index], Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
		}
		else
		{
			GUILayout.Label("---", Timer.rightAlignedLabel, new GUILayoutOption[] { GUILayout.Width(this.timeWidth) });
		}
		GUILayout.Space(10f);
		GUILayout.EndHorizontal();
	}

	// Token: 0x06001A17 RID: 6679 RVA: 0x000805D8 File Offset: 0x0007E7D8
	public void drawSplits()
	{
		using (this.splitsCurrent.GetEnumerator())
		{
			foreach (KeyValuePair<string, Rect> keyValuePair in (SettingsManager.sideWay ? this.map.splitsSideways : this.map.splits))
			{
				MapTools.CreateBox(keyValuePair.Key, keyValuePair.Value);
			}
		}
	}

	// Token: 0x04001199 RID: 4505
	private Map map;

	// Token: 0x0400119A RID: 4506
	private string setting;

	// Token: 0x0400119B RID: 4507
	private List<string> mods;

	// Token: 0x0400119C RID: 4508
	private Vector2 timerSize;

	// Token: 0x0400119D RID: 4509
	private static GUIStyle backgroundStyle;

	// Token: 0x0400119E RID: 4510
	private static GUIStyle timerTitleStyle;

	// Token: 0x0400119F RID: 4511
	private static GUIStyle rightAlignedLabel;

	// Token: 0x040011A0 RID: 4512
	private static GUIStyle rowStyle;

	// Token: 0x040011A1 RID: 4513
	private static GUIStyle rightAlignedRedLabel;

	// Token: 0x040011A2 RID: 4514
	private static GUIStyle rightAlignedGreenLabel;

	// Token: 0x040011A3 RID: 4515
	private static GUIStyle rightAlignedYellowLabel;

	// Token: 0x040011A4 RID: 4516
	private static GUIStyle leftAlignedLabel;

	// Token: 0x040011A5 RID: 4517
	private float timeWidth;

	// Token: 0x040011A6 RID: 4518
	private float deltaWidth;

	// Token: 0x040011A7 RID: 4519
	private float segmentwidth;

	// Token: 0x040011A8 RID: 4520
	private float timerWidth;

	// Token: 0x040011A9 RID: 4521
	private int screenHeight;

	// Token: 0x040011AA RID: 4522
	private float height;

	// Token: 0x040011AB RID: 4523
	private float width;

	// Token: 0x040011AC RID: 4524
	private Dictionary<int, string> cacheSegments;

	// Token: 0x040011AD RID: 4525
	private Dictionary<int, string> cacheSegmentsHistory;

	// Token: 0x040011AE RID: 4526
	private Dictionary<int, string> cacheDeltaGoldSplits;

	// Token: 0x040011AF RID: 4527
	private Dictionary<int, string> cacheDeltaRedSplits;

	// Token: 0x040011B0 RID: 4528
	private Dictionary<int, string> cacheDeltaGreenSplits;

	// Token: 0x040011B1 RID: 4529
	private Dictionary<int, string> cacheSplits;

	// Token: 0x040011B2 RID: 4530
	private Dictionary<int, string> cacheSplitsHistory;

	// Token: 0x040011B3 RID: 4531
	public string cacheSOB;

	// Token: 0x040011B4 RID: 4532
	private static GUIStyle redTimer;

	// Token: 0x040011B5 RID: 4533
	private static GUIStyle greenTimer;

	// Token: 0x040011B6 RID: 4534
	private static GUIStyle whiteTimer;

	// Token: 0x040011B7 RID: 4535
	private Dictionary<int, TimeSpan> cacheTimer;

	// Token: 0x040011B8 RID: 4536
	public Dictionary<int, List<string>> options;

	// Token: 0x040011B9 RID: 4537
	public Dictionary<int, int> optionsSelect;

	// Token: 0x040011BA RID: 4538
	public Dictionary<int, string> optionsTitle;

	// Token: 0x040011BB RID: 4539
	private static GUIStyle redTimerLeft;

	// Token: 0x040011BC RID: 4540
	private static GUIStyle greenTimerLeft;

	// Token: 0x040011BD RID: 4541
	private static GUIStyle whiteTimerLeft;

	// Token: 0x040011BE RID: 4542
	public bool debug;

	// Token: 0x040011BF RID: 4543
	public bool shiftEnabled;

	// Token: 0x040011C0 RID: 4544
	public float officialFinish;

	// Token: 0x040011C1 RID: 4545
	private static GUIStyle rightAlignedLightRedLabel;

	// Token: 0x040011C2 RID: 4546
	private static GUIStyle rightAlignedLightGreenLabel;

	// Token: 0x040011C3 RID: 4547
	private Dictionary<int, string> cacheDeltaLightRedSplits;

	// Token: 0x040011C4 RID: 4548
	private Dictionary<int, string> cacheDeltaLightGreenSplits;

	// Token: 0x020002FC RID: 764
	public class MetaData
	{
		// Token: 0x1700070A RID: 1802
		// (get) Token: 0x06001A18 RID: 6680 RVA: 0x000134FD File Offset: 0x000116FD
		// (set) Token: 0x06001A19 RID: 6681 RVA: 0x00013505 File Offset: 0x00011705
		[XmlElement("Platform")]
		public Timer.MetaData.Platform platform { get; set; }

		// Token: 0x1700070B RID: 1803
		// (get) Token: 0x06001A1A RID: 6682 RVA: 0x0001350E File Offset: 0x0001170E
		// (set) Token: 0x06001A1B RID: 6683 RVA: 0x00013516 File Offset: 0x00011716
		public string Region { get; set; }

		// Token: 0x1700070C RID: 1804
		// (get) Token: 0x06001A1C RID: 6684 RVA: 0x0001351F File Offset: 0x0001171F
		// (set) Token: 0x06001A1D RID: 6685 RVA: 0x00013527 File Offset: 0x00011727
		public string Variables { get; set; }

		// Token: 0x06001A1E RID: 6686 RVA: 0x00013530 File Offset: 0x00011730
		public MetaData()
		{
			this.platform = new Timer.MetaData.Platform();
			this.Region = "";
			this.Variables = "";
		}

		// Token: 0x1700070D RID: 1805
		// (get) Token: 0x06001A1F RID: 6687 RVA: 0x00013564 File Offset: 0x00011764
		// (set) Token: 0x06001A20 RID: 6688 RVA: 0x0001356C File Offset: 0x0001176C
		[XmlElement("Run")]
		public Timer.MetaData.Run run { get; set; } = new Timer.MetaData.Run();

		// Token: 0x020002FD RID: 765
		public class Platform
		{
			// Token: 0x1700070E RID: 1806
			// (get) Token: 0x06001A21 RID: 6689 RVA: 0x00013575 File Offset: 0x00011775
			// (set) Token: 0x06001A22 RID: 6690 RVA: 0x0001357D File Offset: 0x0001177D
			[XmlAttribute("usesEmulator")]
			public string emulator { get; set; } = "False";

			// Token: 0x06001A23 RID: 6691 RVA: 0x00013586 File Offset: 0x00011786
			public Platform()
			{
				this.Value = "";
			}

			// Token: 0x1700070F RID: 1807
			// (get) Token: 0x06001A24 RID: 6692 RVA: 0x000135A4 File Offset: 0x000117A4
			// (set) Token: 0x06001A25 RID: 6693 RVA: 0x000135AC File Offset: 0x000117AC
			[XmlText]
			public string Value { get; set; }
		}

		// Token: 0x020002FE RID: 766
		public class Run
		{
			// Token: 0x17000710 RID: 1808
			// (get) Token: 0x06001A26 RID: 6694 RVA: 0x000135B5 File Offset: 0x000117B5
			// (set) Token: 0x06001A27 RID: 6695 RVA: 0x000135BD File Offset: 0x000117BD
			[XmlAttribute("id")]
			public string id { get; set; } = "";

			// Token: 0x06001A28 RID: 6696 RVA: 0x000135C6 File Offset: 0x000117C6
			public Run()
			{
				this.Value = "";
			}

			// Token: 0x17000711 RID: 1809
			// (get) Token: 0x06001A29 RID: 6697 RVA: 0x000135E4 File Offset: 0x000117E4
			// (set) Token: 0x06001A2A RID: 6698 RVA: 0x000135EC File Offset: 0x000117EC
			[XmlText]
			public string Value { get; set; }
		}
	}

	// Token: 0x020002FF RID: 767
	[XmlRoot("Run")]
	public class Run
	{
		// Token: 0x17000712 RID: 1810
		// (get) Token: 0x06001A2B RID: 6699 RVA: 0x000135F5 File Offset: 0x000117F5
		// (set) Token: 0x06001A2C RID: 6700 RVA: 0x000135FD File Offset: 0x000117FD
		[XmlAttribute]
		public string version { get; set; } = "1.7.0";

		// Token: 0x17000713 RID: 1811
		// (get) Token: 0x06001A2D RID: 6701 RVA: 0x00013606 File Offset: 0x00011806
		// (set) Token: 0x06001A2E RID: 6702 RVA: 0x0001360E File Offset: 0x0001180E
		public string GameIcon { get; set; } = "";

		// Token: 0x17000714 RID: 1812
		// (get) Token: 0x06001A2F RID: 6703 RVA: 0x00013617 File Offset: 0x00011817
		// (set) Token: 0x06001A30 RID: 6704 RVA: 0x0001361F File Offset: 0x0001181F
		public string GameName { get; set; } = "Getting Over It With Bennett Foddy";

		// Token: 0x17000715 RID: 1813
		// (get) Token: 0x06001A31 RID: 6705 RVA: 0x00013628 File Offset: 0x00011828
		// (set) Token: 0x06001A32 RID: 6706 RVA: 0x00013630 File Offset: 0x00011830
		public string CategoryName { get; set; } = "Glitchless";

		// Token: 0x17000716 RID: 1814
		// (get) Token: 0x06001A33 RID: 6707 RVA: 0x00013639 File Offset: 0x00011839
		// (set) Token: 0x06001A34 RID: 6708 RVA: 0x00013641 File Offset: 0x00011841
		[XmlElement("Metadata")]
		public Timer.MetaData metaData { get; set; } = new Timer.MetaData();

		// Token: 0x17000717 RID: 1815
		// (get) Token: 0x06001A35 RID: 6709 RVA: 0x0001364A File Offset: 0x0001184A
		// (set) Token: 0x06001A36 RID: 6710 RVA: 0x00013652 File Offset: 0x00011852
		public string Offset { get; set; } = "00:00:00";

		// Token: 0x17000718 RID: 1816
		// (get) Token: 0x06001A37 RID: 6711 RVA: 0x0001365B File Offset: 0x0001185B
		// (set) Token: 0x06001A38 RID: 6712 RVA: 0x00013663 File Offset: 0x00011863
		public int AttemptCount { get; set; }

		// Token: 0x17000719 RID: 1817
		// (get) Token: 0x06001A39 RID: 6713 RVA: 0x0001366C File Offset: 0x0001186C
		// (set) Token: 0x06001A3A RID: 6714 RVA: 0x00013674 File Offset: 0x00011874
		[XmlArray("Segments")]
		[XmlArrayItem(typeof(Timer.Run.Segment), IsNullable = false)]
		public List<Timer.Run.Segment> segments { get; set; } = new List<Timer.Run.Segment>();

		// Token: 0x1700071A RID: 1818
		// (get) Token: 0x06001A3B RID: 6715 RVA: 0x0001367D File Offset: 0x0001187D
		// (set) Token: 0x06001A3C RID: 6716 RVA: 0x00013685 File Offset: 0x00011885
		public string AutoSplitterSettings { get; set; } = "";

		// Token: 0x1700071B RID: 1819
		// (get) Token: 0x06001A3D RID: 6717 RVA: 0x0001368E File Offset: 0x0001188E
		// (set) Token: 0x06001A3E RID: 6718 RVA: 0x00013696 File Offset: 0x00011896
		[XmlArray("AttemptHistory")]
		[XmlArrayItem(typeof(Timer.Run.Attempt), IsNullable = false)]
		public List<Timer.Run.Attempt> AttemptHistory { get; set; } = new List<Timer.Run.Attempt>();

		// Token: 0x02000300 RID: 768
		public class Segment
		{
			// Token: 0x1700071C RID: 1820
			// (get) Token: 0x06001A40 RID: 6720 RVA: 0x0001369F File Offset: 0x0001189F
			// (set) Token: 0x06001A41 RID: 6721 RVA: 0x000136A7 File Offset: 0x000118A7
			public string Name { get; set; }

			// Token: 0x1700071D RID: 1821
			// (get) Token: 0x06001A42 RID: 6722 RVA: 0x000136B0 File Offset: 0x000118B0
			// (set) Token: 0x06001A43 RID: 6723 RVA: 0x000136B8 File Offset: 0x000118B8
			public string Icon { get; set; }

			// Token: 0x1700071E RID: 1822
			// (get) Token: 0x06001A44 RID: 6724 RVA: 0x000136C1 File Offset: 0x000118C1
			// (set) Token: 0x06001A45 RID: 6725 RVA: 0x000136C9 File Offset: 0x000118C9
			[XmlElement("SplitTimes")]
			public Timer.Run.Segment.SplitTimes splits { get; set; }

			// Token: 0x1700071F RID: 1823
			// (get) Token: 0x06001A46 RID: 6726 RVA: 0x000136D2 File Offset: 0x000118D2
			// (set) Token: 0x06001A47 RID: 6727 RVA: 0x000136DA File Offset: 0x000118DA
			[XmlElement("BestSegmentTime")]
			public Timer.Run.Segment.BestSegmentTime best { get; set; }

			// Token: 0x17000720 RID: 1824
			// (get) Token: 0x06001A48 RID: 6728 RVA: 0x000136E3 File Offset: 0x000118E3
			// (set) Token: 0x06001A49 RID: 6729 RVA: 0x000136EB File Offset: 0x000118EB
			[XmlArray("SegmentHistory")]
			[XmlArrayItem(typeof(Timer.Run.Segment.SplitTimeHistory), IsNullable = false)]
			public List<Timer.Run.Segment.SplitTimeHistory> SegmentHistory { get; set; } = new List<Timer.Run.Segment.SplitTimeHistory>();

			// Token: 0x02000301 RID: 769
			public class SplitTimes
			{
				// Token: 0x17000721 RID: 1825
				// (get) Token: 0x06001A4B RID: 6731 RVA: 0x00013707 File Offset: 0x00011907
				// (set) Token: 0x06001A4C RID: 6732 RVA: 0x0001370F File Offset: 0x0001190F
				[XmlElement("SplitTime")]
				public Timer.Run.Segment.SplitTimes.SplitTime split { get; set; } = new Timer.Run.Segment.SplitTimes.SplitTime();

				// Token: 0x02000302 RID: 770
				public class SplitTime
				{
					// Token: 0x17000722 RID: 1826
					// (get) Token: 0x06001A4E RID: 6734 RVA: 0x0001372B File Offset: 0x0001192B
					// (set) Token: 0x06001A4F RID: 6735 RVA: 0x00013733 File Offset: 0x00011933
					[XmlAttribute("name")]
					public string name { get; set; } = "Personal Best";

					// Token: 0x17000723 RID: 1827
					// (get) Token: 0x06001A50 RID: 6736 RVA: 0x0001373C File Offset: 0x0001193C
					// (set) Token: 0x06001A51 RID: 6737 RVA: 0x00013744 File Offset: 0x00011944
					public string RealTime { get; set; } = "";

					// Token: 0x17000724 RID: 1828
					// (get) Token: 0x06001A52 RID: 6738 RVA: 0x0001374D File Offset: 0x0001194D
					// (set) Token: 0x06001A53 RID: 6739 RVA: 0x00013755 File Offset: 0x00011955
					public string GameTime { get; set; } = "";
				}
			}

			// Token: 0x02000303 RID: 771
			public class BestSegmentTime
			{
				// Token: 0x17000725 RID: 1829
				// (get) Token: 0x06001A55 RID: 6741 RVA: 0x00013787 File Offset: 0x00011987
				// (set) Token: 0x06001A56 RID: 6742 RVA: 0x0001378F File Offset: 0x0001198F
				public string RealTime { get; set; } = "";

				// Token: 0x17000726 RID: 1830
				// (get) Token: 0x06001A57 RID: 6743 RVA: 0x00013798 File Offset: 0x00011998
				// (set) Token: 0x06001A58 RID: 6744 RVA: 0x000137A0 File Offset: 0x000119A0
				public string GameTime { get; set; } = "";
			}

			// Token: 0x02000304 RID: 772
			public class SplitTimeHistory
			{
				// Token: 0x17000727 RID: 1831
				// (get) Token: 0x06001A5A RID: 6746 RVA: 0x000137C7 File Offset: 0x000119C7
				// (set) Token: 0x06001A5B RID: 6747 RVA: 0x000137CF File Offset: 0x000119CF
				[XmlAttribute("id")]
				public int name { get; set; }

				// Token: 0x17000728 RID: 1832
				// (get) Token: 0x06001A5C RID: 6748 RVA: 0x000137D8 File Offset: 0x000119D8
				// (set) Token: 0x06001A5D RID: 6749 RVA: 0x000137E0 File Offset: 0x000119E0
				public string RealTime { get; set; }

				// Token: 0x17000729 RID: 1833
				// (get) Token: 0x06001A5E RID: 6750 RVA: 0x000137E9 File Offset: 0x000119E9
				// (set) Token: 0x06001A5F RID: 6751 RVA: 0x000137F1 File Offset: 0x000119F1
				public string GameTime { get; set; }
			}
		}

		// Token: 0x02000305 RID: 773
		public class Attempt
		{
			// Token: 0x1700072A RID: 1834
			// (get) Token: 0x06001A61 RID: 6753 RVA: 0x000137FA File Offset: 0x000119FA
			// (set) Token: 0x06001A62 RID: 6754 RVA: 0x00013802 File Offset: 0x00011A02
			[XmlAttribute("id")]
			public string id { get; set; }

			// Token: 0x1700072B RID: 1835
			// (get) Token: 0x06001A63 RID: 6755 RVA: 0x0001380B File Offset: 0x00011A0B
			// (set) Token: 0x06001A64 RID: 6756 RVA: 0x00013813 File Offset: 0x00011A13
			[XmlAttribute("started")]
			public string started { get; set; }

			// Token: 0x1700072C RID: 1836
			// (get) Token: 0x06001A65 RID: 6757 RVA: 0x0001381C File Offset: 0x00011A1C
			// (set) Token: 0x06001A66 RID: 6758 RVA: 0x00013824 File Offset: 0x00011A24
			[XmlAttribute("isStartedSynced")]
			public string isStartedSynced { get; set; }

			// Token: 0x1700072D RID: 1837
			// (get) Token: 0x06001A67 RID: 6759 RVA: 0x0001382D File Offset: 0x00011A2D
			// (set) Token: 0x06001A68 RID: 6760 RVA: 0x00013835 File Offset: 0x00011A35
			[XmlAttribute("ended")]
			public string ended { get; set; }

			// Token: 0x1700072E RID: 1838
			// (get) Token: 0x06001A69 RID: 6761 RVA: 0x0001383E File Offset: 0x00011A3E
			// (set) Token: 0x06001A6A RID: 6762 RVA: 0x00013846 File Offset: 0x00011A46
			[XmlAttribute("isEndedSynced")]
			public string isEndedSynced { get; set; }

			// Token: 0x1700072F RID: 1839
			// (get) Token: 0x06001A6B RID: 6763 RVA: 0x0001384F File Offset: 0x00011A4F
			// (set) Token: 0x06001A6C RID: 6764 RVA: 0x00013857 File Offset: 0x00011A57
			[XmlText]
			public string Value { get; set; }
		}
	}
}
