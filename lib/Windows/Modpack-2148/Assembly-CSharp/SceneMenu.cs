using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200000C RID: 12
public class SceneMenu : MonoBehaviour
{
	// Token: 0x06000038 RID: 56 RVA: 0x00002882 File Offset: 0x00000A82
	private void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00015040 File Offset: 0x00013240
	private void Start()
	{
		this.canvas = base.GetComponent<Canvas>();
		for (int i = 0; i < this.sceneNames.Length; i++)
		{
			Transform transform = global::UnityEngine.Object.Instantiate<Transform>(this.prefabButton, Vector3.zero, Quaternion.identity);
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

	// Token: 0x0600003A RID: 58 RVA: 0x00002892 File Offset: 0x00000A92
	private void onClick(string _nameScene)
	{
		SceneManager.LoadScene(_nameScene, LoadSceneMode.Single);
	}

	// Token: 0x04000042 RID: 66
	public string[] sceneNames;

	// Token: 0x04000043 RID: 67
	public float Height = 70f;

	// Token: 0x04000044 RID: 68
	public Transform prefabButton;

	// Token: 0x04000045 RID: 69
	private Canvas canvas;
}
