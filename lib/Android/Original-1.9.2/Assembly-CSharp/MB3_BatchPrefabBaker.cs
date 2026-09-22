using System;
using UnityEngine;

// Token: 0x020000C5 RID: 197
public class MB3_BatchPrefabBaker : MonoBehaviour
{
	// Token: 0x040004B9 RID: 1209
	public MB3_BatchPrefabBaker.MB3_PrefabBakerRow[] prefabRows;

	// Token: 0x040004BA RID: 1210
	public string outputPrefabFolder;

	// Token: 0x020000C6 RID: 198
	[Serializable]
	public class MB3_PrefabBakerRow
	{
		// Token: 0x040004BB RID: 1211
		public GameObject sourcePrefab;

		// Token: 0x040004BC RID: 1212
		public GameObject resultPrefab;
	}
}
