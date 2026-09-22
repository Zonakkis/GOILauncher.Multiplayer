using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200000B RID: 11
public class MainMenuButton : MonoBehaviour
{
	// Token: 0x06000033 RID: 51 RVA: 0x00002848 File Offset: 0x00000A48
	private void Awake()
	{
		global::UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (MainMenuButton.menuInstance == null)
		{
			MainMenuButton.menuInstance = base.gameObject;
			return;
		}
		global::UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x06000034 RID: 52 RVA: 0x0000265E File Offset: 0x0000085E
	private void Start()
	{
	}

	// Token: 0x06000035 RID: 53 RVA: 0x0001500C File Offset: 0x0001320C
	private void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			if (SceneManager.GetActiveScene().buildIndex != 0)
			{
				SceneManager.LoadScene(0, LoadSceneMode.Single);
				return;
			}
			Application.Quit();
		}
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00002879 File Offset: 0x00000A79
	public void LoadMainMenu()
	{
		SceneManager.LoadScene(0, LoadSceneMode.Single);
	}

	// Token: 0x04000041 RID: 65
	public static GameObject menuInstance;
}
