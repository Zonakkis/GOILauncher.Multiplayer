using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x020002F0 RID: 752
public class UpdateManager
{
	// Token: 0x060019AD RID: 6573 RVA: 0x0007B018 File Offset: 0x00079218
	public UpdateManager()
	{
		this.downloading = false;
		this.filesDownloaded = 0;
		this.bytesDownloaded = 0L;
		this.totalBytes = 0L;
		this.percentageDownloaded = 0f;
		this.downloadTime = 0f;
		this.updateAvailable = false;
		this.downloadComplete = false;
		this.checkingFiles = false;
		this.checkingComplete = false;
		this.checkingFailed = false;
		this.FilesInUseFailed = false;
		this.updateComplete = false;
		this.updateFailed = false;
		this.mirrorStarted = false;
		this.mirrorFailed = false;
		this.filename = "";
		this.filebytes = 0L;
	}

	// Token: 0x060019AE RID: 6574 RVA: 0x0007B0B8 File Offset: 0x000792B8
	private void DownloadFileCallback(object sender, AsyncCompletedEventArgs eventHandler)
	{
		if (eventHandler.Error != null)
		{
			Debug.Log("Error downloading file: " + eventHandler.Error.Message);
			this.downloadFailed = true;
			if (this.mirrorStarted)
			{
				this.mirrorFailed = true;
			}
		}
		this.filesDownloaded++;
		if (this.filesDownloaded == this.filesToDownload)
		{
			if (this.downloadFailed && !this.mirrorStarted)
			{
				this.filesDownloaded = 0;
				this.bytesDownloaded = 0L;
				this.percentageDownloaded = 0f;
				foreach (string text in this.update.Files.Keys)
				{
					this.update.Files[text].Downloaded = 0L;
				}
				this.mirrorStarted = true;
				this.downloading = false;
				this.DownloadModpack(true);
				return;
			}
			if (this.mirrorFailed)
			{
				return;
			}
			this.downloadComplete = true;
			if (this.checkFiles())
			{
				this.updateFiles();
			}
		}
	}

	// Token: 0x060019AF RID: 6575 RVA: 0x0007B1E4 File Offset: 0x000793E4
	private UpdateManager.Update DownloadJsonUpdate()
	{
		try
		{
			return JsonConvert.DeserializeObject<UpdateManager.Update>(this.DownloadJsonUpdateString());
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
		}
		return null;
	}

	// Token: 0x060019B0 RID: 6576 RVA: 0x0007B220 File Offset: 0x00079420
	private string DownloadJsonUpdateString()
	{
		try
		{
			return new WebClient().DownloadString("https://www.anjo2.com/goi/update.json");
		}
		catch
		{
		}
		return "";
	}

	// Token: 0x060019B1 RID: 6577 RVA: 0x0007B25C File Offset: 0x0007945C
	public void GetModpackUpdate(string version)
	{
		if (this.version == null || this.version != version)
		{
			this.version = version;
		}
		this.update = this.DownloadJsonUpdate();
		if (this.update != null && this.update.Build != version && this.update.Updates.ContainsKey(this.version))
		{
			try
			{
				for (int i = 0; i < this.update.Updates[version].Length; i++)
				{
					this.totalBytes += (long)this.update.Files[this.update.Updates[version][i]].Size;
					this.update.Files[this.update.Updates[version][i]].Downloaded = 0L;
				}
				this.filesToDownload = this.update.Updates[version].Length;
				this.updateAvailable = true;
			}
			catch
			{
			}
			Debug.Log("Total to update: " + this.totalBytes.ToString());
		}
	}

	// Token: 0x060019B2 RID: 6578 RVA: 0x0007B39C File Offset: 0x0007959C
	public bool checkFiles()
	{
		if (this.checkingFailed)
		{
			return false;
		}
		this.checkingFiles = true;
		for (int i = 0; i < this.update.Updates[this.version].Length; i++)
		{
			if (this.update.Files[this.update.Updates[this.version][i]].Checksum != this.GetFileHash(string.Format("modpack\\updates\\{0}", this.update.Files[this.update.Updates[this.version][i]].Name)))
			{
				this.checkingFailed = true;
				return false;
			}
			if (this.FileInUse(string.Format("{0}{1}", this.update.Files[this.update.Updates[this.version][i]].Path, this.update.Files[this.update.Updates[this.version][i]].Name)))
			{
				this.checkingFailed = true;
				this.FilesInUseFailed = true;
				return false;
			}
		}
		this.checkingComplete = true;
		return true;
	}

	// Token: 0x060019B3 RID: 6579 RVA: 0x0007B4E4 File Offset: 0x000796E4
	public bool updateFiles()
	{
		bool flag;
		if (this.checkingComplete && (!this.updateFailed || !this.mirrorFailed) && !this.updateComplete)
		{
			for (int i = 0; i < this.update.Updates[this.version].Length; i++)
			{
				try
				{
					string text = string.Format("{0}{1}", this.update.Files[this.update.Updates[this.version][i]].Path, this.update.Files[this.update.Updates[this.version][i]].Name);
					this.createDir(text);
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					File.Move(string.Format("modpack\\updates\\{0}", this.update.Files[this.update.Updates[this.version][i]].Name), text);
				}
				catch
				{
					this.updateFailed = true;
					return false;
				}
			}
			this.updateComplete = true;
			flag = true;
		}
		else
		{
			flag = false;
		}
		return flag;
	}

	// Token: 0x060019B4 RID: 6580 RVA: 0x0007B62C File Offset: 0x0007982C
	public string GetFileHash(string filePath)
	{
		string text;
		if (!File.Exists(filePath))
		{
			text = null;
		}
		else
		{
			string text2;
			using (MD5 md = MD5.Create())
			{
				using (FileStream fileStream = File.OpenRead(filePath))
				{
					text2 = BitConverter.ToString(md.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
				}
			}
			text = text2;
		}
		return text;
	}

	// Token: 0x060019B5 RID: 6581 RVA: 0x0007B6A8 File Offset: 0x000798A8
	public bool FileInUse(string filePath)
	{
		if (File.Exists(filePath))
		{
			try
			{
				using (File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
				}
			}
			catch (IOException)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	// Token: 0x060019B6 RID: 6582 RVA: 0x000131AD File Offset: 0x000113AD
	private DownloadProgressChangedEventHandler DownloadProgressCallback(string filename)
	{
		UpdateManager.<>c__DisplayClass9_0 CS$<>8__locals1 = new UpdateManager.<>c__DisplayClass9_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.filename = filename;
		return new DownloadProgressChangedEventHandler(new Action<object, DownloadProgressChangedEventArgs>(CS$<>8__locals1, ldftn(<DownloadProgressCallback>b__0)).Invoke);
	}

	// Token: 0x060019B7 RID: 6583 RVA: 0x0007B6FC File Offset: 0x000798FC
	public bool createDir(string path)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(path);
			if (directoryName.Length > 0 && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
		}
		catch
		{
			return false;
		}
		return true;
	}

	// Token: 0x060019B8 RID: 6584 RVA: 0x0007B744 File Offset: 0x00079944
	private void DownloadFile(string url, string path, string file)
	{
		this.createDir(path);
		WebClient webClient = new WebClient();
		ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
		webClient.DownloadFileCompleted += this.DownloadFileCallback;
		webClient.DownloadProgressChanged += this.DownloadProgressCallback(file);
		webClient.DownloadFileAsync(new Uri(url), path);
	}

	// Token: 0x060019B9 RID: 6585 RVA: 0x0007B7B0 File Offset: 0x000799B0
	public void showChangelog()
	{
		if (this.update != null)
		{
			UpdateManager.changelogSize = new Vector2((float)Screen.width * 0.9f, (float)Screen.height - 200f);
			try
			{
				if (UpdateManager.backgroundStyle == null)
				{
					UpdateManager.backgroundStyle = new GUIStyle(GUI.skin.box)
					{
						normal = 
						{
							background = Texture2D.whiteTexture
						}
					};
					UpdateManager.changelogTitleStyle = new GUIStyle(GUI.skin.label)
					{
						alignment = TextAnchor.MiddleCenter,
						fontStyle = FontStyle.Bold,
						normal = 
						{
							textColor = Color.red
						}
					};
					UpdateManager.rowStyle = new GUIStyle
					{
						normal = 
						{
							background = Texture2D.whiteTexture
						}
					};
				}
				Color color = GUI.backgroundColor;
				Color textColor = GUI.skin.label.normal.textColor;
				GUI.backgroundColor = new Color(0f, 0f, 0f, 0.95f);
				GUILayout.BeginArea(new Rect((float)Screen.width / 2f - UpdateManager.changelogSize.x / 2f, 50f, UpdateManager.changelogSize.x, UpdateManager.changelogSize.y), UpdateManager.backgroundStyle);
				GUILayout.BeginArea(new Rect(10f, 0f, UpdateManager.changelogSize.x - 20f, UpdateManager.changelogSize.y));
				Vector2 vector = GUILayout.BeginScrollView(this.scrollPosition, false, false, new GUILayoutOption[0]);
				foreach (KeyValuePair<string, string[]> keyValuePair in this.update.Changelogs)
				{
					if (keyValuePair.Key == this.version)
					{
						break;
					}
					GUI.backgroundColor = Color.white;
					GUILayout.Label(string.Format("Changelog Build {0}", keyValuePair.Key), UpdateManager.changelogTitleStyle, new GUILayoutOption[] { GUILayout.Width(UpdateManager.changelogSize.x) });
					GUI.backgroundColor = new Color(0f, 0f, 0f, 0.95f);
					for (int i = 0; i < keyValuePair.Value.Length; i++)
					{
						GUILayout.Space(-2f);
						this.DrawRow(i, keyValuePair.Value[i]);
					}
				}
				GUILayout.EndScrollView();
				if (vector != this.scrollPosition)
				{
					this.scrollPosition = vector;
				}
				GUILayout.EndArea();
				GUILayout.EndArea();
				GUI.backgroundColor = color;
			}
			catch (Exception ex)
			{
				Debug.Log(ex.ToString());
			}
		}
	}

	// Token: 0x060019BA RID: 6586 RVA: 0x0007BA70 File Offset: 0x00079C70
	private void DrawRow(int index, string val)
	{
		float num = ((index % 2 == 0) ? 0.3f : 0.2f);
		GUI.backgroundColor = new Color(num, num, num, 0.2f);
		GUILayout.BeginHorizontal(UpdateManager.rowStyle, new GUILayoutOption[] { GUILayout.Width(UpdateManager.changelogSize.x - 41f) });
		GUILayout.Label(string.Format("► {0}", val), new GUILayoutOption[] { GUILayout.Width(UpdateManager.changelogSize.x - 51f) });
		GUILayout.EndHorizontal();
	}

	// Token: 0x060019BB RID: 6587 RVA: 0x0007BAFC File Offset: 0x00079CFC
	public void DownloadModpack(bool mirror = false)
	{
		if (this.updateAvailable && this.update != null && !this.downloading)
		{
			for (int i = 0; i < this.update.Updates[this.version].Length; i++)
			{
				if (mirror)
				{
					this.DownloadFile(this.update.Files[this.update.Updates[this.version][i]].Mirror, "modpack\\updates\\" + this.update.Files[this.update.Updates[this.version][i]].Name, this.update.Updates[this.version][i]);
				}
				else
				{
					this.DownloadFile(this.update.Files[this.update.Updates[this.version][i]].Uri, "modpack\\updates\\" + this.update.Files[this.update.Updates[this.version][i]].Name, this.update.Updates[this.version][i]);
				}
			}
			this.downloading = true;
		}
	}

	// Token: 0x04001152 RID: 4434
	public bool downloading;

	// Token: 0x04001153 RID: 4435
	public int filesDownloaded;

	// Token: 0x04001154 RID: 4436
	public long bytesDownloaded;

	// Token: 0x04001155 RID: 4437
	public long totalBytes;

	// Token: 0x04001156 RID: 4438
	public float percentageDownloaded;

	// Token: 0x04001157 RID: 4439
	public float downloadTime;

	// Token: 0x04001158 RID: 4440
	public bool updateAvailable;

	// Token: 0x04001159 RID: 4441
	public UpdateManager.Update update;

	// Token: 0x0400115A RID: 4442
	private string version;

	// Token: 0x0400115B RID: 4443
	public int filesToDownload;

	// Token: 0x0400115C RID: 4444
	public bool downloadFinished;

	// Token: 0x0400115D RID: 4445
	public bool downloadFailed;

	// Token: 0x0400115E RID: 4446
	public bool downloadComplete;

	// Token: 0x0400115F RID: 4447
	public bool checkingFiles;

	// Token: 0x04001160 RID: 4448
	public bool checkingFailed;

	// Token: 0x04001161 RID: 4449
	public bool checkingComplete;

	// Token: 0x04001162 RID: 4450
	public bool FilesInUseFailed;

	// Token: 0x04001163 RID: 4451
	public bool updateComplete;

	// Token: 0x04001164 RID: 4452
	public bool updateFailed;

	// Token: 0x04001165 RID: 4453
	private string filename;

	// Token: 0x04001166 RID: 4454
	private long filebytes;

	// Token: 0x04001167 RID: 4455
	private static Vector2 changelogSize = new Vector2(600f, 400f);

	// Token: 0x04001168 RID: 4456
	private static readonly Color backgroundColor = new Color(0f, 0f, 0f, 0.95f);

	// Token: 0x04001169 RID: 4457
	private static GUIStyle backgroundStyle;

	// Token: 0x0400116A RID: 4458
	private static GUIStyle changelogTitleStyle;

	// Token: 0x0400116B RID: 4459
	private Vector2 scrollPosition;

	// Token: 0x0400116C RID: 4460
	private static GUIStyle rowStyle;

	// Token: 0x0400116D RID: 4461
	public bool mirrorStarted;

	// Token: 0x0400116E RID: 4462
	public bool mirrorFailed;

	// Token: 0x020002F1 RID: 753
	public class FileData
	{
		// Token: 0x170006F3 RID: 1779
		// (get) Token: 0x060019BD RID: 6589 RVA: 0x0001320C File Offset: 0x0001140C
		// (set) Token: 0x060019BE RID: 6590 RVA: 0x00013214 File Offset: 0x00011414
		public string Path { get; set; }

		// Token: 0x170006F4 RID: 1780
		// (get) Token: 0x060019BF RID: 6591 RVA: 0x0001321D File Offset: 0x0001141D
		// (set) Token: 0x060019C0 RID: 6592 RVA: 0x00013225 File Offset: 0x00011425
		public string Name { get; set; }

		// Token: 0x170006F5 RID: 1781
		// (get) Token: 0x060019C1 RID: 6593 RVA: 0x0001322E File Offset: 0x0001142E
		// (set) Token: 0x060019C2 RID: 6594 RVA: 0x00013236 File Offset: 0x00011436
		public int Size { get; set; }

		// Token: 0x170006F6 RID: 1782
		// (get) Token: 0x060019C3 RID: 6595 RVA: 0x0001323F File Offset: 0x0001143F
		// (set) Token: 0x060019C4 RID: 6596 RVA: 0x00013247 File Offset: 0x00011447
		public string Checksum { get; set; }

		// Token: 0x170006F7 RID: 1783
		// (get) Token: 0x060019C5 RID: 6597 RVA: 0x00013250 File Offset: 0x00011450
		// (set) Token: 0x060019C6 RID: 6598 RVA: 0x00013258 File Offset: 0x00011458
		public string Uri { get; set; }

		// Token: 0x170006F8 RID: 1784
		// (get) Token: 0x060019C7 RID: 6599 RVA: 0x00013261 File Offset: 0x00011461
		// (set) Token: 0x060019C8 RID: 6600 RVA: 0x00013269 File Offset: 0x00011469
		public long Downloaded { get; set; }

		// Token: 0x170006F9 RID: 1785
		// (get) Token: 0x060019C9 RID: 6601 RVA: 0x00013272 File Offset: 0x00011472
		// (set) Token: 0x060019CA RID: 6602 RVA: 0x0001327A File Offset: 0x0001147A
		public string Mirror { get; set; }
	}

	// Token: 0x020002F2 RID: 754
	public class Update
	{
		// Token: 0x170006FA RID: 1786
		// (get) Token: 0x060019CC RID: 6604 RVA: 0x00013283 File Offset: 0x00011483
		// (set) Token: 0x060019CD RID: 6605 RVA: 0x0001328B File Offset: 0x0001148B
		public string Build { get; set; }

		// Token: 0x170006FB RID: 1787
		// (get) Token: 0x060019CE RID: 6606 RVA: 0x00013294 File Offset: 0x00011494
		// (set) Token: 0x060019CF RID: 6607 RVA: 0x0001329C File Offset: 0x0001149C
		public Dictionary<string, string[]> Changelogs { get; set; }

		// Token: 0x170006FC RID: 1788
		// (get) Token: 0x060019D0 RID: 6608 RVA: 0x000132A5 File Offset: 0x000114A5
		// (set) Token: 0x060019D1 RID: 6609 RVA: 0x000132AD File Offset: 0x000114AD
		public Dictionary<string, string[]> Updates { get; set; }

		// Token: 0x170006FD RID: 1789
		// (get) Token: 0x060019D2 RID: 6610 RVA: 0x000132B6 File Offset: 0x000114B6
		// (set) Token: 0x060019D3 RID: 6611 RVA: 0x000132BE File Offset: 0x000114BE
		public Dictionary<string, UpdateManager.FileData> Files { get; set; }
	}
}
