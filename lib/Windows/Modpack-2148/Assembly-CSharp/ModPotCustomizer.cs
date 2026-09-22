using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x02000316 RID: 790
public class ModPotCustomizer : Mod
{
	// Token: 0x06001B11 RID: 6929 RVA: 0x00082164 File Offset: 0x00080364
	public bool Load()
	{
		try
		{
			if (GameObject.Find("Player") && this.options.Count > 0)
			{
				if (this.options.Count <= this.option)
				{
					this.option = 0;
					this.lastOption = this.option;
				}
				this.potRendered = GameObject.Find("Player").transform.Find("Pot/Mesh").GetComponent<MeshRenderer>();
				if (this.backupMaterial == null)
				{
					this.backupMaterial = this.potRendered.material;
				}
				try
				{
					if (this.option > 1)
					{
						this.potRendered.material = this.backupMaterial;
						this.potRendered.material.SetFloat("_Goldness", 0f);
						this.potRendered.material.mainTexture = ModPackManager.LoadPNG(string.Format("modpack\\mods\\{0}\\{1}", this.name, this.optionsTextures[this.option]));
					}
					else
					{
						this.updateColor(true);
					}
				}
				catch (Exception)
				{
					this.UnLoad();
					return true;
				}
				PlayerPrefs.SetString("ModPotCustomizerOption", this.options[this.option]);
				PlayerPrefs.Save();
				this.lastOption = this.option;
			}
		}
		catch
		{
		}
		this.enabled = true;
		PlayerPrefs.SetInt("ModPotCustomizer", 1);
		PlayerPrefs.Save();
		return true;
	}

	// Token: 0x06001B12 RID: 6930 RVA: 0x00082300 File Offset: 0x00080500
	public ModPotCustomizer()
	{
		this.name = "Pot Customizer";
		this.version = "1.1";
		this.author = "fm2";
		this.web = "";
		this.optionsTitle = "Choose Pot";
		this.options = new List<string>();
		this.optionsTextures = new List<string>();
		this.options.Add("RGB");
		this.optionsTextures.Add("No Texture RGB");
		this.options.Add("Static Color");
		this.optionsTextures.Add("No Texture Color");
		this.option = 0;
		this.lastOption = -1;
		this.enableSplits = false;
		this.enabled = false;
		this.backupMaterial = null;
		this.potRendered = null;
		this.colorR = 0f;
		this.colorG = 0f;
		this.colorB = 0f;
		this.timeResetColor = 0f;
		this.targetColor = global::UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.6f, 1f);
		string text = string.Format("modpack\\mods\\{0}\\", this.name);
		string text2 = string.Empty;
		if (PlayerPrefs.HasKey("ModPotCustomizerOption"))
		{
			text2 = PlayerPrefs.GetString("ModPotCustomizerOption");
		}
		if (text2 == "No Texture ")
		{
			this.option = 1;
		}
		if (Directory.Exists(text))
		{
			int num = 0;
			foreach (string text3 in Directory.GetFiles(text))
			{
				if (text3.Length > 0 && text3.EndsWith(".png"))
				{
					try
					{
						this.optionsTextures.Add(Path.GetFileName(text3));
						this.options.Add(Path.GetFileName(text3).Substring(0, Path.GetFileName(text3).Length - 4));
					}
					catch
					{
						if (this.optionsTextures.Count > this.options.Count)
						{
							this.optionsTextures.RemoveAt(this.optionsTextures.Count - 1);
						}
					}
					if (this.options[num].Equals(text2, StringComparison.OrdinalIgnoreCase))
					{
						this.option = num;
					}
					num++;
				}
			}
		}
		if (PlayerPrefs.HasKey("ModPotCustomizer"))
		{
			this.enabled = PlayerPrefs.GetInt("ModPotCustomizer") == 1;
		}
	}

	// Token: 0x06001B13 RID: 6931 RVA: 0x00082574 File Offset: 0x00080774
	public bool UnLoad()
	{
		try
		{
			if (GameObject.Find("Player"))
			{
				this.potRendered.material.SetFloat("_Goldness", (float)SettingsManager.potGoldness);
				this.potRendered.material = this.backupMaterial;
			}
			this.enabled = false;
			PlayerPrefs.SetInt("ModPotCustomizer", 0);
			PlayerPrefs.Save();
		}
		catch
		{
		}
		return true;
	}

	// Token: 0x1700077A RID: 1914
	// (get) Token: 0x06001B14 RID: 6932 RVA: 0x00013C95 File Offset: 0x00011E95
	public string name { get; }

	// Token: 0x1700077B RID: 1915
	// (get) Token: 0x06001B15 RID: 6933 RVA: 0x00013C9D File Offset: 0x00011E9D
	public string version { get; }

	// Token: 0x1700077C RID: 1916
	// (get) Token: 0x06001B16 RID: 6934 RVA: 0x00013CA5 File Offset: 0x00011EA5
	public string author { get; }

	// Token: 0x1700077D RID: 1917
	// (get) Token: 0x06001B17 RID: 6935 RVA: 0x00013CAD File Offset: 0x00011EAD
	public string web { get; }

	// Token: 0x06001B18 RID: 6936 RVA: 0x000825EC File Offset: 0x000807EC
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
						if (this.option > 1)
						{
							this.potRendered.material = this.backupMaterial;
							this.potRendered.material.SetFloat("_Goldness", 0f);
							this.potRendered.material.mainTexture = ModPackManager.LoadPNG(string.Format("modpack\\mods\\{0}\\{1}", this.name, this.optionsTextures[this.option]));
						}
						else
						{
							this.potRendered.material = this.backupMaterial;
							this.updateColor(true);
						}
						if (this.option == 0)
						{
							this.timeResetColor = deltaTime;
						}
						this.lastOption = this.option;
					}
				}
				catch (Exception ex)
				{
					Debug.Log(ex.ToString());
				}
				PlayerPrefs.SetString("ModPotCustomizerOption", this.options[this.option]);
				PlayerPrefs.Save();
				return "";
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
				this.targetColor = global::UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.6f, 1f);
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

	// Token: 0x1700077E RID: 1918
	// (get) Token: 0x06001B19 RID: 6937 RVA: 0x00013CB5 File Offset: 0x00011EB5
	// (set) Token: 0x06001B1A RID: 6938 RVA: 0x00013CBD File Offset: 0x00011EBD
	public int option { get; set; }

	// Token: 0x1700077F RID: 1919
	// (get) Token: 0x06001B1B RID: 6939 RVA: 0x00013CC6 File Offset: 0x00011EC6
	public List<string> options { get; }

	// Token: 0x17000780 RID: 1920
	// (get) Token: 0x06001B1C RID: 6940 RVA: 0x00013CCE File Offset: 0x00011ECE
	// (set) Token: 0x06001B1D RID: 6941 RVA: 0x00013CD6 File Offset: 0x00011ED6
	public bool enabled { get; set; }

	// Token: 0x17000781 RID: 1921
	// (get) Token: 0x06001B1E RID: 6942 RVA: 0x00013CDF File Offset: 0x00011EDF
	// (set) Token: 0x06001B1F RID: 6943 RVA: 0x00013CE7 File Offset: 0x00011EE7
	public string optionsTitle { get; set; }

	// Token: 0x17000782 RID: 1922
	// (get) Token: 0x06001B20 RID: 6944 RVA: 0x00013CF0 File Offset: 0x00011EF0
	// (set) Token: 0x06001B21 RID: 6945 RVA: 0x00013CF8 File Offset: 0x00011EF8
	public bool enableSplits { get; set; }

	// Token: 0x17000783 RID: 1923
	// (get) Token: 0x06001B22 RID: 6946 RVA: 0x00013D01 File Offset: 0x00011F01
	// (set) Token: 0x06001B23 RID: 6947 RVA: 0x00013D09 File Offset: 0x00011F09
	public int lastOption { get; set; }

	// Token: 0x17000784 RID: 1924
	// (get) Token: 0x06001B24 RID: 6948 RVA: 0x00013D12 File Offset: 0x00011F12
	// (set) Token: 0x06001B25 RID: 6949 RVA: 0x00013D1A File Offset: 0x00011F1A
	public List<string> optionsTextures { get; set; }

	// Token: 0x06001B26 RID: 6950 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001B27 RID: 6951 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x17000785 RID: 1925
	// (get) Token: 0x06001B28 RID: 6952 RVA: 0x00013D23 File Offset: 0x00011F23
	// (set) Token: 0x06001B29 RID: 6953 RVA: 0x00013D2B File Offset: 0x00011F2B
	private Material backupMaterial { get; set; }

	// Token: 0x06001B2A RID: 6954 RVA: 0x0008281C File Offset: 0x00080A1C
	private void updateColor(bool syncColors = true)
	{
		if (syncColors)
		{
			this.colorR = (float)SettingsManager.potColorR;
			this.colorG = (float)SettingsManager.potColorG;
			this.colorB = (float)SettingsManager.potColorB;
		}
		this.potRendered.material = new Material(Shader.Find("Specular"));
		this.potRendered.material.SetColor("_Color", new Color(this.colorR, this.colorG, this.colorB, 1f));
	}

	// Token: 0x17000786 RID: 1926
	// (get) Token: 0x06001B2B RID: 6955 RVA: 0x00013D34 File Offset: 0x00011F34
	// (set) Token: 0x06001B2C RID: 6956 RVA: 0x00013D3C File Offset: 0x00011F3C
	public float colorR { get; set; }

	// Token: 0x17000787 RID: 1927
	// (get) Token: 0x06001B2D RID: 6957 RVA: 0x00013D45 File Offset: 0x00011F45
	// (set) Token: 0x06001B2E RID: 6958 RVA: 0x00013D4D File Offset: 0x00011F4D
	public float colorG { get; set; }

	// Token: 0x17000788 RID: 1928
	// (get) Token: 0x06001B2F RID: 6959 RVA: 0x00013D56 File Offset: 0x00011F56
	// (set) Token: 0x06001B30 RID: 6960 RVA: 0x00013D5E File Offset: 0x00011F5E
	public float colorB { get; set; }

	// Token: 0x17000789 RID: 1929
	// (get) Token: 0x06001B31 RID: 6961 RVA: 0x00013D67 File Offset: 0x00011F67
	// (set) Token: 0x06001B32 RID: 6962 RVA: 0x00013D6F File Offset: 0x00011F6F
	public Color32 targetColor { get; set; }

	// Token: 0x1700078A RID: 1930
	// (get) Token: 0x06001B33 RID: 6963 RVA: 0x00013D78 File Offset: 0x00011F78
	// (set) Token: 0x06001B34 RID: 6964 RVA: 0x00013D80 File Offset: 0x00011F80
	public float timeResetColor { get; set; }

	// Token: 0x1700078B RID: 1931
	// (get) Token: 0x06001B35 RID: 6965 RVA: 0x00013D89 File Offset: 0x00011F89
	// (set) Token: 0x06001B36 RID: 6966 RVA: 0x00013D91 File Offset: 0x00011F91
	private MeshRenderer potRendered { get; set; }
}
