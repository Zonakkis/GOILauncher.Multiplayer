using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Noodle;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x020000EC RID: 236
public class MobileManager : MonoBehaviour
{
	// Token: 0x06000690 RID: 1680 RVA: 0x00037F10 File Offset: 0x00036310
	public MobileManager(MobileManager.MobileQualitySettings qs)
	{
		this.qs = qs;
	}

	// Token: 0x06000691 RID: 1681 RVA: 0x00037F84 File Offset: 0x00036384
	public MobileManager()
	{
	}

	// Token: 0x170000A0 RID: 160
	// (get) Token: 0x06000692 RID: 1682 RVA: 0x00037FF1 File Offset: 0x000363F1
	private MobileManager.AndroidSpecs AndroidSpec
	{
		get
		{
			return (SystemInfo.systemMemorySize <= 3100 || NoodleManager.GetAndroidApiVersion() < 26) ? ((SystemInfo.systemMemorySize <= 3100) ? MobileManager.AndroidSpecs.Low : MobileManager.AndroidSpecs.Medium) : MobileManager.AndroidSpecs.High;
		}
	}

	// Token: 0x06000693 RID: 1683 RVA: 0x0003802C File Offset: 0x0003642C
	private IEnumerator Start()
	{
		this.m_FpsNextPeriod = Time.timeSinceLevelLoad + 0.5f;
		this.thresholdMax = 8;
		this.thresholdMin = -30;
		this.BatCam = GameObject.FindGameObjectWithTag("BatCam").GetComponent<Camera>();
		this.TreeCam = GameObject.FindGameObjectWithTag("TreeCam").GetComponent<Camera>();
		this.UICam = GameObject.FindGameObjectWithTag("UICam").GetComponent<Camera>();
		this.mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		this.bkgCam = GameObject.FindGameObjectWithTag("BackgroundCamera").GetComponent<Camera>();
		this.Cumulus = GameObject.FindGameObjectWithTag("Cumulus").GetComponent<FogVolume>();
		this.Stratus = GameObject.FindGameObjectWithTag("Stratus").GetComponent<FogVolume>();
		this.Space = GameObject.FindGameObjectWithTag("Space").GetComponent<FogVolume>();
		this.FogVolumeSurrogate = GameObject.FindGameObjectWithTag("FogVolumeSurrogate").GetComponent<MeshRenderer>();
		this.DisableClouds();
		this.sf = this.mainCam.GetComponent<ScreenFader>();
		this.canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		this.player = GameObject.FindGameObjectWithTag("PlayerMesh").GetComponent<SkinnedMeshRenderer>();
		this.pot = GameObject.FindGameObjectWithTag("PotMesh").GetComponent<MeshRenderer>();
		this.hammer = GameObject.FindGameObjectWithTag("HammerMesh").GetComponent<SkinnedMeshRenderer>();
		this.gcMan = base.GetComponent<GamecenterManager>();
		this.SpaceParticles = GameObject.FindGameObjectWithTag("SpaceParticles").GetComponent<ParticleSystem>();
		this.SnowParticles = GameObject.FindGameObjectWithTag("SnowParticles").GetComponent<ParticleSystem>();
		this.WaterParticles = GameObject.FindGameObjectWithTag("WaterParticles").GetComponent<ParticleSystem>();
		this.DebrisParticles = GameObject.FindGameObjectWithTag("DebrisParticles").GetComponent<ParticleSystem>();
		this.SparkParticles = GameObject.FindGameObjectWithTag("SparkParticles").GetComponent<ParticleSystem>();
		this.fxpro = this.bkgCam.GetComponent<FxPro>();
		base.gameObject.transform.SetParent(this.canvas.gameObject.transform, false);
		this.rect = base.gameObject.GetComponent<RectTransform>();
		this.rect.anchorMin = new Vector2(0f, 0f);
		this.rect.anchorMax = new Vector2(1f, 1f);
		this.rect.pivot = new Vector2(0.5f, 0.5f);
		RectTransform rectTransform = this.rect;
		Vector2 zero = Vector2.zero;
		this.rect.offsetMax = zero;
		rectTransform.offsetMin = zero;
		this.qs = this.getMobileQualitySettings();
		this.origSettings = this.qs;
		LODGroup[] lodTrees = new LODGroup[0];
		lodTrees = global::UnityEngine.Object.FindObjectsOfType<LODGroup>();
		foreach (LODGroup lodgroup in lodTrees)
		{
			if (lodgroup.gameObject.name == "Broadleaf_Desktop")
			{
				lodgroup.ForceLOD(2);
			}
			else if (lodgroup.gameObject.name == "Deadtree")
			{
				lodgroup.ForceLOD(0);
			}
			else
			{
				switch (this.qs.DOF)
				{
				case MobileManager.MobileDOF.Off:
					lodgroup.ForceLOD(2);
					break;
				case MobileManager.MobileDOF.Low:
					lodgroup.ForceLOD(2);
					break;
				case MobileManager.MobileDOF.Medium:
					lodgroup.ForceLOD(1);
					break;
				case MobileManager.MobileDOF.High:
					lodgroup.ForceLOD(1);
					break;
				}
			}
		}
		this.fvr = this.bkgCam.GetComponent<FogVolumeRenderer>();
		switch (this.qs.CloudDownsample)
		{
		case MobileManager.MobileRayMCloudDownsample.Twelve:
			this.fvr._Downsample = 12;
			break;
		case MobileManager.MobileRayMCloudDownsample.Ten:
			this.fvr._Downsample = 10;
			break;
		case MobileManager.MobileRayMCloudDownsample.Eight:
			this.fvr._Downsample = 8;
			break;
		case MobileManager.MobileRayMCloudDownsample.Six:
			this.fvr._Downsample = 6;
			break;
		}
		switch (this.qs.CloudIterations)
		{
		case MobileManager.MobileCloudIterations.Disabled:
			this.Cumulus.VolumeFogInscattering = false;
			this.Cumulus._DirectionalLighting = false;
			this.Stratus.VolumeFogInscattering = false;
			this.Stratus._DirectionalLighting = false;
			this.Space.VolumeFogInscattering = false;
			this.Space._DirectionalLighting = false;
			break;
		case MobileManager.MobileCloudIterations.TenTenTen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 10;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 10;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 10;
			break;
		case MobileManager.MobileCloudIterations.FifteenFifteenFifteen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 15;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 15;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 15;
			break;
		case MobileManager.MobileCloudIterations.TwentyTwentyFifteen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 20;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 20;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 15;
			break;
		case MobileManager.MobileCloudIterations.TwentyFiveTwentyFiveTwenty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 25;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 25;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 20;
			break;
		case MobileManager.MobileCloudIterations.ThirtyThirtyFiveThirty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 30;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 35;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 30;
			break;
		case MobileManager.MobileCloudIterations.FourtySixtyFourty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 40;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 60;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 40;
			break;
		}
		ParticleSystem.MainModule snowM = this.SnowParticles.main;
		ParticleSystem.MainModule spaceM = this.SpaceParticles.main;
		ParticleSystem.CollisionModule waterM = this.WaterParticles.collision;
		ParticleSystem.CollisionModule debrisM = this.DebrisParticles.collision;
		ParticleSystem.CollisionModule sparksM = this.SparkParticles.collision;
		MobileManager.MobileMaxParticles maxParticles = this.qs.MaxParticles;
		if (maxParticles != MobileManager.MobileMaxParticles.FiveHundred)
		{
			if (maxParticles == MobileManager.MobileMaxParticles.OneThousand)
			{
				snowM.maxParticles = 1000;
				spaceM.maxParticles = 1000;
			}
		}
		else
		{
			snowM.maxParticles = 500;
			spaceM.maxParticles = 500;
		}
		waterM.enabled = false;
		debrisM.enabled = false;
		sparksM.enabled = false;
		MeshRenderer[] mrs = global::UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		SkinnedMeshRenderer[] smrs = global::UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
		foreach (MeshRenderer meshRenderer in mrs)
		{
			meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in smrs)
		{
			skinnedMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		this.player.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		this.pot.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		this.hammer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		switch (this.qs.Scale)
		{
		case MobileManager.MobileScale.PtFifty:
			this.fxpro.DOFParams.DOFBlurSize = 0.318f;
			break;
		case MobileManager.MobileScale.PtSeventyFive:
			this.fxpro.DOFParams.DOFBlurSize = 0.477f;
			break;
		case MobileManager.MobileScale.One:
			this.fxpro.DOFParams.DOFBlurSize = 0.636f;
			break;
		case MobileManager.MobileScale.None:
			this.fxpro.DOFParams.DOFBlurSize = 1.272f;
			break;
		}
		this.fxpro.FilmGrainIntensity = ((!this.qs.Grain) ? 0f : 0.12f);
		this.fxpro.VignettingIntensity = ((!this.qs.Vignetting) ? 0f : 0.671f);
		switch (this.qs.DOF)
		{
		case MobileManager.MobileDOF.Off:
			this.fxpro.enabled = false;
			this.fxpro.DOFEnabled = false;
			break;
		case MobileManager.MobileDOF.Low:
			this.fxpro.enabled = true;
			this.fxpro.DOFEnabled = true;
			break;
		case MobileManager.MobileDOF.Medium:
			this.fxpro.enabled = true;
			this.fxpro.DOFEnabled = true;
			break;
		case MobileManager.MobileDOF.High:
			this.fxpro.enabled = true;
			this.fxpro.DOFEnabled = true;
			break;
		}
		this.fxpro.Init(false);
		yield return null;
		this.TreesOrigPos = new Vector3[this.Trees.Length];
		for (int m = 0; m < this.Trees.Length; m++)
		{
			this.TreesOrigPos[m] = this.Trees[m].transform.position;
			if (this.Trees[m].name.Contains("(1)"))
			{
				this.Trees[m].layer = 0;
				IEnumerator enumerator = this.Trees[m].transform.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform = (Transform)obj;
						transform.gameObject.layer = 0;
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = enumerator as IDisposable) != null)
					{
						disposable.Dispose();
					}
				}
			}
			this.Trees[m].transform.position = this.player.transform.position;
			this.Trees[m].SetActive(false);
		}
		this.Trees[0].SetActive(true);
		yield return null;
		for (int i = 1; i < this.Trees.Length; i++)
		{
			this.Trees[i - 1].SetActive(false);
			this.Trees[i].SetActive(true);
			yield return null;
			if (this.Trees[i].name.Contains("(1)"))
			{
				this.Trees[i].layer = 20;
				IEnumerator enumerator2 = this.Trees[i].transform.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						object obj2 = enumerator2.Current;
						Transform transform2 = (Transform)obj2;
						transform2.gameObject.layer = 20;
					}
				}
				finally
				{
					IDisposable disposable2;
					if ((disposable2 = enumerator2 as IDisposable) != null)
					{
						disposable2.Dispose();
					}
				}
			}
			this.Trees[i].transform.position = this.TreesOrigPos[i];
		}
		this.Trees[0].transform.position = this.TreesOrigPos[0];
		yield return null;
		this.BatCam.clearFlags = CameraClearFlags.Depth;
		this.TreeCam.clearFlags = CameraClearFlags.Depth;
		this.UICam.clearFlags = CameraClearFlags.Depth;
		this.mainCam.clearFlags = CameraClearFlags.Depth;
		yield return null;
		if (this.qs.CloudIterations != MobileManager.MobileCloudIterations.Disabled)
		{
			this.ToggleClouds(true);
		}
		yield return null;
		if (PlayerPrefs.HasKey("DisplayMode"))
		{
			switch (PlayerPrefs.GetInt("DisplayMode"))
			{
			case 0:
				this.BeautifulOn();
				break;
			case 1:
				this.SixtyFramesOn();
				break;
			case 2:
				this.BatterySaverOn();
				break;
			case 3:
				Debug.LogWarning("Streaming mode at start");
				this.BeautifulOn();
				break;
			}
		}
		else
		{
			Debug.Log("[MobileManager] systemMemorySize = " + SystemInfo.systemMemorySize);
			if (this.AndroidSpec == MobileManager.AndroidSpecs.High)
			{
				this.BeautifulOn();
			}
			else if (this.AndroidSpec == MobileManager.AndroidSpecs.Medium)
			{
				this.SixtyFramesOn();
			}
			else
			{
				this.BatterySaverOn();
			}
		}
		foreach (GameObject gameObject in this.Trees)
		{
			while (!gameObject.activeInHierarchy)
			{
				gameObject.SetActive(true);
			}
		}
		this.sf.StartScene();
		this.Ready = true;
		if (!Application.isEditor)
		{
			this.gcMan.InitializeGameCenter();
		}
		Debug.unityLogger.logEnabled = false;
		yield break;
	}

	// Token: 0x06000694 RID: 1684 RVA: 0x00038048 File Offset: 0x00036448
	public MobileManager.MobileQualitySettings getMobileQualitySettings()
	{
		MobileManager.MobileQualitySettings mobileQualitySettings = new MobileManager.MobileQualitySettings();
		switch (this.AndroidSpec)
		{
		case MobileManager.AndroidSpecs.Low:
			mobileQualitySettings = this.AllMobileQualitySettings[24];
			break;
		case MobileManager.AndroidSpecs.Medium:
			mobileQualitySettings = this.AllMobileQualitySettings[23];
			break;
		case MobileManager.AndroidSpecs.High:
			mobileQualitySettings = this.AllMobileQualitySettings[22];
			break;
		default:
			mobileQualitySettings = this.AllMobileQualitySettings[24];
			break;
		}
		return mobileQualitySettings;
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x000380C6 File Offset: 0x000364C6
	public MobileManager.MobileScale getDeviceScale()
	{
		if (this.qs == null)
		{
			Debug.LogError("MobileManager qs was null in getDeviceScale. Filling defaults.");
			this.qs = this.getMobileQualitySettings();
		}
		return this.qs.Scale;
	}

	// Token: 0x06000696 RID: 1686 RVA: 0x000380F4 File Offset: 0x000364F4
	public void ToggleClouds(bool enabled)
	{
		if (this.origSettings.CloudIterations == MobileManager.MobileCloudIterations.Disabled || !enabled)
		{
			this.DisableClouds();
		}
		else
		{
			base.StartCoroutine(this.EnableClouds());
		}
	}

	// Token: 0x06000697 RID: 1687 RVA: 0x00038124 File Offset: 0x00036524
	public IEnumerator EnableClouds()
	{
		this.FogVolumeSurrogate.enabled = true;
		yield return new WaitForEndOfFrame();
		this.Cumulus.gameObject.SetActive(true);
		this.Stratus.gameObject.SetActive(true);
		this.Space.gameObject.SetActive(true);
		yield return new WaitForEndOfFrame();
		this.bkgCam.GetComponent<FogVolumeRenderer>().enabled = true;
		yield break;
	}

	// Token: 0x06000698 RID: 1688 RVA: 0x00038140 File Offset: 0x00036540
	private void DisableClouds()
	{
		this.Cumulus.gameObject.SetActive(false);
		this.Stratus.gameObject.SetActive(false);
		this.Space.gameObject.SetActive(false);
		this.FogVolumeSurrogate.enabled = false;
		this.bkgCam.GetComponent<FogVolumeRenderer>().enabled = false;
	}

	// Token: 0x06000699 RID: 1689 RVA: 0x000381A0 File Offset: 0x000365A0
	public void TogglePostProc(bool enabled)
	{
		if (enabled)
		{
			this.fxpro.enabled = true;
			this.fxpro.DOFEnabled = this.origSettings.DOF != MobileManager.MobileDOF.Off;
			this.fxpro.FilmGrainIntensity = ((!this.origSettings.Grain) ? 0f : 0.012f);
			this.fxpro.VignettingIntensity = ((!this.origSettings.Vignetting) ? 0f : 0.671f);
			this.fxpro.Init(false);
		}
		else
		{
			this.fxpro.enabled = false;
		}
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x0003824C File Offset: 0x0003664C
	private void Update()
	{
		this.m_FpsAccumulator += 1f;
		if (Time.timeSinceLevelLoad > this.m_FpsNextPeriod)
		{
			this.m_CurrentFps = this.m_FpsAccumulator / 0.5f;
			this.m_FpsAccumulator = 0f;
			this.m_FpsNextPeriod += 0.5f;
			if (Time.timeSinceLevelLoad < 5f)
			{
				return;
			}
			if (this.myDisplayMode != MobileManager.DisplayMode.Beautiful)
			{
				return;
			}
			if (this.origSettings == null)
			{
				return;
			}
			if (this.origSettings.CloudIterations == MobileManager.MobileCloudIterations.Disabled)
			{
				return;
			}
			if (this.m_CurrentFps <= 29f && !this.minedOut)
			{
				if (this.hysteresis > 0)
				{
					this.hysteresis = 0;
				}
				this.hysteresis--;
				if (this.hysteresis < this.thresholdMin)
				{
					if (this.qs.CloudIterations != MobileManager.MobileCloudIterations.Disabled)
					{
						this.qs.CloudIterations = this.qs.CloudIterations - 1;
					}
					if (this.qs.CloudDownsample != MobileManager.MobileRayMCloudDownsample.Twelve)
					{
						this.qs.CloudDownsample = this.qs.CloudDownsample - 1;
					}
					if (this.qs.CloudIterations == MobileManager.MobileCloudIterations.Disabled && this.qs.CloudDownsample == MobileManager.MobileRayMCloudDownsample.Twelve)
					{
						this.minedOut = true;
					}
					this.RefreshCloudSettings();
					this.maxedOut = false;
					this.hysteresis = 0;
				}
			}
			else if (this.m_CurrentFps > 29f && !this.maxedOut)
			{
				this.hysteresis++;
				if (this.hysteresis > this.thresholdMax)
				{
					if (this.thresholdMax >= 15360 && this.qs.CloudIterations == MobileManager.MobileCloudIterations.Disabled)
					{
						return;
					}
					this.thresholdMax *= 2;
					if (this.qs.CloudIterations != this.origSettings.CloudIterations)
					{
						this.qs.CloudIterations = this.qs.CloudIterations + 1;
					}
					if (this.qs.CloudDownsample != this.origSettings.CloudDownsample)
					{
						this.qs.CloudDownsample = this.qs.CloudDownsample + 1;
					}
					if (this.qs.CloudIterations == this.origSettings.CloudIterations && this.qs.CloudDownsample == this.origSettings.CloudDownsample)
					{
						this.maxedOut = true;
					}
					this.RefreshCloudSettings();
					this.minedOut = false;
					this.hysteresis = 0;
				}
			}
		}
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x000384E8 File Offset: 0x000368E8
	public void RefreshCloudSettings()
	{
		switch (this.qs.CloudIterations)
		{
		case MobileManager.MobileCloudIterations.Disabled:
			this.Cumulus.VolumeFogInscattering = false;
			this.Cumulus._DirectionalLighting = false;
			this.Stratus.VolumeFogInscattering = false;
			this.Stratus._DirectionalLighting = false;
			this.Space.VolumeFogInscattering = false;
			this.Space._DirectionalLighting = false;
			break;
		case MobileManager.MobileCloudIterations.TenTenTen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 10;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 10;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 10;
			break;
		case MobileManager.MobileCloudIterations.FifteenFifteenFifteen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 15;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 15;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 15;
			break;
		case MobileManager.MobileCloudIterations.TwentyTwentyFifteen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 20;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 20;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 15;
			break;
		case MobileManager.MobileCloudIterations.TwentyFiveTwentyFiveTwenty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 25;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 25;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 20;
			break;
		case MobileManager.MobileCloudIterations.ThirtyThirtyFiveThirty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 30;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 35;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 30;
			break;
		case MobileManager.MobileCloudIterations.FourtySixtyFourty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 40;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 60;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 40;
			break;
		}
		if (this.fvr == null)
		{
			this.fvr = this.bkgCam.GetComponent<FogVolumeRenderer>();
		}
		switch (this.qs.CloudDownsample)
		{
		case MobileManager.MobileRayMCloudDownsample.Twelve:
			this.fvr._Downsample = 12;
			break;
		case MobileManager.MobileRayMCloudDownsample.Ten:
			this.fvr._Downsample = 10;
			break;
		case MobileManager.MobileRayMCloudDownsample.Eight:
			this.fvr._Downsample = 8;
			break;
		case MobileManager.MobileRayMCloudDownsample.Six:
			this.fvr._Downsample = 6;
			break;
		}
		this.ToggleClouds(true);
	}

	// Token: 0x0600069C RID: 1692 RVA: 0x000388C4 File Offset: 0x00036CC4
	public void BeautifulOn()
	{
		if (this.myDisplayMode == MobileManager.DisplayMode.Streaming)
		{
			return;
		}
		this.myDisplayMode = MobileManager.DisplayMode.Beautiful;
		this.ToggleClouds(true);
		this.TogglePostProc(true);
		Application.targetFrameRate = 60;
		base.StartCoroutine(PlayerControl.AdjustScreenResolution(1f));
		QualitySettings.SetQualityLevel(4);
		QualitySettings.vSyncCount = 0;
		PlayerPrefs.SetInt("DisplayMode", 0);
		PlayerPrefs.Save();
		this.PrettyButton.SetActive(true);
		this.SixtyFPSButton.SetActive(false);
		this.BatterySaverButton.SetActive(false);
		this.StreamingButton.SetActive(false);
	}

	// Token: 0x0600069D RID: 1693 RVA: 0x00038958 File Offset: 0x00036D58
	public void SixtyFramesOn()
	{
		if (this.myDisplayMode == MobileManager.DisplayMode.Streaming)
		{
			return;
		}
		this.myDisplayMode = MobileManager.DisplayMode.SixtyFrames;
		this.ToggleClouds(false);
		this.TogglePostProc(false);
		Application.targetFrameRate = 60;
		base.StartCoroutine(PlayerControl.AdjustScreenResolution(1f));
		QualitySettings.SetQualityLevel(2);
		QualitySettings.vSyncCount = 0;
		PlayerPrefs.SetInt("DisplayMode", 1);
		PlayerPrefs.Save();
		this.PrettyButton.SetActive(false);
		this.SixtyFPSButton.SetActive(true);
		this.BatterySaverButton.SetActive(false);
		this.StreamingButton.SetActive(false);
	}

	// Token: 0x0600069E RID: 1694 RVA: 0x000389EC File Offset: 0x00036DEC
	public void BatterySaverOn()
	{
		if (this.myDisplayMode == MobileManager.DisplayMode.Streaming)
		{
			return;
		}
		this.myDisplayMode = MobileManager.DisplayMode.BatterySaver;
		this.ToggleClouds(false);
		this.TogglePostProc(false);
		Application.targetFrameRate = 30;
		base.StartCoroutine(PlayerControl.AdjustScreenResolution(0.5f));
		QualitySettings.SetQualityLevel(0);
		QualitySettings.vSyncCount = 0;
		PlayerPrefs.SetInt("DisplayMode", 2);
		PlayerPrefs.Save();
		this.PrettyButton.SetActive(false);
		this.SixtyFPSButton.SetActive(false);
		this.BatterySaverButton.SetActive(true);
		this.StreamingButton.SetActive(false);
	}

	// Token: 0x0600069F RID: 1695 RVA: 0x00038A80 File Offset: 0x00036E80
	public void StreamingOn()
	{
		if (this.myDisplayMode == MobileManager.DisplayMode.Streaming)
		{
			return;
		}
		this.lastDisplayMode = this.myDisplayMode;
		this.myDisplayMode = MobileManager.DisplayMode.Streaming;
		this.ToggleClouds(false);
		this.TogglePostProc(false);
		Application.targetFrameRate = 30;
		PlayerPrefs.SetInt("DisplayMode", 3);
		PlayerPrefs.Save();
		this.PrettyButton.SetActive(false);
		this.SixtyFPSButton.SetActive(false);
		this.BatterySaverButton.SetActive(false);
		this.StreamingButton.SetActive(true);
	}

	// Token: 0x060006A0 RID: 1696 RVA: 0x00038B04 File Offset: 0x00036F04
	public void StreamingOff()
	{
		if (this.myDisplayMode != MobileManager.DisplayMode.Streaming)
		{
			Debug.LogWarning("Attempting to turn of streaming before turning it on.");
			if (this.lastDisplayMode == MobileManager.DisplayMode.Streaming)
			{
				this.myDisplayMode = MobileManager.DisplayMode.Beautiful;
			}
		}
		else
		{
			this.myDisplayMode = this.lastDisplayMode;
		}
		switch (this.myDisplayMode)
		{
		case MobileManager.DisplayMode.Beautiful:
			this.BeautifulOn();
			break;
		case MobileManager.DisplayMode.SixtyFrames:
			this.SixtyFramesOn();
			break;
		case MobileManager.DisplayMode.BatterySaver:
			this.BatterySaverOn();
			break;
		case MobileManager.DisplayMode.Streaming:
			Debug.LogWarning("Streaming Display was last mode while Streaming");
			this.BeautifulOn();
			break;
		}
	}

	// Token: 0x060006A1 RID: 1697 RVA: 0x00038BA4 File Offset: 0x00036FA4
	public void SetNewQualitySettings(int SettingsInt)
	{
		MobileManager.MobileQualitySettings mobileQualitySettings = this.getMobileQualitySettings();
		switch (mobileQualitySettings.DOF)
		{
		case MobileManager.MobileDOF.Off:
			this.fxpro.DOFEnabled = false;
			break;
		case MobileManager.MobileDOF.Low:
			this.fxpro.DOFEnabled = true;
			break;
		case MobileManager.MobileDOF.Medium:
			this.fxpro.DOFEnabled = true;
			break;
		case MobileManager.MobileDOF.High:
			this.fxpro.DOFEnabled = true;
			break;
		}
		this.fxpro.FilmGrainIntensity = ((!mobileQualitySettings.Grain) ? 0f : 0.099f);
		this.fxpro.VignettingIntensity = ((!mobileQualitySettings.Vignetting) ? 0f : 0.671f);
		LODGroup[] array = new LODGroup[0];
		array = global::UnityEngine.Object.FindObjectsOfType<LODGroup>();
		foreach (LODGroup lodgroup in array)
		{
			if (lodgroup.gameObject.name == "Broadleaf_Desktop")
			{
				lodgroup.ForceLOD(2);
			}
			else if (lodgroup.gameObject.name == "Deadtree")
			{
				lodgroup.ForceLOD(0);
			}
			else
			{
				switch (mobileQualitySettings.DOF)
				{
				case MobileManager.MobileDOF.Off:
					lodgroup.ForceLOD(2);
					break;
				case MobileManager.MobileDOF.Low:
					lodgroup.ForceLOD(2);
					break;
				case MobileManager.MobileDOF.Medium:
					lodgroup.ForceLOD(1);
					break;
				case MobileManager.MobileDOF.High:
					lodgroup.ForceLOD(1);
					break;
				}
			}
		}
		switch (mobileQualitySettings.CloudIterations)
		{
		case MobileManager.MobileCloudIterations.Disabled:
			this.Cumulus.VolumeFogInscattering = false;
			this.Cumulus._DirectionalLighting = false;
			this.Stratus.VolumeFogInscattering = false;
			this.Stratus._DirectionalLighting = false;
			this.Space.VolumeFogInscattering = false;
			this.Space._DirectionalLighting = false;
			break;
		case MobileManager.MobileCloudIterations.TenTenTen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 10;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 10;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 10;
			break;
		case MobileManager.MobileCloudIterations.FifteenFifteenFifteen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 15;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 15;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 15;
			break;
		case MobileManager.MobileCloudIterations.TwentyTwentyFifteen:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 20;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 20;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 15;
			break;
		case MobileManager.MobileCloudIterations.TwentyFiveTwentyFiveTwenty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 25;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 25;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 20;
			break;
		case MobileManager.MobileCloudIterations.ThirtyThirtyFiveThirty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 30;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 35;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 30;
			break;
		case MobileManager.MobileCloudIterations.FourtySixtyFourty:
			this.Cumulus.VolumeFogInscattering = true;
			this.Cumulus._DirectionalLighting = true;
			this.Cumulus.Iterations = 40;
			this.Stratus.VolumeFogInscattering = true;
			this.Stratus._DirectionalLighting = true;
			this.Stratus.Iterations = 60;
			this.Space.VolumeFogInscattering = true;
			this.Space._DirectionalLighting = true;
			this.Space.Iterations = 40;
			break;
		}
		QualitySettings.SetQualityLevel((int)mobileQualitySettings.LOD);
		this.fvr = this.bkgCam.GetComponent<FogVolumeRenderer>();
		switch (mobileQualitySettings.CloudDownsample)
		{
		case MobileManager.MobileRayMCloudDownsample.Twelve:
			this.fvr._Downsample = 12;
			break;
		case MobileManager.MobileRayMCloudDownsample.Ten:
			this.fvr._Downsample = 10;
			break;
		case MobileManager.MobileRayMCloudDownsample.Eight:
			this.fvr._Downsample = 8;
			break;
		case MobileManager.MobileRayMCloudDownsample.Six:
			this.fvr._Downsample = 6;
			break;
		}
		ParticleSystem.MainModule main = this.SnowParticles.main;
		ParticleSystem.MainModule main2 = this.SpaceParticles.main;
		ParticleSystem.CollisionModule collision = this.WaterParticles.collision;
		ParticleSystem.CollisionModule collision2 = this.DebrisParticles.collision;
		ParticleSystem.CollisionModule collision3 = this.SparkParticles.collision;
		MobileManager.MobileMaxParticles maxParticles = mobileQualitySettings.MaxParticles;
		if (maxParticles != MobileManager.MobileMaxParticles.FiveHundred)
		{
			if (maxParticles == MobileManager.MobileMaxParticles.OneThousand)
			{
				main.maxParticles = 1000;
				main2.maxParticles = 1000;
			}
		}
		else
		{
			main.maxParticles = 500;
			main2.maxParticles = 500;
		}
		collision.enabled = false;
		collision2.enabled = false;
		collision3.enabled = false;
		MeshRenderer[] array3 = global::UnityEngine.Object.FindObjectsOfType<MeshRenderer>();
		SkinnedMeshRenderer[] array4 = global::UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
		foreach (MeshRenderer meshRenderer in array3)
		{
			meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array4)
		{
			skinnedMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		}
		this.player.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		this.pot.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		this.hammer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
		this.fxpro.Init(false);
	}

	// Token: 0x060006A2 RID: 1698 RVA: 0x00039238 File Offset: 0x00037638
	private void PopulateDeviceScaling()
	{
		for (int i = 0; i < Enum.GetNames(typeof(MobileManager.iOSDeviceNames)).Length; i++)
		{
			switch (this.AllMobileQualitySettings[i].Scale)
			{
			case MobileManager.MobileScale.PtFifty:
				this.ScalePtFifty.AddRange(MobileManager.DeviceIDsbyDeviceNameList[i]);
				break;
			case MobileManager.MobileScale.PtSeventyFive:
				this.ScalePtSeventyFive.AddRange(MobileManager.DeviceIDsbyDeviceNameList[i]);
				break;
			case MobileManager.MobileScale.One:
				this.ScaleOne.AddRange(MobileManager.DeviceIDsbyDeviceNameList[i]);
				break;
			}
		}
		string text = "Assets/Mobile/DeviceScaling1.txt";
		using (FileStream fileStream = new FileStream(text, FileMode.Truncate))
		{
			using (StreamWriter streamWriter = new StreamWriter(fileStream))
			{
				foreach (string text2 in this.ScaleOne)
				{
					streamWriter.Write(text2);
					streamWriter.WriteLine();
				}
			}
		}
		text = "Assets/Mobile/DeviceScaling75.txt";
		using (FileStream fileStream2 = new FileStream(text, FileMode.Truncate))
		{
			using (StreamWriter streamWriter2 = new StreamWriter(fileStream2))
			{
				foreach (string text3 in this.ScalePtSeventyFive)
				{
					streamWriter2.Write(text3);
					streamWriter2.WriteLine();
				}
			}
		}
		text = "Assets/Mobile/DeviceScaling50.txt";
		using (FileStream fileStream3 = new FileStream(text, FileMode.Truncate))
		{
			using (StreamWriter streamWriter3 = new StreamWriter(fileStream3))
			{
				foreach (string text4 in this.ScalePtFifty)
				{
					streamWriter3.Write(text4);
					streamWriter3.WriteLine();
				}
			}
		}
	}

	// Token: 0x04000543 RID: 1347
	public static List<List<string>> DeviceIDsbyDeviceNameList = new List<List<string>>
	{
		new List<string> { "iPhone10,3", "iPhone10,6" },
		new List<string> { "iPhone10,1", "iPhone10,4" },
		new List<string> { "iPhone10,2", "iPhone10,5" },
		new List<string> { "iPhone9,1", "iPhone9,3" },
		new List<string> { "iPhone9,2", "iPhone9,4" },
		new List<string> { "iPhone8,1" },
		new List<string> { "iPhone8,2" },
		new List<string> { "iPhone8,4" },
		new List<string> { "iPhone7,2" },
		new List<string> { "iPhone7,1" },
		new List<string> { "iPhone6,1", "iPhone6,2" },
		new List<string> { "iPad7,1", "iPad7,2" },
		new List<string> { "iPad6,3", "iPad6,4" },
		new List<string> { "iPad7,3", "iPad7,4" },
		new List<string> { "iPad6,7", "iPad6,8" },
		new List<string> { "iPad6,11", "iPad6,12" },
		new List<string> { "iPad5,3", "iPad5,4" },
		new List<string> { "iPad4,1", "iPad4,2", "iPad4,3" },
		new List<string> { "iPad5,1", "iPad5,2" },
		new List<string> { "iPad4,7", "iPad4,8", "iPad4,9" },
		new List<string> { "iPad4,4", "iPad4,5", "iPad4,6" },
		new List<string> { "iPod7,1" }
	};

	// Token: 0x04000544 RID: 1348
	public List<string> ScalePtFifty = new List<string>();

	// Token: 0x04000545 RID: 1349
	public List<string> ScalePtSeventyFive = new List<string>();

	// Token: 0x04000546 RID: 1350
	public List<string> ScaleOne = new List<string>();

	// Token: 0x04000547 RID: 1351
	public bool BuildBool;

	// Token: 0x04000548 RID: 1352
	public bool BuildBool1;

	// Token: 0x04000549 RID: 1353
	public bool BuildBool2;

	// Token: 0x0400054A RID: 1354
	public bool BuildBool3;

	// Token: 0x0400054B RID: 1355
	public bool BuildBool4;

	// Token: 0x0400054C RID: 1356
	public bool BuildBool5;

	// Token: 0x0400054D RID: 1357
	public List<MobileManager.MobileQualitySettings> AllMobileQualitySettings = new List<MobileManager.MobileQualitySettings>();

	// Token: 0x0400054E RID: 1358
	public GameObject blendProbe;

	// Token: 0x0400054F RID: 1359
	public RectTransform rect;

	// Token: 0x04000550 RID: 1360
	public GameObject PrettyButton;

	// Token: 0x04000551 RID: 1361
	public GameObject SixtyFPSButton;

	// Token: 0x04000552 RID: 1362
	public GameObject BatterySaverButton;

	// Token: 0x04000553 RID: 1363
	public GameObject StreamingButton;

	// Token: 0x04000554 RID: 1364
	private Camera mainCam;

	// Token: 0x04000555 RID: 1365
	private Camera bkgCam;

	// Token: 0x04000556 RID: 1366
	private SkinnedMeshRenderer player;

	// Token: 0x04000557 RID: 1367
	private MeshRenderer pot;

	// Token: 0x04000558 RID: 1368
	private SkinnedMeshRenderer hammer;

	// Token: 0x04000559 RID: 1369
	private FogVolume Cumulus;

	// Token: 0x0400055A RID: 1370
	private FogVolume Stratus;

	// Token: 0x0400055B RID: 1371
	private FogVolume Space;

	// Token: 0x0400055C RID: 1372
	private MeshRenderer FogVolumeSurrogate;

	// Token: 0x0400055D RID: 1373
	private ParticleSystem SpaceParticles;

	// Token: 0x0400055E RID: 1374
	private ParticleSystem SnowParticles;

	// Token: 0x0400055F RID: 1375
	private ParticleSystem WaterParticles;

	// Token: 0x04000560 RID: 1376
	private ParticleSystem DebrisParticles;

	// Token: 0x04000561 RID: 1377
	private ParticleSystem SparkParticles;

	// Token: 0x04000562 RID: 1378
	private Canvas canvas;

	// Token: 0x04000563 RID: 1379
	private MobileManager.MobileQualitySettings qs = new MobileManager.MobileQualitySettings();

	// Token: 0x04000564 RID: 1380
	private MobileManager.MobileQualitySettings origSettings = new MobileManager.MobileQualitySettings();

	// Token: 0x04000565 RID: 1381
	private FogVolumeRenderer fvr;

	// Token: 0x04000566 RID: 1382
	private const float fpsMeasurePeriod = 0.5f;

	// Token: 0x04000567 RID: 1383
	private float m_FpsAccumulator;

	// Token: 0x04000568 RID: 1384
	private float m_FpsNextPeriod;

	// Token: 0x04000569 RID: 1385
	public float m_CurrentFps;

	// Token: 0x0400056A RID: 1386
	public int hysteresis;

	// Token: 0x0400056B RID: 1387
	public Camera BatCam;

	// Token: 0x0400056C RID: 1388
	public Camera TreeCam;

	// Token: 0x0400056D RID: 1389
	public Camera UICam;

	// Token: 0x0400056E RID: 1390
	private FxPro fxpro;

	// Token: 0x0400056F RID: 1391
	public MobileManager.DisplayMode myDisplayMode;

	// Token: 0x04000570 RID: 1392
	private ScreenFader sf;

	// Token: 0x04000571 RID: 1393
	public GamecenterManager gcMan;

	// Token: 0x04000572 RID: 1394
	public bool Ready;

	// Token: 0x04000573 RID: 1395
	public GameObject[] Trees = new GameObject[0];

	// Token: 0x04000574 RID: 1396
	private Vector3[] TreesOrigPos = new Vector3[0];

	// Token: 0x04000575 RID: 1397
	private bool maxedOut;

	// Token: 0x04000576 RID: 1398
	private bool minedOut;

	// Token: 0x04000577 RID: 1399
	private int minedCount;

	// Token: 0x04000578 RID: 1400
	private int maxedCount;

	// Token: 0x04000579 RID: 1401
	private int thresholdMax;

	// Token: 0x0400057A RID: 1402
	private int thresholdMin;

	// Token: 0x0400057B RID: 1403
	private bool switchCloud;

	// Token: 0x0400057C RID: 1404
	private MobileManager.DisplayMode lastDisplayMode;

	// Token: 0x020000ED RID: 237
	public enum iOSDeviceNames
	{
		// Token: 0x0400057E RID: 1406
		iPhoneX,
		// Token: 0x0400057F RID: 1407
		iPhone8,
		// Token: 0x04000580 RID: 1408
		iPhone8Plus,
		// Token: 0x04000581 RID: 1409
		iPhone7,
		// Token: 0x04000582 RID: 1410
		iPhone7Plus,
		// Token: 0x04000583 RID: 1411
		iPhone6s,
		// Token: 0x04000584 RID: 1412
		iPhone6sPlus,
		// Token: 0x04000585 RID: 1413
		iPhoneSE,
		// Token: 0x04000586 RID: 1414
		iPhone6,
		// Token: 0x04000587 RID: 1415
		iPhone6Plus,
		// Token: 0x04000588 RID: 1416
		iPhone5s,
		// Token: 0x04000589 RID: 1417
		iPadPro12gen2,
		// Token: 0x0400058A RID: 1418
		iPadPro9,
		// Token: 0x0400058B RID: 1419
		iPadPro10,
		// Token: 0x0400058C RID: 1420
		iPadPro12,
		// Token: 0x0400058D RID: 1421
		iPad5thgen,
		// Token: 0x0400058E RID: 1422
		iPadAir2,
		// Token: 0x0400058F RID: 1423
		iPadAir,
		// Token: 0x04000590 RID: 1424
		iPadMini4,
		// Token: 0x04000591 RID: 1425
		iPadMini3,
		// Token: 0x04000592 RID: 1426
		iPadMini2,
		// Token: 0x04000593 RID: 1427
		iPod6,
		// Token: 0x04000594 RID: 1428
		AndroidHigh,
		// Token: 0x04000595 RID: 1429
		AndroidMedium,
		// Token: 0x04000596 RID: 1430
		AndroidLow
	}

	// Token: 0x020000EE RID: 238
	public enum AndroidSpecs
	{
		// Token: 0x04000598 RID: 1432
		Low,
		// Token: 0x04000599 RID: 1433
		Medium,
		// Token: 0x0400059A RID: 1434
		High
	}

	// Token: 0x020000EF RID: 239
	[Serializable]
	public class MobileQualitySettings
	{
		// Token: 0x0400059B RID: 1435
		public MobileManager.iOSDeviceNames DeviceName;

		// Token: 0x0400059C RID: 1436
		public MobileManager.MobileScale Scale;

		// Token: 0x0400059D RID: 1437
		public MobileManager.MobileDOF DOF;

		// Token: 0x0400059E RID: 1438
		public bool Grain;

		// Token: 0x0400059F RID: 1439
		public bool Vignetting;

		// Token: 0x040005A0 RID: 1440
		public MobileManager.MobileLOD LOD;

		// Token: 0x040005A1 RID: 1441
		public MobileManager.MobileShadow ShadowQuality;

		// Token: 0x040005A2 RID: 1442
		public MobileManager.MobileCloudIterations CloudIterations;

		// Token: 0x040005A3 RID: 1443
		public MobileManager.MobileRayMCloudDownsample CloudDownsample;

		// Token: 0x040005A4 RID: 1444
		public MobileManager.MobileMaxParticles MaxParticles;
	}

	// Token: 0x020000F0 RID: 240
	public enum MobileScale
	{
		// Token: 0x040005A6 RID: 1446
		PtFifty,
		// Token: 0x040005A7 RID: 1447
		PtSeventyFive,
		// Token: 0x040005A8 RID: 1448
		One,
		// Token: 0x040005A9 RID: 1449
		None
	}

	// Token: 0x020000F1 RID: 241
	public enum MobileDOF
	{
		// Token: 0x040005AB RID: 1451
		Off,
		// Token: 0x040005AC RID: 1452
		Low,
		// Token: 0x040005AD RID: 1453
		Medium,
		// Token: 0x040005AE RID: 1454
		High
	}

	// Token: 0x020000F2 RID: 242
	public enum MobileLOD
	{
		// Token: 0x040005B0 RID: 1456
		PtThree,
		// Token: 0x040005B1 RID: 1457
		PtSix,
		// Token: 0x040005B2 RID: 1458
		PtSeven,
		// Token: 0x040005B3 RID: 1459
		One,
		// Token: 0x040005B4 RID: 1460
		OnePtFive
	}

	// Token: 0x020000F3 RID: 243
	public enum MobileShadow
	{
		// Token: 0x040005B6 RID: 1462
		None,
		// Token: 0x040005B7 RID: 1463
		Low,
		// Token: 0x040005B8 RID: 1464
		Medium,
		// Token: 0x040005B9 RID: 1465
		High
	}

	// Token: 0x020000F4 RID: 244
	public enum MobileCloudIterations
	{
		// Token: 0x040005BB RID: 1467
		Disabled,
		// Token: 0x040005BC RID: 1468
		TenTenTen,
		// Token: 0x040005BD RID: 1469
		FifteenFifteenFifteen,
		// Token: 0x040005BE RID: 1470
		TwentyTwentyFifteen,
		// Token: 0x040005BF RID: 1471
		TwentyFiveTwentyFiveTwenty,
		// Token: 0x040005C0 RID: 1472
		ThirtyThirtyFiveThirty,
		// Token: 0x040005C1 RID: 1473
		FourtySixtyFourty
	}

	// Token: 0x020000F5 RID: 245
	public enum MobileRayMCloudDownsample
	{
		// Token: 0x040005C3 RID: 1475
		Twelve,
		// Token: 0x040005C4 RID: 1476
		Ten,
		// Token: 0x040005C5 RID: 1477
		Eight,
		// Token: 0x040005C6 RID: 1478
		Six
	}

	// Token: 0x020000F6 RID: 246
	public enum MobileMaxParticles
	{
		// Token: 0x040005C8 RID: 1480
		FiveHundred,
		// Token: 0x040005C9 RID: 1481
		OneThousand
	}

	// Token: 0x020000F7 RID: 247
	public enum DisplayMode
	{
		// Token: 0x040005CB RID: 1483
		Beautiful,
		// Token: 0x040005CC RID: 1484
		SixtyFrames,
		// Token: 0x040005CD RID: 1485
		BatterySaver,
		// Token: 0x040005CE RID: 1486
		Streaming
	}
}
