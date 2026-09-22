using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000A3 RID: 163
public class MB3_KMeansClustering
{
	// Token: 0x06000435 RID: 1077 RVA: 0x00021700 File Offset: 0x0001FB00
	public MB3_KMeansClustering(List<GameObject> gos, int numClusters)
	{
		for (int i = 0; i < gos.Count; i++)
		{
			if (gos[i] != null)
			{
				MB3_KMeansClustering.DataPoint dataPoint = new MB3_KMeansClustering.DataPoint(gos[i]);
				this._normalizedDataToCluster.Add(dataPoint);
			}
			else
			{
				Debug.LogWarning(string.Format("Object {0} in list of objects to cluster was null.", i));
			}
		}
		if (numClusters <= 0)
		{
			Debug.LogError("Number of clusters must be posititve.");
			numClusters = 1;
		}
		if (this._normalizedDataToCluster.Count <= numClusters)
		{
			Debug.LogError("There must be fewer clusters than objects to cluster");
			numClusters = this._normalizedDataToCluster.Count - 1;
		}
		this._numberOfClusters = numClusters;
		if (this._numberOfClusters <= 0)
		{
			this._numberOfClusters = 1;
		}
		this._clusters = new Vector3[this._numberOfClusters];
	}

	// Token: 0x06000436 RID: 1078 RVA: 0x000217F0 File Offset: 0x0001FBF0
	private void InitializeCentroids()
	{
		for (int i = 0; i < this._numberOfClusters; i++)
		{
			this._normalizedDataToCluster[i].Cluster = i;
		}
		for (int j = this._numberOfClusters; j < this._normalizedDataToCluster.Count; j++)
		{
			this._normalizedDataToCluster[j].Cluster = global::UnityEngine.Random.Range(0, this._numberOfClusters);
		}
	}

	// Token: 0x06000437 RID: 1079 RVA: 0x00021864 File Offset: 0x0001FC64
	private bool UpdateDataPointMeans(bool force)
	{
		if (this.AnyAreEmpty(this._normalizedDataToCluster) && !force)
		{
			return false;
		}
		Vector3[] array = new Vector3[this._numberOfClusters];
		int[] array2 = new int[this._numberOfClusters];
		for (int i = 0; i < this._normalizedDataToCluster.Count; i++)
		{
			int cluster = this._normalizedDataToCluster[i].Cluster;
			array[cluster] += this._normalizedDataToCluster[i].center;
			array2[cluster]++;
		}
		for (int j = 0; j < this._numberOfClusters; j++)
		{
			this._clusters[j] = array[j] / (float)array2[j];
		}
		return true;
	}

	// Token: 0x06000438 RID: 1080 RVA: 0x0002194C File Offset: 0x0001FD4C
	private bool AnyAreEmpty(List<MB3_KMeansClustering.DataPoint> data)
	{
		int[] array = new int[this._numberOfClusters];
		for (int i = 0; i < this._normalizedDataToCluster.Count; i++)
		{
			array[this._normalizedDataToCluster[i].Cluster]++;
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] == 0)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000439 RID: 1081 RVA: 0x000219BC File Offset: 0x0001FDBC
	private bool UpdateClusterMembership()
	{
		bool flag = false;
		float[] array = new float[this._numberOfClusters];
		for (int i = 0; i < this._normalizedDataToCluster.Count; i++)
		{
			for (int j = 0; j < this._numberOfClusters; j++)
			{
				array[j] = this.ElucidanDistance(this._normalizedDataToCluster[i], this._clusters[j]);
			}
			int num = this.MinIndex(array);
			if (num != this._normalizedDataToCluster[i].Cluster)
			{
				flag = true;
				this._normalizedDataToCluster[i].Cluster = num;
			}
		}
		return flag;
	}

	// Token: 0x0600043A RID: 1082 RVA: 0x00021A74 File Offset: 0x0001FE74
	private float ElucidanDistance(MB3_KMeansClustering.DataPoint dataPoint, Vector3 mean)
	{
		return Vector3.Distance(dataPoint.center, mean);
	}

	// Token: 0x0600043B RID: 1083 RVA: 0x00021A84 File Offset: 0x0001FE84
	private int MinIndex(float[] distances)
	{
		int num = 0;
		double num2 = (double)distances[0];
		for (int i = 0; i < distances.Length; i++)
		{
			if ((double)distances[i] < num2)
			{
				num2 = (double)distances[i];
				num = i;
			}
		}
		return num;
	}

	// Token: 0x0600043C RID: 1084 RVA: 0x00021AC0 File Offset: 0x0001FEC0
	public List<Renderer> GetCluster(int idx, out Vector3 mean, out float size)
	{
		if (idx < 0 || idx >= this._numberOfClusters)
		{
			Debug.LogError("idx is out of bounds");
			mean = Vector3.zero;
			size = 1f;
			return new List<Renderer>();
		}
		this.UpdateDataPointMeans(true);
		List<Renderer> list = new List<Renderer>();
		mean = this._clusters[idx];
		float num = 0f;
		for (int i = 0; i < this._normalizedDataToCluster.Count; i++)
		{
			if (this._normalizedDataToCluster[i].Cluster == idx)
			{
				float num2 = Vector3.Distance(mean, this._normalizedDataToCluster[i].center);
				if (num2 > num)
				{
					num = num2;
				}
				list.Add(this._normalizedDataToCluster[i].gameObject.GetComponent<Renderer>());
			}
		}
		mean = this._clusters[idx];
		size = num;
		return list;
	}

	// Token: 0x0600043D RID: 1085 RVA: 0x00021BC0 File Offset: 0x0001FFC0
	public void Cluster()
	{
		bool flag = true;
		bool flag2 = true;
		this.InitializeCentroids();
		int num = this._normalizedDataToCluster.Count * 1000;
		int num2 = 0;
		while (flag2 && flag && num2 < num)
		{
			num2++;
			flag2 = this.UpdateDataPointMeans(false);
			flag = this.UpdateClusterMembership();
		}
	}

	// Token: 0x040003F7 RID: 1015
	private List<MB3_KMeansClustering.DataPoint> _normalizedDataToCluster = new List<MB3_KMeansClustering.DataPoint>();

	// Token: 0x040003F8 RID: 1016
	private Vector3[] _clusters = new Vector3[0];

	// Token: 0x040003F9 RID: 1017
	private int _numberOfClusters;

	// Token: 0x020000A4 RID: 164
	private class DataPoint
	{
		// Token: 0x0600043E RID: 1086 RVA: 0x00021C18 File Offset: 0x00020018
		public DataPoint(GameObject go)
		{
			this.gameObject = go;
			this.center = go.transform.position;
			if (go.GetComponent<Renderer>() == null)
			{
				Debug.LogError("Object does not have a renderer " + go);
			}
		}

		// Token: 0x040003FA RID: 1018
		public Vector3 center;

		// Token: 0x040003FB RID: 1019
		public GameObject gameObject;

		// Token: 0x040003FC RID: 1020
		public int Cluster;
	}
}
