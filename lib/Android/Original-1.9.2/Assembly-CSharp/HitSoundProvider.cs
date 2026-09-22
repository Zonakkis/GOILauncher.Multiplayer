using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200012C RID: 300
public class HitSoundProvider : MonoBehaviour
{
	// Token: 0x0600079F RID: 1951 RVA: 0x00040A74 File Offset: 0x0003EE74
	public AudioClip GetHit(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return this.rockhits[global::UnityEngine.Random.Range(0, this.rockhits.Count)];
		case GroundCol.SoundMaterial.wood:
			return this.woodhits[global::UnityEngine.Random.Range(0, this.woodhits.Count)];
		case GroundCol.SoundMaterial.metal:
			return this.metalhits[global::UnityEngine.Random.Range(0, this.metalhits.Count)];
		case GroundCol.SoundMaterial.plastic:
			return this.plastichits[global::UnityEngine.Random.Range(0, this.plastichits.Count)];
		case GroundCol.SoundMaterial.furniture:
			return this.furniturehits[global::UnityEngine.Random.Range(0, this.furniturehits.Count)];
		case GroundCol.SoundMaterial.snow:
			return this.snowhits[global::UnityEngine.Random.Range(0, this.snowhits.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return this.cardboardhits[global::UnityEngine.Random.Range(0, this.cardboardhits.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		case GroundCol.SoundMaterial.snake:
			return this.snakehits[global::UnityEngine.Random.Range(0, this.snakehits.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return this.solidmetalhits[global::UnityEngine.Random.Range(0, this.solidmetalhits.Count)];
		default:
			return this.rockhits[global::UnityEngine.Random.Range(0, this.rockhits.Count)];
		}
	}

	// Token: 0x060007A0 RID: 1952 RVA: 0x00040BD8 File Offset: 0x0003EFD8
	public AudioClip GetHardHit(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return this.rockhardhits[global::UnityEngine.Random.Range(0, this.rockhardhits.Count)];
		case GroundCol.SoundMaterial.wood:
			return this.woodhardhits[global::UnityEngine.Random.Range(0, this.woodhardhits.Count)];
		case GroundCol.SoundMaterial.metal:
			return this.metalhardhits[global::UnityEngine.Random.Range(0, this.metalhardhits.Count)];
		case GroundCol.SoundMaterial.plastic:
			return this.plastichardhits[global::UnityEngine.Random.Range(0, this.plastichardhits.Count)];
		case GroundCol.SoundMaterial.furniture:
			return this.furniturehardhits[global::UnityEngine.Random.Range(0, this.furniturehardhits.Count)];
		case GroundCol.SoundMaterial.snow:
			return this.snowhardhits[global::UnityEngine.Random.Range(0, this.snowhardhits.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return this.cardboardhardhits[global::UnityEngine.Random.Range(0, this.cardboardhardhits.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		case GroundCol.SoundMaterial.snake:
			return this.snakehardhits[global::UnityEngine.Random.Range(0, this.snakehardhits.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return this.solidmetalhardhits[global::UnityEngine.Random.Range(0, this.solidmetalhardhits.Count)];
		default:
			return this.rockhardhits[global::UnityEngine.Random.Range(0, this.rockhardhits.Count)];
		}
	}

	// Token: 0x060007A1 RID: 1953 RVA: 0x00040D3C File Offset: 0x0003F13C
	public AudioClip GetScrape(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return this.rockscrapes[global::UnityEngine.Random.Range(0, this.rockscrapes.Count)];
		case GroundCol.SoundMaterial.wood:
			return this.woodscrapes[global::UnityEngine.Random.Range(0, this.woodscrapes.Count)];
		case GroundCol.SoundMaterial.metal:
			return this.metalscrapes[global::UnityEngine.Random.Range(0, this.metalscrapes.Count)];
		case GroundCol.SoundMaterial.plastic:
			return this.plasticscrapes[global::UnityEngine.Random.Range(0, this.plasticscrapes.Count)];
		case GroundCol.SoundMaterial.furniture:
			return this.furniturescrapes[global::UnityEngine.Random.Range(0, this.furniturescrapes.Count)];
		case GroundCol.SoundMaterial.snow:
			return this.snowscrapes[global::UnityEngine.Random.Range(0, this.snowscrapes.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return this.cardboardscrapes[global::UnityEngine.Random.Range(0, this.cardboardscrapes.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		case GroundCol.SoundMaterial.snake:
			return this.snakescrapes[global::UnityEngine.Random.Range(0, this.snakescrapes.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return this.solidmetalscrapes[global::UnityEngine.Random.Range(0, this.solidmetalscrapes.Count)];
		default:
			return this.rockscrapes[global::UnityEngine.Random.Range(0, this.rockscrapes.Count)];
		}
	}

	// Token: 0x040006EC RID: 1772
	public List<AudioClip> rockhits;

	// Token: 0x040006ED RID: 1773
	public List<AudioClip> woodhits;

	// Token: 0x040006EE RID: 1774
	public List<AudioClip> metalhits;

	// Token: 0x040006EF RID: 1775
	public List<AudioClip> plastichits;

	// Token: 0x040006F0 RID: 1776
	public List<AudioClip> furniturehits;

	// Token: 0x040006F1 RID: 1777
	public List<AudioClip> snowhits;

	// Token: 0x040006F2 RID: 1778
	public List<AudioClip> cardboardhits;

	// Token: 0x040006F3 RID: 1779
	public List<AudioClip> snakehits;

	// Token: 0x040006F4 RID: 1780
	public List<AudioClip> solidmetalhits;

	// Token: 0x040006F5 RID: 1781
	public List<AudioClip> rockhardhits;

	// Token: 0x040006F6 RID: 1782
	public List<AudioClip> woodhardhits;

	// Token: 0x040006F7 RID: 1783
	public List<AudioClip> metalhardhits;

	// Token: 0x040006F8 RID: 1784
	public List<AudioClip> plastichardhits;

	// Token: 0x040006F9 RID: 1785
	public List<AudioClip> furniturehardhits;

	// Token: 0x040006FA RID: 1786
	public List<AudioClip> snowhardhits;

	// Token: 0x040006FB RID: 1787
	public List<AudioClip> cardboardhardhits;

	// Token: 0x040006FC RID: 1788
	public List<AudioClip> snakehardhits;

	// Token: 0x040006FD RID: 1789
	public List<AudioClip> solidmetalhardhits;

	// Token: 0x040006FE RID: 1790
	public List<AudioClip> rockscrapes;

	// Token: 0x040006FF RID: 1791
	public List<AudioClip> woodscrapes;

	// Token: 0x04000700 RID: 1792
	public List<AudioClip> metalscrapes;

	// Token: 0x04000701 RID: 1793
	public List<AudioClip> plasticscrapes;

	// Token: 0x04000702 RID: 1794
	public List<AudioClip> furniturescrapes;

	// Token: 0x04000703 RID: 1795
	public List<AudioClip> snowscrapes;

	// Token: 0x04000704 RID: 1796
	public List<AudioClip> cardboardscrapes;

	// Token: 0x04000705 RID: 1797
	public List<AudioClip> snakescrapes;

	// Token: 0x04000706 RID: 1798
	public List<AudioClip> solidmetalscrapes;
}
