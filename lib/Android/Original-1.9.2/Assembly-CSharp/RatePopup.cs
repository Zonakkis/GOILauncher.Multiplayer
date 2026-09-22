using System;
using UnityEngine;

// Token: 0x02000111 RID: 273
public class RatePopup : MonoBehaviour
{
	// Token: 0x06000718 RID: 1816 RVA: 0x0003E098 File Offset: 0x0003C498
	public bool ShouldPrompt()
	{
		return PlayerPrefs.GetInt("noodle.didPromptPlayerToReviewGame", 0) == 0 && this.progressMeter.progress >= this.minProgressForRating && (DateTimeOffset.UtcNow - this.progressMeter.LastSeriousLossTimestamp).TotalSeconds > 60.0;
	}

	// Token: 0x06000719 RID: 1817 RVA: 0x0003E0F6 File Offset: 0x0003C4F6
	private void OnEnable()
	{
		PlayerPrefs.SetInt("noodle.didPromptPlayerToReviewGame", 1);
	}

	// Token: 0x0600071A RID: 1818 RVA: 0x0003E103 File Offset: 0x0003C503
	public void ReviewGame()
	{
		base.gameObject.SetActive(false);
		Application.OpenURL("https://play.google.com/store/apps/details?id=com.noodlecake.gettingoverit");
	}

	// Token: 0x0400066D RID: 1645
	public ProgressMeter progressMeter;

	// Token: 0x0400066E RID: 1646
	public float minProgressForRating;
}
