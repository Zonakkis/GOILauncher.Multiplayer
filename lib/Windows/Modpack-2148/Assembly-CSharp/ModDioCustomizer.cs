using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x02000322 RID: 802
public class ModDioCustomizer : Mod
{
	// Token: 0x06001B8A RID: 7050 RVA: 0x00087DEC File Offset: 0x00085FEC
	public bool Load()
	{
		try
		{
			if (GameObject.Find("Player") && this.options.Count > 0)
			{
				if (this.options.Count <= this.option)
				{
					this.option = 0;
				}
				this.DioRendered = GameObject.Find("Player").transform.Find("dude/Body").GetComponent<SkinnedMeshRenderer>();
				if (this.backupMaterial == null)
				{
					this.backupMaterial = this.DioRendered.material;
					this.backupTexture = this.DioRendered.material.mainTexture;
				}
				try
				{
					if (this.option > 0)
					{
						this.DioRendered.material = this.backupMaterial;
						this.DioRendered.material.mainTexture = ModPackManager.LoadPNG(string.Format("modpack\\mods\\{0}\\{1}", this.name, this.optionsTextures[this.option]));
					}
					else
					{
						this.updateColor(true);
					}
					PlayerPrefs.SetInt("ModDioCustomizerOption", this.option);
					PlayerPrefs.Save();
				}
				catch (Exception)
				{
					this.UnLoad();
					return true;
				}
				this.lastOption = this.option;
			}
		}
		catch
		{
		}
		this.enabled = true;
		PlayerPrefs.SetInt("ModDioCustomizer", 1);
		PlayerPrefs.Save();
		return true;
	}

	// Token: 0x06001B8B RID: 7051 RVA: 0x00087F70 File Offset: 0x00086170
	public ModDioCustomizer()
	{
		this.name = "Dio Customizer";
		this.version = "0.9";
		this.author = "The Head Obamid";
		this.web = "";
		this.optionsTitle = "Choose skin";
		this.options = new List<string>();
		this.options.Add("RGB");
		this.optionsTextures = new List<string>();
		this.optionsTextures.Add("No textures");
		this.option = 0;
		this.lastOption = -1;
		this.enableSplits = false;
		this.enabled = false;
		this.backupMaterial = null;
		this.backupTexture = null;
		this.DioRendered = null;
		this.colorR = 0f;
		this.colorG = 0f;
		this.colorB = 0f;
		this.timeResetColor = 0f;
		this.targetColor = global::UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.4f, 1f);
		string text = string.Format("modpack\\mods\\{0}\\", this.name);
		if (Directory.Exists(text))
		{
			foreach (string text2 in Directory.GetFiles(text))
			{
				if (text2.Length > 0 && text2.EndsWith(".png"))
				{
					try
					{
						this.optionsTextures.Add(Path.GetFileName(text2));
						this.options.Add(Path.GetFileName(text2).Substring(0, Path.GetFileName(text2).Length - 4));
					}
					catch
					{
					}
				}
			}
		}
		if (PlayerPrefs.HasKey("ModDioCustomizer"))
		{
			this.enabled = PlayerPrefs.GetInt("ModDioCustomizer") == 1;
		}
		if (PlayerPrefs.HasKey("ModDioCustomizerOption"))
		{
			this.option = PlayerPrefs.GetInt("ModDioCustomizerOption");
		}
		if (this.options.Count <= this.option)
		{
			this.option = 0;
			this.lastOption = this.option;
		}
	}

	// Token: 0x06001B8C RID: 7052 RVA: 0x00088170 File Offset: 0x00086370
	public bool UnLoad()
	{
		try
		{
			if (GameObject.Find("Player"))
			{
				this.DioRendered.material = this.backupMaterial;
				this.DioRendered.material.mainTexture = this.backupTexture;
			}
			this.enabled = false;
			PlayerPrefs.SetInt("ModDioCustomizer", 0);
			PlayerPrefs.Save();
		}
		catch
		{
		}
		return true;
	}

	// Token: 0x06001B8D RID: 7053 RVA: 0x000881E4 File Offset: 0x000863E4
	public string Update(float deltaTime, GameObject player)
	{
		try
		{
			if (this.lastOption != this.option && player && this.enabled)
			{
				try
				{
					if (this.options.Count > 0)
					{
						if (this.option > 0)
						{
							this.DioRendered.material = this.backupMaterial;
							this.DioRendered.material.mainTexture = ModPackManager.LoadPNG(string.Format("modpack\\mods\\{0}\\{1}", this.name, this.optionsTextures[this.option]));
						}
						else
						{
							this.DioRendered.material = this.backupMaterial;
							this.updateColor(true);
						}
						this.lastOption = this.option;
					}
					PlayerPrefs.SetInt("ModDioCustomizerOption", this.option);
					PlayerPrefs.Save();
				}
				catch (Exception ex)
				{
					Debug.Log(ex.ToString());
				}
			}
		}
		catch (Exception ex2)
		{
			return ex2.ToString();
		}
		if (this.option == 0 && this.enabled)
		{
			if (deltaTime > this.timeResetColor)
			{
				this.targetColor = global::UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.4f, 1f);
				this.timeResetColor = 1f;
			}
			else
			{
				this.timeResetColor -= deltaTime;
			}
			Color color = Color.Lerp(new Color(this.colorR, this.colorG, this.colorB, 1f), this.targetColor, deltaTime / this.timeResetColor);
			this.colorR = color.r;
			this.colorG = color.g;
			this.colorB = color.b;
			this.updateColor(false);
		}
		return "";
	}

	// Token: 0x06001B8E RID: 7054 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001B8F RID: 7055 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x170007B3 RID: 1971
	// (get) Token: 0x06001B90 RID: 7056 RVA: 0x00013F2C File Offset: 0x0001212C
	public string name { get; }

	// Token: 0x170007B4 RID: 1972
	// (get) Token: 0x06001B91 RID: 7057 RVA: 0x00013F34 File Offset: 0x00012134
	public string version { get; }

	// Token: 0x170007B5 RID: 1973
	// (get) Token: 0x06001B92 RID: 7058 RVA: 0x00013F3C File Offset: 0x0001213C
	public string author { get; }

	// Token: 0x170007B6 RID: 1974
	// (get) Token: 0x06001B93 RID: 7059 RVA: 0x00013F44 File Offset: 0x00012144
	public string web { get; }

	// Token: 0x170007B7 RID: 1975
	// (get) Token: 0x06001B94 RID: 7060 RVA: 0x00013F4C File Offset: 0x0001214C
	// (set) Token: 0x06001B95 RID: 7061 RVA: 0x00013F54 File Offset: 0x00012154
	public int option { get; set; }

	// Token: 0x170007B8 RID: 1976
	// (get) Token: 0x06001B96 RID: 7062 RVA: 0x00013F5D File Offset: 0x0001215D
	public List<string> options { get; }

	// Token: 0x170007B9 RID: 1977
	// (get) Token: 0x06001B97 RID: 7063 RVA: 0x00013F65 File Offset: 0x00012165
	// (set) Token: 0x06001B98 RID: 7064 RVA: 0x00013F6D File Offset: 0x0001216D
	public bool enabled { get; set; }

	// Token: 0x170007BA RID: 1978
	// (get) Token: 0x06001B99 RID: 7065 RVA: 0x00013F76 File Offset: 0x00012176
	// (set) Token: 0x06001B9A RID: 7066 RVA: 0x00013F7E File Offset: 0x0001217E
	public string optionsTitle { get; set; }

	// Token: 0x170007BB RID: 1979
	// (get) Token: 0x06001B9B RID: 7067 RVA: 0x00013F87 File Offset: 0x00012187
	// (set) Token: 0x06001B9C RID: 7068 RVA: 0x00013F8F File Offset: 0x0001218F
	public bool enableSplits { get; set; }

	// Token: 0x170007BC RID: 1980
	// (get) Token: 0x06001B9D RID: 7069 RVA: 0x00013F98 File Offset: 0x00012198
	// (set) Token: 0x06001B9E RID: 7070 RVA: 0x00013FA0 File Offset: 0x000121A0
	public int lastOption { get; set; }

	// Token: 0x170007BD RID: 1981
	// (get) Token: 0x06001B9F RID: 7071 RVA: 0x00013FA9 File Offset: 0x000121A9
	// (set) Token: 0x06001BA0 RID: 7072 RVA: 0x00013FB1 File Offset: 0x000121B1
	public List<string> optionsTextures { get; set; }

	// Token: 0x06001BA1 RID: 7073 RVA: 0x000883B8 File Offset: 0x000865B8
	private void updateColor(bool randomizeColors = true)
	{
		if (randomizeColors)
		{
			Color color = global::UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.4f, 1f);
			this.colorR = color.r;
			this.colorG = color.g;
			this.colorB = color.b;
		}
		this.DioRendered.material = new Material(Shader.Find("Specular"));
		this.DioRendered.material.SetColor("_Color", new Color(this.colorR, this.colorG, this.colorB, 1f));
	}

	// Token: 0x170007BE RID: 1982
	// (get) Token: 0x06001BA2 RID: 7074 RVA: 0x00013FBA File Offset: 0x000121BA
	// (set) Token: 0x06001BA3 RID: 7075 RVA: 0x00013FC2 File Offset: 0x000121C2
	public float colorR { get; set; }

	// Token: 0x170007BF RID: 1983
	// (get) Token: 0x06001BA4 RID: 7076 RVA: 0x00013FCB File Offset: 0x000121CB
	// (set) Token: 0x06001BA5 RID: 7077 RVA: 0x00013FD3 File Offset: 0x000121D3
	public float colorG { get; set; }

	// Token: 0x170007C0 RID: 1984
	// (get) Token: 0x06001BA6 RID: 7078 RVA: 0x00013FDC File Offset: 0x000121DC
	// (set) Token: 0x06001BA7 RID: 7079 RVA: 0x00013FE4 File Offset: 0x000121E4
	public float colorB { get; set; }

	// Token: 0x170007C1 RID: 1985
	// (get) Token: 0x06001BA8 RID: 7080 RVA: 0x00013FED File Offset: 0x000121ED
	// (set) Token: 0x06001BA9 RID: 7081 RVA: 0x00013FF5 File Offset: 0x000121F5
	public Color32 targetColor { get; set; }

	// Token: 0x170007C2 RID: 1986
	// (get) Token: 0x06001BAA RID: 7082 RVA: 0x00013FFE File Offset: 0x000121FE
	// (set) Token: 0x06001BAB RID: 7083 RVA: 0x00014006 File Offset: 0x00012206
	public float timeResetColor { get; set; }

	// Token: 0x170007C3 RID: 1987
	// (get) Token: 0x06001BAC RID: 7084 RVA: 0x0001400F File Offset: 0x0001220F
	// (set) Token: 0x06001BAD RID: 7085 RVA: 0x00014017 File Offset: 0x00012217
	private Material backupMaterial { get; set; }

	// Token: 0x170007C4 RID: 1988
	// (get) Token: 0x06001BAE RID: 7086 RVA: 0x00014020 File Offset: 0x00012220
	// (set) Token: 0x06001BAF RID: 7087 RVA: 0x00014028 File Offset: 0x00012228
	private SkinnedMeshRenderer DioRendered { get; set; }

	// Token: 0x170007C5 RID: 1989
	// (get) Token: 0x06001BB0 RID: 7088 RVA: 0x00014031 File Offset: 0x00012231
	// (set) Token: 0x06001BB1 RID: 7089 RVA: 0x00014039 File Offset: 0x00012239
	private Texture backupTexture { get; set; }
}
