using System;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000018 RID: 24
[ExecuteInEditMode]
public class FogVolumePrimitive : MonoBehaviour
{
	// Token: 0x06000099 RID: 153 RVA: 0x00008EF3 File Offset: 0x000072F3
	public FogVolumePrimitive()
	{
		this.SphereColl = null;
		this.BoxColl = null;
	}

	// Token: 0x17000019 RID: 25
	// (get) Token: 0x0600009A RID: 154 RVA: 0x00008F1B File Offset: 0x0000731B
	public Transform GetTransform
	{
		get
		{
			return base.gameObject.transform;
		}
	}

	// Token: 0x1700001A RID: 26
	// (get) Token: 0x0600009B RID: 155 RVA: 0x00008F28 File Offset: 0x00007328
	public Vector3 GetPrimitiveScale
	{
		get
		{
			return new Vector3(Mathf.Max(this.MinScale, base.transform.lossyScale.x), Mathf.Max(this.MinScale, base.transform.lossyScale.y), Mathf.Max(this.MinScale, base.transform.lossyScale.z));
		}
	}

	// Token: 0x1700001B RID: 27
	// (get) Token: 0x0600009C RID: 156 RVA: 0x00008F94 File Offset: 0x00007394
	public Bounds Bounds
	{
		get
		{
			if (this.BoxColl != null)
			{
				return this.BoxColl.bounds;
			}
			if (this.SphereColl != null)
			{
				return this.SphereColl.bounds;
			}
			return new Bounds(base.gameObject.transform.position, base.gameObject.transform.lossyScale);
		}
	}

	// Token: 0x0600009D RID: 157 RVA: 0x00009000 File Offset: 0x00007400
	public void AddColliderIfNeccessary(EFogVolumePrimitiveType _type)
	{
		this.Type = _type;
		EFogVolumePrimitiveType type = this.Type;
		if (type != EFogVolumePrimitiveType.None)
		{
			if (type != EFogVolumePrimitiveType.Box)
			{
				if (type == EFogVolumePrimitiveType.Sphere)
				{
					if (this.SphereColl == null)
					{
						this.SphereColl = base.gameObject.AddComponent<SphereCollider>();
					}
				}
			}
			else if (this.BoxColl == null)
			{
				this.BoxColl = base.gameObject.AddComponent<BoxCollider>();
			}
		}
	}

	// Token: 0x0600009E RID: 158 RVA: 0x00009088 File Offset: 0x00007488
	private void OnEnable()
	{
		this.Primitive = base.gameObject;
		this._Renderer = this.Primitive.GetComponent<MeshRenderer>();
		if (!this.PrimitiveMaterial)
		{
			this.PrimitiveMaterial = (Material)Resources.Load("PrimitiveMaterial");
		}
		this._Renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		this._Renderer.lightProbeUsage = LightProbeUsage.Off;
		this._Renderer.shadowCastingMode = ShadowCastingMode.Off;
		this._Renderer.receiveShadows = false;
		base.GetComponent<MeshRenderer>().material = this.PrimitiveMaterial;
		this.BoxColl = base.GetComponent<BoxCollider>();
		this.SphereColl = base.GetComponent<SphereCollider>();
		if (this.BoxColl == null && this.SphereColl == null)
		{
			this.BoxColl = base.gameObject.AddComponent<BoxCollider>();
			this.Type = EFogVolumePrimitiveType.Box;
		}
		else if (this.BoxColl != null)
		{
			this.Type = EFogVolumePrimitiveType.Box;
		}
		else if (this.SphereColl != null)
		{
			this.Type = EFogVolumePrimitiveType.Sphere;
		}
		else
		{
			this.Type = EFogVolumePrimitiveType.None;
		}
	}

	// Token: 0x04000152 RID: 338
	public BoxCollider BoxColl;

	// Token: 0x04000153 RID: 339
	public SphereCollider SphereColl;

	// Token: 0x04000154 RID: 340
	public bool IsPersistent = true;

	// Token: 0x04000155 RID: 341
	public EFogVolumePrimitiveType Type;

	// Token: 0x04000156 RID: 342
	public bool IsSubtractive;

	// Token: 0x04000157 RID: 343
	public Material PrimitiveMaterial;

	// Token: 0x04000158 RID: 344
	private GameObject Primitive;

	// Token: 0x04000159 RID: 345
	private Renderer _Renderer;

	// Token: 0x0400015A RID: 346
	private readonly float MinScale = 0.0001f;
}
