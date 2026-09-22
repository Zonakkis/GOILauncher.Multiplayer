using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

// Token: 0x020002FA RID: 762
public class ModPackManager
{
	// Token: 0x060019E2 RID: 6626 RVA: 0x0001332D File Offset: 0x0001152D
	public ModPackManager()
	{
		this.tpManager = new TeleportManager();
	}

	// Token: 0x060019E3 RID: 6627 RVA: 0x00013340 File Offset: 0x00011540
	public void Teleport(int x)
	{
		this.tpManager.Teleport(x);
		this.ResetProps();
	}

	// Token: 0x060019E4 RID: 6628 RVA: 0x00013354 File Offset: 0x00011554
	public void CopyToClipboard(string s)
	{
		TextEditor textEditor = new TextEditor();
		textEditor.text = s;
		textEditor.SelectAll();
		textEditor.Copy();
	}

	// Token: 0x060019E5 RID: 6629 RVA: 0x0007D0D0 File Offset: 0x0007B2D0
	public void ResetProps()
	{
		try
		{
			foreach (GameObject gameObject in global::UnityEngine.Object.FindObjectsOfType<GameObject>())
			{
				try
				{
					if (gameObject.name.Contains("Bone1") || gameObject.name.Contains("Rope4"))
					{
						gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
					}
				}
				catch (Exception ex)
				{
					Debug.Log(gameObject.name);
					Debug.Log(ex.ToString());
				}
				try
				{
					if (gameObject.name.Contains("Bone2") || gameObject.name.Contains("Bone3") || gameObject.name.Contains("Bone4") || gameObject.name.Contains("Bone5"))
					{
						gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
						gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
						gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
						gameObject.GetComponent<Rigidbody2D>().angularVelocity = 0f;
						gameObject.GetComponent<Rigidbody2D>().freezeRotation = false;
					}
				}
				catch (Exception ex2)
				{
					Debug.Log(gameObject.name);
					Debug.Log(ex2.ToString());
				}
			}
		}
		catch
		{
		}
		try
		{
			GameObject.Find("Props").transform.Find("Coffee+Cup+Takeaway").gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
			GameObject.Find("Props").transform.Find("Coffee+Cup+Takeaway").position = new Vector3(21f, 36.1f, 0f);
			GameObject.Find("Props").transform.Find("Coffee+Cup+Takeaway").rotation = new Quaternion(0f, 0f, 0.1f, 1f);
			GameObject.Find("Props").transform.Find("Coffee+Cup+Takeaway").gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
			GameObject.Find("Props").transform.Find("Coffee+Cup+Takeaway").gameObject.GetComponent<Rigidbody2D>().freezeRotation = false;
		}
		catch
		{
		}
		try
		{
			GameObject.Find("Props").transform.Find("Orange").gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
			GameObject.Find("Props").transform.Find("Orange").position = new Vector3(-3.3f, 163.6f, -4f);
			GameObject.Find("Props").transform.Find("Orange").rotation = new Quaternion(0f, 0f, 0.2f, 1f);
			GameObject.Find("Props").transform.Find("Orange").gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
			GameObject.Find("Props").transform.Find("Orange").gameObject.GetComponent<Rigidbody2D>().freezeRotation = false;
		}
		catch
		{
		}
		try
		{
			GameObject.Find("Props").transform.Find("SnowHat").gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
			GameObject.Find("Props").transform.Find("SnowHat").position = new Vector3(61.1f, 242.3f, -0.4f);
			GameObject.Find("Props").transform.Find("SnowHat").rotation = new Quaternion(0f, 0f, 0f, 1f);
			GameObject.Find("Props").transform.Find("SnowHat").gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
			GameObject.Find("Props").transform.Find("SnowHat").gameObject.GetComponent<Rigidbody2D>().freezeRotation = false;
		}
		catch
		{
		}
	}

	// Token: 0x060019E6 RID: 6630 RVA: 0x0001336D File Offset: 0x0001156D
	public void SaveStateToClipboard()
	{
		this.CopyToClipboard(this.tpManager.GetSaveState());
	}

	// Token: 0x060019E7 RID: 6631 RVA: 0x0007D5A8 File Offset: 0x0007B7A8
	public void SaveCustomState()
	{
		try
		{
			this.tpManager.SaveCustomState();
		}
		catch
		{
		}
	}

	// Token: 0x060019E8 RID: 6632 RVA: 0x00013380 File Offset: 0x00011580
	public bool LoadCustomState()
	{
		if (this.tpManager.LoadCustomState())
		{
			this.ResetProps();
			return true;
		}
		return false;
	}

	// Token: 0x060019E9 RID: 6633 RVA: 0x0007D5D8 File Offset: 0x0007B7D8
	public static string GetFileHash(string filePath)
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

	// Token: 0x060019EA RID: 6634 RVA: 0x0007D654 File Offset: 0x0007B854
	public static bool FileInUse(string filePath)
	{
		ModPackManager.createDir(filePath);
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

	// Token: 0x060019EB RID: 6635 RVA: 0x0007D6B0 File Offset: 0x0007B8B0
	public static bool createDir(string path)
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

	// Token: 0x060019EC RID: 6636 RVA: 0x0007D6F8 File Offset: 0x0007B8F8
	public static Texture2D LoadPNG(string filePath)
	{
		Texture2D texture2D = null;
		if (File.Exists(filePath))
		{
			byte[] array = File.ReadAllBytes(filePath);
			texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(array);
		}
		return texture2D;
	}

	// Token: 0x060019ED RID: 6637 RVA: 0x00013398 File Offset: 0x00011598
	public void SaveStateToClipboardJson()
	{
		this.CopyToClipboard(this.tpManager.GetSaveStateJson());
	}

	// Token: 0x060019EE RID: 6638 RVA: 0x000133AB File Offset: 0x000115AB
	public bool TeleportMap(Map map, int k)
	{
		if (this.tpManager.TeleportMap(map, k))
		{
			this.ResetProps();
			return true;
		}
		return false;
	}

	// Token: 0x0400118C RID: 4492
	private TeleportManager tpManager;
}
