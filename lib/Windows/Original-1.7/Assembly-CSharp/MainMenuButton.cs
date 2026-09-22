using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200000A RID: 10
public class MainMenuButton : MonoBehaviour
{
	// Token: 0x06000032 RID: 50 RVA: 0x00003196 File Offset: 0x00001396
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		if (MainMenuButton.menuInstance == null)
		{
			MainMenuButton.menuInstance = base.gameObject;
			return;
		}
		Object.Destroy(base.gameObject);
	}

	// Token: 0x06000033 RID: 51 RVA: 0x000031C7 File Offset: 0x000013C7
	private void Start()
	{
	}

	// Token: 0x06000034 RID: 52 RVA: 0x000031CC File Offset: 0x000013CC
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

	// Token: 0x06000035 RID: 53 RVA: 0x000031FE File Offset: 0x000013FE
	public void LoadMainMenu()
	{
		SceneManager.LoadScene(0, LoadSceneMode.Single);
	}

	// Token: 0x0400003F RID: 63
	public static GameObject menuInstance;
}
