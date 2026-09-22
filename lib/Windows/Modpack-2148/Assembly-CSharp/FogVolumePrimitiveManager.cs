using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000022 RID: 34
public class FogVolumePrimitiveManager : MonoBehaviour
{
	// Token: 0x17000021 RID: 33
	// (get) Token: 0x060000C1 RID: 193 RVA: 0x00002D63 File Offset: 0x00000F63
	// (set) Token: 0x060000C2 RID: 194 RVA: 0x00002D6B File Offset: 0x00000F6B
	public int CurrentPrimitiveCount { get; private set; }

	// Token: 0x17000022 RID: 34
	// (get) Token: 0x060000C3 RID: 195 RVA: 0x00002D74 File Offset: 0x00000F74
	// (set) Token: 0x060000C4 RID: 196 RVA: 0x00002D7C File Offset: 0x00000F7C
	public int VisiblePrimitiveCount { get; private set; }

	// Token: 0x17000023 RID: 35
	// (get) Token: 0x060000C5 RID: 197 RVA: 0x00002D85 File Offset: 0x00000F85
	public bool AlreadyUsesTransformForPoI
	{
		get
		{
			return this.m_pointOfInterestTf != null;
		}
	}

	// Token: 0x060000C6 RID: 198 RVA: 0x0001A6CC File Offset: 0x000188CC
	public void FindPrimitivesInFogVolume()
	{
		this.CurrentPrimitiveCount = 0;
		this.VisiblePrimitiveCount = 0;
		this.m_primitives.Clear();
		this.m_primitivesInFrustum.Clear();
		for (int i = 0; i < 1000; i++)
		{
			this.m_primitives.Add(new FogVolumePrimitiveManager.PrimitiveData());
			this.m_primitivesInFrustum.Add(new FogVolumePrimitiveManager.PrimitiveData());
		}
		if (this.m_boxCollider == null)
		{
			this.m_boxCollider = base.gameObject.GetComponent<BoxCollider>();
		}
		Bounds bounds = this.m_boxCollider.bounds;
		foreach (FogVolumePrimitive fogVolumePrimitive in global::UnityEngine.Object.FindObjectsOfType<FogVolumePrimitive>())
		{
			if (bounds.Intersects(fogVolumePrimitive.Bounds))
			{
				if (fogVolumePrimitive.BoxColl != null)
				{
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Box;
				}
				else if (fogVolumePrimitive.SphereColl != null)
				{
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Sphere;
				}
				else
				{
					fogVolumePrimitive.BoxColl = fogVolumePrimitive.GetTransform.gameObject.AddComponent<BoxCollider>();
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Box;
				}
				if (fogVolumePrimitive.Type == EFogVolumePrimitiveType.Box)
				{
					this.AddPrimitiveBox(fogVolumePrimitive);
				}
				else if (fogVolumePrimitive.Type == EFogVolumePrimitiveType.Sphere)
				{
					this.AddPrimitiveSphere(fogVolumePrimitive);
				}
			}
		}
	}

	// Token: 0x060000C7 RID: 199 RVA: 0x0001A800 File Offset: 0x00018A00
	public bool AddPrimitiveBox(FogVolumePrimitive _box)
	{
		int num = this._FindFirstFreePrimitive();
		if (num != -1)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[num];
			int currentPrimitiveCount = this.CurrentPrimitiveCount;
			this.CurrentPrimitiveCount = currentPrimitiveCount + 1;
			primitiveData.PrimitiveType = EFogVolumePrimitiveType.Box;
			primitiveData.Transform = _box.transform;
			primitiveData.Renderer = _box.GetComponent<Renderer>();
			primitiveData.Primitive = _box;
			primitiveData.Bounds = new Bounds(primitiveData.Transform.position, _box.GetPrimitiveScale);
			return true;
		}
		return false;
	}

	// Token: 0x060000C8 RID: 200 RVA: 0x0001A878 File Offset: 0x00018A78
	public bool AddPrimitiveSphere(FogVolumePrimitive _sphere)
	{
		int num = this._FindFirstFreePrimitive();
		if (num != -1)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[num];
			int currentPrimitiveCount = this.CurrentPrimitiveCount;
			this.CurrentPrimitiveCount = currentPrimitiveCount + 1;
			primitiveData.PrimitiveType = EFogVolumePrimitiveType.Sphere;
			primitiveData.Transform = _sphere.transform;
			primitiveData.Renderer = _sphere.GetComponent<Renderer>();
			primitiveData.Primitive = _sphere;
			primitiveData.Bounds = new Bounds(primitiveData.Transform.position, _sphere.GetPrimitiveScale);
			return true;
		}
		return false;
	}

	// Token: 0x060000C9 RID: 201 RVA: 0x0001A8F0 File Offset: 0x00018AF0
	public bool RemovePrimitive(Transform _primitiveToRemove)
	{
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[i];
			if (this.m_primitives[i].Transform == _primitiveToRemove)
			{
				primitiveData.Reset();
				int currentPrimitiveCount = this.CurrentPrimitiveCount;
				this.CurrentPrimitiveCount = currentPrimitiveCount - 1;
				return true;
			}
		}
		return false;
	}

	// Token: 0x060000CA RID: 202 RVA: 0x00002D93 File Offset: 0x00000F93
	public void SetPointOfInterest(Vector3 _pointOfInterest)
	{
		this.m_pointOfInterestTf = null;
		this.m_pointOfInterest = _pointOfInterest;
	}

	// Token: 0x060000CB RID: 203 RVA: 0x00002DA3 File Offset: 0x00000FA3
	public void SetPointOfInterest(Transform _pointOfInterest)
	{
		this.m_pointOfInterestTf = _pointOfInterest;
	}

	// Token: 0x060000CC RID: 204 RVA: 0x00002DAC File Offset: 0x00000FAC
	public void OnDrawGizmos()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}

	// Token: 0x060000CD RID: 205 RVA: 0x0001A950 File Offset: 0x00018B50
	public void ManualUpdate(ref Plane[] _frustumPlanes)
	{
		this.m_camera = ((this.m_fogVolumeData != null) ? this.m_fogVolumeData.GameCamera : null);
		if (this.m_camera == null)
		{
			return;
		}
		this.FrustumPlanes = _frustumPlanes;
		if (this.m_boxCollider == null)
		{
			this.m_boxCollider = this.m_fogVolume.GetComponent<BoxCollider>();
		}
		this._UpdateBounds();
		this._FindPrimitivesInFrustum();
		if (this.m_primitivesInFrustum.Count > 20)
		{
			this._SortPrimitivesInFrustum();
		}
		this._PrepareShaderArrays();
	}

	// Token: 0x060000CE RID: 206 RVA: 0x0001A9DC File Offset: 0x00018BDC
	public void SetVisibility(bool _enabled)
	{
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.m_primitives[i].Renderer != null)
			{
				this.m_primitives[i].Renderer.enabled = _enabled;
			}
		}
	}

	// Token: 0x060000CF RID: 207 RVA: 0x0001AA34 File Offset: 0x00018C34
	public void Initialize()
	{
		this.m_fogVolume = base.gameObject.GetComponent<FogVolume>();
		this.m_fogVolumeData = global::UnityEngine.Object.FindObjectOfType<FogVolumeData>();
		this.m_camera = null;
		this.m_boxCollider = null;
		this.CurrentPrimitiveCount = 0;
		if (this.m_primitives == null)
		{
			this.m_primitives = new List<FogVolumePrimitiveManager.PrimitiveData>(1000);
			this.m_primitivesInFrustum = new List<FogVolumePrimitiveManager.PrimitiveData>();
			for (int i = 0; i < 1000; i++)
			{
				this.m_primitives.Add(new FogVolumePrimitiveManager.PrimitiveData());
				this.m_primitivesInFrustum.Add(new FogVolumePrimitiveManager.PrimitiveData());
			}
		}
	}

	// Token: 0x060000D0 RID: 208 RVA: 0x00002DB5 File Offset: 0x00000FB5
	public void Deinitialize()
	{
		this.VisiblePrimitiveCount = 0;
	}

	// Token: 0x060000D1 RID: 209 RVA: 0x00002DBE File Offset: 0x00000FBE
	public Vector4[] GetPrimitivePositionArray()
	{
		return this.m_primitivePos;
	}

	// Token: 0x060000D2 RID: 210 RVA: 0x00002DC6 File Offset: 0x00000FC6
	public Vector4[] GetPrimitiveScaleArray()
	{
		return this.m_primitiveScale;
	}

	// Token: 0x060000D3 RID: 211 RVA: 0x00002DCE File Offset: 0x00000FCE
	public Matrix4x4[] GetPrimitiveTransformArray()
	{
		return this.m_primitiveTf;
	}

	// Token: 0x060000D4 RID: 212 RVA: 0x00002DD6 File Offset: 0x00000FD6
	public Vector4[] GetPrimitiveDataArray()
	{
		return this.m_primitiveData;
	}

	// Token: 0x060000D5 RID: 213 RVA: 0x0001AAC8 File Offset: 0x00018CC8
	private void _UpdateBounds()
	{
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[i];
			if (primitiveData.PrimitiveType != EFogVolumePrimitiveType.None)
			{
				if (primitiveData.Primitive == null)
				{
					this.RemovePrimitive(primitiveData.Transform);
				}
				else if (primitiveData.PrimitiveType == EFogVolumePrimitiveType.Box)
				{
					if (primitiveData.Primitive.BoxColl == null)
					{
						Debug.LogWarning("FogVolumePrimitive requires a collider.\nThe collider will be automatically created.");
						primitiveData.Primitive.AddColliderIfNeccessary(EFogVolumePrimitiveType.Box);
					}
					primitiveData.Bounds = primitiveData.Primitive.BoxColl.bounds;
				}
				else if (primitiveData.PrimitiveType == EFogVolumePrimitiveType.Sphere)
				{
					if (primitiveData.Primitive.SphereColl == null)
					{
						Debug.LogWarning("FogVolumePrimitive requires a collider.\nThe collider will be automatically created.");
						primitiveData.Primitive.AddColliderIfNeccessary(EFogVolumePrimitiveType.Sphere);
					}
					primitiveData.Bounds = primitiveData.Primitive.SphereColl.bounds;
				}
			}
		}
	}

	// Token: 0x060000D6 RID: 214 RVA: 0x0001ABC0 File Offset: 0x00018DC0
	private int _FindFirstFreePrimitive()
	{
		if (this.CurrentPrimitiveCount < 1000)
		{
			int count = this.m_primitives.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.m_primitives[i].PrimitiveType == EFogVolumePrimitiveType.None)
				{
					return i;
				}
			}
		}
		return -1;
	}

	// Token: 0x060000D7 RID: 215 RVA: 0x0001AC0C File Offset: 0x00018E0C
	private void _FindPrimitivesInFrustum()
	{
		this.m_inFrustumCount = 0;
		Vector3 position = this.m_camera.gameObject.transform.position;
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[i];
			if (primitiveData.Transform == null)
			{
				primitiveData.PrimitiveType = EFogVolumePrimitiveType.None;
			}
			if (primitiveData.PrimitiveType != EFogVolumePrimitiveType.None)
			{
				if (primitiveData.Primitive.IsPersistent)
				{
					Vector3 position2 = primitiveData.Transform.position;
					primitiveData.SqDistance = (position2 - this.m_pointOfInterest).sqrMagnitude;
					primitiveData.Distance2Camera = (position2 - position).magnitude;
					List<FogVolumePrimitiveManager.PrimitiveData> primitivesInFrustum = this.m_primitivesInFrustum;
					int num = this.m_inFrustumCount;
					this.m_inFrustumCount = num + 1;
					primitivesInFrustum[num] = primitiveData;
				}
				else if (GeometryUtility.TestPlanesAABB(this.FrustumPlanes, this.m_primitives[i].Bounds))
				{
					Vector3 position3 = primitiveData.Transform.position;
					primitiveData.SqDistance = (position3 - this.m_pointOfInterest).sqrMagnitude;
					primitiveData.Distance2Camera = (position3 - position).magnitude;
					List<FogVolumePrimitiveManager.PrimitiveData> primitivesInFrustum2 = this.m_primitivesInFrustum;
					int num = this.m_inFrustumCount;
					this.m_inFrustumCount = num + 1;
					primitivesInFrustum2[num] = primitiveData;
				}
			}
		}
	}

	// Token: 0x060000D8 RID: 216 RVA: 0x0001AD70 File Offset: 0x00018F70
	private void _SortPrimitivesInFrustum()
	{
		bool flag;
		do
		{
			flag = true;
			for (int i = 0; i < this.m_inFrustumCount - 1; i++)
			{
				if (this.m_primitivesInFrustum[i].SqDistance > this.m_primitivesInFrustum[i + 1].SqDistance)
				{
					FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitivesInFrustum[i];
					this.m_primitivesInFrustum[i] = this.m_primitivesInFrustum[i + 1];
					this.m_primitivesInFrustum[i + 1] = primitiveData;
					flag = false;
				}
			}
		}
		while (!flag);
	}

	// Token: 0x060000D9 RID: 217 RVA: 0x0001ADF8 File Offset: 0x00018FF8
	private void _PrepareShaderArrays()
	{
		this.VisiblePrimitiveCount = 0;
		Quaternion rotation = this.m_fogVolume.gameObject.transform.rotation;
		int num = 0;
		while (num < 20 && num < this.m_inFrustumCount)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitivesInFrustum[num];
			Vector3 position = primitiveData.Transform.position;
			this.m_primitivePos[num] = base.gameObject.transform.InverseTransformPoint(position);
			this.m_primitiveTf[num].SetTRS(position, Quaternion.Inverse(primitiveData.Transform.rotation) * rotation, Vector3.one);
			this.m_primitiveScale[num] = primitiveData.Primitive.GetPrimitiveScale;
			this.m_primitiveData[num] = new Vector4((primitiveData.PrimitiveType == EFogVolumePrimitiveType.Box) ? 0.5f : 1.5f, primitiveData.Primitive.IsSubtractive ? 1.5f : 0.5f, 0f, 0f);
			int visiblePrimitiveCount = this.VisiblePrimitiveCount;
			this.VisiblePrimitiveCount = visiblePrimitiveCount + 1;
			num++;
		}
	}

	// Token: 0x04000182 RID: 386
	private FogVolume m_fogVolume;

	// Token: 0x04000183 RID: 387
	private FogVolumeData m_fogVolumeData;

	// Token: 0x04000184 RID: 388
	private Camera m_camera;

	// Token: 0x04000185 RID: 389
	private BoxCollider m_boxCollider;

	// Token: 0x04000186 RID: 390
	private Transform m_pointOfInterestTf;

	// Token: 0x04000187 RID: 391
	private Vector3 m_pointOfInterest = Vector3.zero;

	// Token: 0x04000188 RID: 392
	private List<FogVolumePrimitiveManager.PrimitiveData> m_primitives;

	// Token: 0x04000189 RID: 393
	private List<FogVolumePrimitiveManager.PrimitiveData> m_primitivesInFrustum;

	// Token: 0x0400018A RID: 394
	private int m_inFrustumCount;

	// Token: 0x0400018B RID: 395
	private Plane[] FrustumPlanes;

	// Token: 0x0400018C RID: 396
	private readonly Vector4[] m_primitivePos = new Vector4[20];

	// Token: 0x0400018D RID: 397
	private readonly Vector4[] m_primitiveScale = new Vector4[20];

	// Token: 0x0400018E RID: 398
	private readonly Matrix4x4[] m_primitiveTf = new Matrix4x4[20];

	// Token: 0x0400018F RID: 399
	private readonly Vector4[] m_primitiveData = new Vector4[20];

	// Token: 0x04000190 RID: 400
	private const int InvalidIndex = -1;

	// Token: 0x04000191 RID: 401
	private const int MaxVisiblePrimitives = 20;

	// Token: 0x04000192 RID: 402
	private const int MaxPrimitivesCount = 1000;

	// Token: 0x02000023 RID: 35
	protected class PrimitiveData
	{
		// Token: 0x060000DB RID: 219 RVA: 0x0001AF74 File Offset: 0x00019174
		public PrimitiveData()
		{
			this.PrimitiveType = EFogVolumePrimitiveType.None;
			this.Primitive = null;
			this.Transform = null;
			this.Renderer = null;
			this.SqDistance = 0f;
			this.Distance2Camera = 0f;
			this.Bounds = default(Bounds);
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000DC RID: 220 RVA: 0x00002DDE File Offset: 0x00000FDE
		// (set) Token: 0x060000DD RID: 221 RVA: 0x00002DE6 File Offset: 0x00000FE6
		public EFogVolumePrimitiveType PrimitiveType { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000DE RID: 222 RVA: 0x00002DEF File Offset: 0x00000FEF
		// (set) Token: 0x060000DF RID: 223 RVA: 0x00002DF7 File Offset: 0x00000FF7
		public FogVolumePrimitive Primitive { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00002E00 File Offset: 0x00001000
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x00002E08 File Offset: 0x00001008
		public Transform Transform { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x00002E11 File Offset: 0x00001011
		// (set) Token: 0x060000E3 RID: 227 RVA: 0x00002E19 File Offset: 0x00001019
		public Renderer Renderer { get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x00002E22 File Offset: 0x00001022
		// (set) Token: 0x060000E5 RID: 229 RVA: 0x00002E2A File Offset: 0x0000102A
		public float SqDistance { get; set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x00002E33 File Offset: 0x00001033
		// (set) Token: 0x060000E7 RID: 231 RVA: 0x00002E3B File Offset: 0x0000103B
		public float Distance2Camera { get; set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000E8 RID: 232 RVA: 0x00002E44 File Offset: 0x00001044
		// (set) Token: 0x060000E9 RID: 233 RVA: 0x00002E4C File Offset: 0x0000104C
		public Bounds Bounds { get; set; }

		// Token: 0x060000EA RID: 234 RVA: 0x00002E55 File Offset: 0x00001055
		public void Reset()
		{
			this.PrimitiveType = EFogVolumePrimitiveType.None;
			this.Primitive = null;
			this.Transform = null;
			this.Renderer = null;
			this.SqDistance = 0f;
			this.Distance2Camera = 0f;
		}
	}
}
