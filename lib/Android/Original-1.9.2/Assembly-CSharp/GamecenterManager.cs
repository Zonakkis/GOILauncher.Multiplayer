using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

// Token: 0x020000EA RID: 234
public class GamecenterManager : MonoBehaviour
{
	// Token: 0x0600067B RID: 1659 RVA: 0x00037A0C File Offset: 0x00035E0C
	private void Start()
	{
		this.settingsMan = GameObject.FindGameObjectWithTag("SettingsManager").GetComponent<SettingsManager>();
	}

	// Token: 0x0600067C RID: 1660 RVA: 0x00037A23 File Offset: 0x00035E23
	public void InitializeGameCenter()
	{
	}

	// Token: 0x0600067D RID: 1661 RVA: 0x00037A25 File Offset: 0x00035E25
	private void logIn()
	{
	}

	// Token: 0x0600067E RID: 1662 RVA: 0x00037A28 File Offset: 0x00035E28
	private void ProcessAuthentication(bool success)
	{
		if (success)
		{
			PlayerPrefs.SetInt("GCLogin", 1);
			PlayerPrefs.Save();
			this.loggedIn = true;
			this.tryingToLogin = false;
			this.userID = Social.localUser.id;
			this.userIDs = new string[] { this.userID };
			this.numberOfWinsLocal = PlayerPrefs.GetInt("NumWins");
			this.reportScore();
			this.scoresLeaderboard = Social.CreateLeaderboard();
			this.scoresLeaderboard.id = "speedrun";
			this.scoresLeaderboard.userScope = UserScope.FriendsOnly;
			this.scoresLeaderboard.SetUserFilter(this.userIDs);
			this.scoresLeaderboard.LoadScores(delegate(bool loaded)
			{
				this.myScoreLoaded(loaded);
			});
			Social.LoadScores("speedrun", delegate(IScore[] scores)
			{
				this.scoresLoaded(scores);
			});
			this.winsLeaderboard = Social.CreateLeaderboard();
			this.winsLeaderboard.id = "numberofwins";
			this.winsLeaderboard.userScope = UserScope.FriendsOnly;
			this.winsLeaderboard.SetUserFilter(this.userIDs);
			this.winsLeaderboard.LoadScores(delegate(bool loaded)
			{
				this.winsLoaded(loaded);
			});
			this.gamecenterButton.SetActive(false);
			this.leaderboardsButton.SetActive(true);
		}
		else
		{
			this.tryingToLogin = false;
			this.gamecenterButton.SetActive(true);
			this.leaderboardsButton.SetActive(false);
		}
	}

	// Token: 0x0600067F RID: 1663 RVA: 0x00037B84 File Offset: 0x00035F84
	private void winsLoaded(bool loaded)
	{
		if (loaded && this.winsLeaderboard.localUserScore != null)
		{
			this.numberOfWinsRemote = (int)this.winsLeaderboard.localUserScore.value;
			if (this.numberOfWinsLocal > this.numberOfWinsRemote)
			{
				if (this.isACheater && (this.numberOfWinsLocal > 1 || this.numberOfWinsRemote > 1))
				{
					this.punishCheater();
				}
				Social.ReportScore((long)this.numberOfWinsLocal, "numberofwins", delegate(bool result)
				{
					if (!result)
					{
						Debug.LogWarning("Scores submission failed");
					}
				});
			}
			else if (this.numberOfWinsRemote > this.numberOfWinsLocal)
			{
				if (this.isACheater && (this.numberOfWinsLocal > 1 || this.numberOfWinsRemote > 1))
				{
					this.punishCheater();
				}
				PlayerPrefs.SetInt("NumWins", this.numberOfWinsRemote);
				if (this.settingsMan != null)
				{
					this.settingsMan.UpdateGoldPot();
				}
			}
		}
	}

	// Token: 0x06000680 RID: 1664 RVA: 0x00037C96 File Offset: 0x00036096
	private void myScoreLoaded(bool loaded)
	{
		if (loaded && this.scoresLeaderboard.localUserScore == null)
		{
			this.isACheater = true;
		}
	}

	// Token: 0x06000681 RID: 1665 RVA: 0x00037CB5 File Offset: 0x000360B5
	private void punishCheater()
	{
		Social.ReportScore(-5507L, "numberofwins", delegate(bool result)
		{
			if (!result)
			{
				Debug.LogWarning("Scores submission failed");
			}
		});
	}

	// Token: 0x06000682 RID: 1666 RVA: 0x00037CE4 File Offset: 0x000360E4
	private void scoresLoaded(IScore[] scores)
	{
		if (scores != null && scores.Length > 0)
		{
			this.nClimbersAtTop = scores.Length;
			PlayerPrefs.SetInt("playersAtTop", this.nClimbersAtTop);
			PlayerPrefs.Save();
		}
	}

	// Token: 0x06000683 RID: 1667 RVA: 0x00037D13 File Offset: 0x00036113
	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			this.logIn();
		}
	}

	// Token: 0x06000684 RID: 1668 RVA: 0x00037D24 File Offset: 0x00036124
	public void reportScore()
	{
		if (!this.loggedIn)
		{
			this.logIn();
		}
		else if (!this.tryingToLogin && (PlayerPrefs.HasKey("BestTime") || PlayerPrefs.HasKey("LastTime")))
		{
			float num;
			if (PlayerPrefs.HasKey("BestTime"))
			{
				num = PlayerPrefs.GetFloat("BestTime");
			}
			else
			{
				num = PlayerPrefs.GetFloat("LastTime");
			}
			num = (float)Mathf.FloorToInt(num * 100f);
			if (num <= 1f)
			{
				Social.ReportScore(550755075507L, "speedrun", delegate(bool result)
				{
					if (!result)
					{
						Debug.LogWarning("Scores submission failed");
					}
				});
			}
			long num2 = (long)num;
			Social.ReportScore(num2, "speedrun", delegate(bool result)
			{
				if (!result)
				{
					Debug.LogWarning("Scores submission failed");
				}
			});
		}
	}

	// Token: 0x06000685 RID: 1669 RVA: 0x00037E0E File Offset: 0x0003620E
	public void showLeaderboards()
	{
		if (this.loggedIn)
		{
			Social.ShowLeaderboardUI();
		}
		else
		{
			this.logIn();
		}
	}

	// Token: 0x04000530 RID: 1328
	public GameObject leaderboardsButton;

	// Token: 0x04000531 RID: 1329
	public GameObject gamecenterButton;

	// Token: 0x04000532 RID: 1330
	public bool loggedIn;

	// Token: 0x04000533 RID: 1331
	public bool tryingToLogin;

	// Token: 0x04000534 RID: 1332
	public int nClimbersAtTop;

	// Token: 0x04000535 RID: 1333
	public int numberOfWinsLocal;

	// Token: 0x04000536 RID: 1334
	public int numberOfWinsRemote;

	// Token: 0x04000537 RID: 1335
	private SettingsManager settingsMan;

	// Token: 0x04000538 RID: 1336
	private ILeaderboard winsLeaderboard;

	// Token: 0x04000539 RID: 1337
	private string userID;

	// Token: 0x0400053A RID: 1338
	private ILeaderboard scoresLeaderboard;

	// Token: 0x0400053B RID: 1339
	private string[] userIDs;

	// Token: 0x0400053C RID: 1340
	private bool isACheater;
}
