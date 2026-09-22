using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

// Token: 0x02000116 RID: 278
public class NoodleNewsClient : MonoBehaviour
{
	// Token: 0x14000003 RID: 3
	// (add) Token: 0x0600072C RID: 1836 RVA: 0x0003E124 File Offset: 0x0003C524
	// (remove) Token: 0x0600072D RID: 1837 RVA: 0x0003E158 File Offset: 0x0003C558
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event WillShowCreative OnWillShowCreative;

	// Token: 0x14000004 RID: 4
	// (add) Token: 0x0600072E RID: 1838 RVA: 0x0003E18C File Offset: 0x0003C58C
	// (remove) Token: 0x0600072F RID: 1839 RVA: 0x0003E1C0 File Offset: 0x0003C5C0
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event DidDismissCreative OnDidDismissCreative;

	// Token: 0x14000005 RID: 5
	// (add) Token: 0x06000730 RID: 1840 RVA: 0x0003E1F4 File Offset: 0x0003C5F4
	// (remove) Token: 0x06000731 RID: 1841 RVA: 0x0003E228 File Offset: 0x0003C628
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event WillShowMoreGames OnWillShowMoreGames;

	// Token: 0x14000006 RID: 6
	// (add) Token: 0x06000732 RID: 1842 RVA: 0x0003E25C File Offset: 0x0003C65C
	// (remove) Token: 0x06000733 RID: 1843 RVA: 0x0003E290 File Offset: 0x0003C690
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public static event DidDismissMoreGames OnDidDismissMoreGames;

	// Token: 0x06000734 RID: 1844
	[DllImport("nativeNoodleNews")]
	private static extern void setNewsCreativeShowCallback(WillShowCreative showCallback);

	// Token: 0x06000735 RID: 1845
	[DllImport("nativeNoodleNews")]
	private static extern void setNewsCreativeDismissCallback(DidDismissCreative dismissCallback);

	// Token: 0x06000736 RID: 1846
	[DllImport("nativeNoodleNews")]
	private static extern void setNewsMoreGamesShowCallback(WillShowMoreGames showCallback);

	// Token: 0x06000737 RID: 1847
	[DllImport("nativeNoodleNews")]
	private static extern void setNewsMoreGamesDismissCallback(DidDismissMoreGames dismissCallback);

	// Token: 0x06000738 RID: 1848 RVA: 0x0003E2C4 File Offset: 0x0003C6C4
	public static void StartSession(string platform)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject androidJavaObject = @static.Call<AndroidJavaObject>("getIntent", new object[0]);
		if (!NoodleNewsClient._initialized)
		{
			NoodleNewsClient.noodleNewsClientClass.CallStatic("startSession", new object[] { @static, platform });
			NoodleNewsClient._initialized = true;
		}
		else if (androidJavaObject != null)
		{
			string text = androidJavaObject.Call<string>("getStringExtra", new object[] { "notification_campaign_id" });
			if (text != NoodleNewsClient._campaignFromLastIntent)
			{
				NoodleNewsClient._campaignFromLastIntent = text;
				NoodleNewsClient.noodleNewsClientClass.CallStatic("onNewIntent", new object[] { androidJavaObject });
			}
		}
		NoodleNewsClient.setNewsCreativeShowCallback(new WillShowCreative(NoodleNewsClient.InternalWillShowCreativeCallback));
		NoodleNewsClient.setNewsCreativeDismissCallback(new DidDismissCreative(NoodleNewsClient.InternalDidDismissCreativeCallback));
		NoodleNewsClient.setNewsMoreGamesShowCallback(new WillShowMoreGames(NoodleNewsClient.InternalWillShowMoreGamesCallback));
		NoodleNewsClient.setNewsMoreGamesDismissCallback(new DidDismissMoreGames(NoodleNewsClient.InternalDidDismissMoreGamesCallback));
	}

	// Token: 0x06000739 RID: 1849 RVA: 0x0003E403 File Offset: 0x0003C803
	public static void SetDebugLoggingEnabled(bool enabled)
	{
		NoodleNewsClient.noodleNewsClientClass.CallStatic("setDebugLoggingEnabled", new object[] { enabled });
	}

	// Token: 0x0600073A RID: 1850 RVA: 0x0003E423 File Offset: 0x0003C823
	public static void InternalWillShowCreativeCallback(bool shouldMuteAudio)
	{
		if (NoodleNewsClient.OnWillShowCreative != null)
		{
			NoodleNewsClient.OnWillShowCreative(shouldMuteAudio);
		}
	}

	// Token: 0x0600073B RID: 1851 RVA: 0x0003E43A File Offset: 0x0003C83A
	public static void InternalDidDismissCreativeCallback(bool positiveActionTaken)
	{
		if (NoodleNewsClient.OnDidDismissCreative != null)
		{
			NoodleNewsClient.OnDidDismissCreative(positiveActionTaken);
		}
	}

	// Token: 0x0600073C RID: 1852 RVA: 0x0003E451 File Offset: 0x0003C851
	public static void InternalWillShowMoreGamesCallback()
	{
		if (NoodleNewsClient.OnWillShowMoreGames != null)
		{
			NoodleNewsClient.OnWillShowMoreGames();
		}
	}

	// Token: 0x0600073D RID: 1853 RVA: 0x0003E467 File Offset: 0x0003C867
	public static void InternalDidDismissMoreGamesCallback()
	{
		if (NoodleNewsClient.OnDidDismissMoreGames != null)
		{
			NoodleNewsClient.OnDidDismissMoreGames();
		}
	}

	// Token: 0x0600073E RID: 1854 RVA: 0x0003E480 File Offset: 0x0003C880
	public static void ShowCreative()
	{
		bool flag = false;
		NoodleNewsClient.ShowCreative(flag);
	}

	// Token: 0x0600073F RID: 1855 RVA: 0x0003E495 File Offset: 0x0003C895
	public static void ShowCreative(bool explicitRequest)
	{
		NoodleNewsClient.noodleNewsClientClass.CallStatic("showCreative", new object[] { explicitRequest });
	}

	// Token: 0x06000740 RID: 1856 RVA: 0x0003E4B5 File Offset: 0x0003C8B5
	public static void ShowMoreGames()
	{
		NoodleNewsClient.noodleNewsClientClass.CallStatic("showMoreGames", new object[0]);
	}

	// Token: 0x06000741 RID: 1857 RVA: 0x0003E4CC File Offset: 0x0003C8CC
	public static bool ShowPushCampaign()
	{
		bool flag = NoodleNewsClient.noodleNewsClientClass.CallStatic<bool>("showPushCampaign", new object[0]);
		global::UnityEngine.Debug.Log("[NoodleNews] Tried showing pushed campaign - " + ((!flag) ? "NO" : "YES"));
		return flag;
	}

	// Token: 0x06000742 RID: 1858 RVA: 0x0003E518 File Offset: 0x0003C918
	public static string GetSupportIdentifier()
	{
		return NoodleNewsClient.noodleNewsClientClass.CallStatic<string>("getSupportIdentifier", new object[0]);
	}

	// Token: 0x06000743 RID: 1859 RVA: 0x0003E540 File Offset: 0x0003C940
	public static string GetSupportResponseContent(int id)
	{
		return NoodleNewsClient.noodleNewsClientClass.CallStatic<string>("getSupportResponseContent", new object[] { id });
	}

	// Token: 0x06000744 RID: 1860 RVA: 0x0003E570 File Offset: 0x0003C970
	public static int GetSupportResponseId()
	{
		return NoodleNewsClient.noodleNewsClientClass.CallStatic<int>("getSupportResponseID", new object[0]);
	}

	// Token: 0x06000745 RID: 1861 RVA: 0x0003E596 File Offset: 0x0003C996
	public static void AcknowledgeSupportResponse(int id)
	{
		NoodleNewsClient.noodleNewsClientClass.CallStatic("acknowledgeSupportResponse", new object[] { id });
	}

	// Token: 0x06000746 RID: 1862 RVA: 0x0003E5B8 File Offset: 0x0003C9B8
	public static bool HasPendingCreative()
	{
		bool flag = false;
		return NoodleNewsClient.HasPendingCreative(flag);
	}

	// Token: 0x06000747 RID: 1863 RVA: 0x0003E5D0 File Offset: 0x0003C9D0
	public static bool HasPendingCreative(bool explicitRequest)
	{
		return NoodleNewsClient.noodleNewsClientClass.CallStatic<bool>("hasPendingCreative", new object[] { explicitRequest });
	}

	// Token: 0x06000748 RID: 1864 RVA: 0x0003E600 File Offset: 0x0003CA00
	public static bool HasPushCampaign()
	{
		return NoodleNewsClient.noodleNewsClientClass.CallStatic<bool>("hasPushCampaign", new object[0]);
	}

	// Token: 0x06000749 RID: 1865 RVA: 0x0003E628 File Offset: 0x0003CA28
	public static int AvailableCampaignCount()
	{
		return NoodleNewsClient.noodleNewsClientClass.CallStatic<int>("availableCampaignCount", new object[0]);
	}

	// Token: 0x0600074A RID: 1866 RVA: 0x0003E650 File Offset: 0x0003CA50
	public static string GetNativeCreative()
	{
		return NoodleNewsClient.noodleNewsClientClass.CallStatic<string>("getNativeCreative", new object[0]);
	}

	// Token: 0x0600074B RID: 1867 RVA: 0x0003E676 File Offset: 0x0003CA76
	public static void HitNativeCreative(int creativeId)
	{
		NoodleNewsClient.noodleNewsClientClass.CallStatic("hitNativeCreative", new object[] { creativeId });
	}

	// Token: 0x0600074C RID: 1868 RVA: 0x0003E696 File Offset: 0x0003CA96
	public static void DismissedNativeCreative(int creativeId)
	{
		NoodleNewsClient.noodleNewsClientClass.CallStatic("dismissedNativeCreative", new object[] { creativeId });
	}

	// Token: 0x0600074D RID: 1869 RVA: 0x0003E6B8 File Offset: 0x0003CAB8
	public static void ResetCreativeViews()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.noodlecake.noodlenews.NoodleNewsClient$Debug");
		androidJavaClass.CallStatic("resetCreativeViews", new object[0]);
	}

	// Token: 0x04000673 RID: 1651
	private static AndroidJavaClass noodleNewsClientClass = new AndroidJavaClass("com.noodlecake.noodlenews.NoodleNewsClient");

	// Token: 0x04000674 RID: 1652
	private static bool _initialized = false;

	// Token: 0x04000675 RID: 1653
	private static string _campaignFromLastIntent;
}
