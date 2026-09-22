using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200000B RID: 11
public class SceneMenu : MonoBehaviour
{
	// Token: 0x06000037 RID: 55 RVA: 0x0000320F File Offset: 0x0000140F
	private void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	// Token: 0x06000038 RID: 56 RVA: 0x00003220 File Offset: 0x00001420
	private void Start()
	{
		this.canvas = base.GetComponent<Canvas>();
		for (int i = 0; i < this.sceneNames.Length; i++)
		{
			Transform transform = Object.Instantiate<Transform>(this.prefabButton, Vector3.zero, Quaternion.identity);
			transform.SetParent(this.canvas.transform);
			transform.position -= Vector3.up * (float)i * 50f;
			transform.position += Vector3.up * this.Height;
			transform.GetComponentInChildren<Text>().text = this.sceneNames[i];
			string a = this.sceneNames[i];
			transform.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.onClick(a);
			});
		}
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00003309 File Offset: 0x00001509
	private void onClick(string _nameScene)
	{
		SceneManager.LoadScene(_nameScene, LoadSceneMode.Single);
	}

	// Token: 0x04000040 RID: 64
	public string[] sceneNames;

	// Token: 0x04000041 RID: 65
	public float Height = 70f;

	// Token: 0x04000042 RID: 66
	public Transform prefabButton;

	// Token: 0x04000043 RID: 67
	private Canvas canvas;
}
