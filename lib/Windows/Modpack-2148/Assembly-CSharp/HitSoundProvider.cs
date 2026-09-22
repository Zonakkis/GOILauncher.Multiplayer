using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200004D RID: 77
public class HitSoundProvider : MonoBehaviour
{
	// Token: 0x060001BE RID: 446 RVA: 0x000206AC File Offset: 0x0001E8AC
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

	// Token: 0x060001BF RID: 447 RVA: 0x00020810 File Offset: 0x0001EA10
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

	// Token: 0x060001C0 RID: 448 RVA: 0x00020974 File Offset: 0x0001EB74
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

	// Token: 0x040002D6 RID: 726
	public List<AudioClip> rockhits;

	// Token: 0x040002D7 RID: 727
	public List<AudioClip> woodhits;

	// Token: 0x040002D8 RID: 728
	public List<AudioClip> metalhits;

	// Token: 0x040002D9 RID: 729
	public List<AudioClip> plastichits;

	// Token: 0x040002DA RID: 730
	public List<AudioClip> furniturehits;

	// Token: 0x040002DB RID: 731
	public List<AudioClip> snowhits;

	// Token: 0x040002DC RID: 732
	public List<AudioClip> cardboardhits;

	// Token: 0x040002DD RID: 733
	public List<AudioClip> snakehits;

	// Token: 0x040002DE RID: 734
	public List<AudioClip> solidmetalhits;

	// Token: 0x040002DF RID: 735
	public List<AudioClip> rockhardhits;

	// Token: 0x040002E0 RID: 736
	public List<AudioClip> woodhardhits;

	// Token: 0x040002E1 RID: 737
	public List<AudioClip> metalhardhits;

	// Token: 0x040002E2 RID: 738
	public List<AudioClip> plastichardhits;

	// Token: 0x040002E3 RID: 739
	public List<AudioClip> furniturehardhits;

	// Token: 0x040002E4 RID: 740
	public List<AudioClip> snowhardhits;

	// Token: 0x040002E5 RID: 741
	public List<AudioClip> cardboardhardhits;

	// Token: 0x040002E6 RID: 742
	public List<AudioClip> snakehardhits;

	// Token: 0x040002E7 RID: 743
	public List<AudioClip> solidmetalhardhits;

	// Token: 0x040002E8 RID: 744
	public List<AudioClip> rockscrapes;

	// Token: 0x040002E9 RID: 745
	public List<AudioClip> woodscrapes;

	// Token: 0x040002EA RID: 746
	public List<AudioClip> metalscrapes;

	// Token: 0x040002EB RID: 747
	public List<AudioClip> plasticscrapes;

	// Token: 0x040002EC RID: 748
	public List<AudioClip> furniturescrapes;

	// Token: 0x040002ED RID: 749
	public List<AudioClip> snowscrapes;

	// Token: 0x040002EE RID: 750
	public List<AudioClip> cardboardscrapes;

	// Token: 0x040002EF RID: 751
	public List<AudioClip> snakescrapes;

	// Token: 0x040002F0 RID: 752
	public List<AudioClip> solidmetalscrapes;
}
