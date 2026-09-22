using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x0200007D RID: 125
public class MB3_TestTexturePacker : MonoBehaviour
{
	// Token: 0x06000382 RID: 898 RVA: 0x0001C084 File Offset: 0x0001A484
	[ContextMenu("Generate List Of Images To Add")]
	public void GenerateListOfImagesToAdd()
	{
		this.imgsToAdd = new List<Vector2>();
		for (int i = 0; i < this.numTex; i++)
		{
			Vector2 vector = new Vector2((float)Mathf.RoundToInt((float)global::UnityEngine.Random.Range(this.min, this.max) * this.xMult), (float)Mathf.RoundToInt((float)global::UnityEngine.Random.Range(this.min, this.max) * this.yMult));
			if (this.imgsMustBePowerOfTwo)
			{
				vector.x = (float)MB2_TexturePacker.RoundToNearestPositivePowerOfTwo((int)vector.x);
				vector.y = (float)MB2_TexturePacker.RoundToNearestPositivePowerOfTwo((int)vector.y);
			}
			this.imgsToAdd.Add(vector);
		}
	}

	// Token: 0x06000383 RID: 899 RVA: 0x0001C13C File Offset: 0x0001A53C
	[ContextMenu("Run")]
	public void RunTestHarness()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = this.doPowerOfTwoTextures;
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(this.imgsToAdd, this.maxDim, this.padding, this.doMultiAtlas);
		if (this.rs != null)
		{
			Debug.Log("NumAtlas= " + this.rs.Length);
			for (int i = 0; i < this.rs.Length; i++)
			{
				for (int j = 0; j < this.rs[i].rects.Length; j++)
				{
					Rect rect = this.rs[i].rects[j];
					rect.x *= (float)this.rs[i].atlasX;
					rect.y *= (float)this.rs[i].atlasY;
					rect.width *= (float)this.rs[i].atlasX;
					rect.height *= (float)this.rs[i].atlasY;
					Debug.Log(rect.ToString("f5"));
				}
				Debug.Log("===============");
			}
			this.res = string.Concat(new object[]
			{
				"mxX= ",
				this.rs[0].atlasX,
				" mxY= ",
				this.rs[0].atlasY
			});
		}
		else
		{
			this.res = "ERROR: PACKING FAILED";
		}
	}

	// Token: 0x06000384 RID: 900 RVA: 0x0001C300 File Offset: 0x0001A700
	private void OnDrawGizmos()
	{
		if (this.rs != null)
		{
			for (int i = 0; i < this.rs.Length; i++)
			{
				Vector2 vector = new Vector2((float)i * 1.5f * (float)this.maxDim, 0f);
				AtlasPackingResult atlasPackingResult = this.rs[i];
				Vector2 vector2 = new Vector2(vector.x + (float)(atlasPackingResult.atlasX / 2), vector.y + (float)(atlasPackingResult.atlasY / 2));
				Vector2 vector3 = new Vector2((float)atlasPackingResult.atlasX, (float)atlasPackingResult.atlasY);
				Gizmos.DrawWireCube(vector2, vector3);
				for (int j = 0; j < this.rs[i].rects.Length; j++)
				{
					Rect rect = this.rs[i].rects[j];
					Gizmos.color = new Color(global::UnityEngine.Random.value, global::UnityEngine.Random.value, global::UnityEngine.Random.value);
					vector2 = new Vector2(vector.x + (rect.x + rect.width / 2f) * (float)this.rs[i].atlasX, vector.y + (rect.y + rect.height / 2f) * (float)this.rs[i].atlasY);
					Vector2 vector4 = new Vector2(rect.width * (float)this.rs[i].atlasX, rect.height * (float)this.rs[i].atlasY);
					Gizmos.DrawCube(vector2, vector4);
				}
			}
		}
	}

	// Token: 0x06000385 RID: 901 RVA: 0x0001C4A4 File Offset: 0x0001A8A4
	[ContextMenu("Test1")]
	private void Test1()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = true;
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(450f, 200f));
		list.Add(new Vector2(450f, 200f));
		list.Add(new Vector2(450f, 80f));
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(list, 512, 8, true);
		Debug.Log("Success! ");
	}

	// Token: 0x06000386 RID: 902 RVA: 0x0001C544 File Offset: 0x0001A944
	[ContextMenu("Test2")]
	private void Test2()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = true;
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(200f, 450f));
		list.Add(new Vector2(200f, 450f));
		list.Add(new Vector2(80f, 450f));
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(list, 512, 8, true);
		Debug.Log("Success! ");
	}

	// Token: 0x06000387 RID: 903 RVA: 0x0001C5E4 File Offset: 0x0001A9E4
	[ContextMenu("Test3")]
	private void Test3()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = false;
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(450f, 200f));
		list.Add(new Vector2(450f, 200f));
		list.Add(new Vector2(450f, 80f));
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(list, 512, 8, true);
		Debug.Log("Success! ");
	}

	// Token: 0x06000388 RID: 904 RVA: 0x0001C684 File Offset: 0x0001AA84
	[ContextMenu("Test4")]
	private void Test4()
	{
		this.texturePacker = new MB2_TexturePacker();
		this.texturePacker.doPowerOfTwoTextures = false;
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(200f, 450f));
		list.Add(new Vector2(200f, 450f));
		list.Add(new Vector2(80f, 450f));
		this.texturePacker.LOG_LEVEL = this.logLevel;
		this.rs = this.texturePacker.GetRects(list, 512, 8, true);
		Debug.Log("Success! ");
	}

	// Token: 0x04000361 RID: 865
	private MB2_TexturePacker texturePacker;

	// Token: 0x04000362 RID: 866
	public int numTex = 32;

	// Token: 0x04000363 RID: 867
	public int min = 126;

	// Token: 0x04000364 RID: 868
	public int max = 2046;

	// Token: 0x04000365 RID: 869
	public float xMult = 1f;

	// Token: 0x04000366 RID: 870
	public float yMult = 1f;

	// Token: 0x04000367 RID: 871
	public bool imgsMustBePowerOfTwo;

	// Token: 0x04000368 RID: 872
	public List<Vector2> imgsToAdd = new List<Vector2>();

	// Token: 0x04000369 RID: 873
	public int padding = 1;

	// Token: 0x0400036A RID: 874
	public int maxDim = 4096;

	// Token: 0x0400036B RID: 875
	public bool doPowerOfTwoTextures = true;

	// Token: 0x0400036C RID: 876
	public bool doMultiAtlas;

	// Token: 0x0400036D RID: 877
	public MB2_LogLevel logLevel;

	// Token: 0x0400036E RID: 878
	public string res;

	// Token: 0x0400036F RID: 879
	public AtlasPackingResult[] rs;
}
