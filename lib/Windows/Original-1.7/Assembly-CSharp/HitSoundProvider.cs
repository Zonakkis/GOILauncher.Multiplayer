using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000036 RID: 54
public class HitSoundProvider : MonoBehaviour
{
	// Token: 0x0600018E RID: 398 RVA: 0x0000F3DC File Offset: 0x0000D5DC
	public AudioClip GetHit(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return this.rockhits[Random.Range(0, this.rockhits.Count)];
		case GroundCol.SoundMaterial.wood:
			return this.woodhits[Random.Range(0, this.woodhits.Count)];
		case GroundCol.SoundMaterial.metal:
			return this.metalhits[Random.Range(0, this.metalhits.Count)];
		case GroundCol.SoundMaterial.plastic:
			return this.plastichits[Random.Range(0, this.plastichits.Count)];
		case GroundCol.SoundMaterial.furniture:
			return this.furniturehits[Random.Range(0, this.furniturehits.Count)];
		case GroundCol.SoundMaterial.snow:
			return this.snowhits[Random.Range(0, this.snowhits.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return this.cardboardhits[Random.Range(0, this.cardboardhits.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		case GroundCol.SoundMaterial.snake:
			return this.snakehits[Random.Range(0, this.snakehits.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return this.solidmetalhits[Random.Range(0, this.solidmetalhits.Count)];
		default:
			return this.rockhits[Random.Range(0, this.rockhits.Count)];
		}
	}

	// Token: 0x0600018F RID: 399 RVA: 0x0000F540 File Offset: 0x0000D740
	public AudioClip GetHardHit(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return this.rockhardhits[Random.Range(0, this.rockhardhits.Count)];
		case GroundCol.SoundMaterial.wood:
			return this.woodhardhits[Random.Range(0, this.woodhardhits.Count)];
		case GroundCol.SoundMaterial.metal:
			return this.metalhardhits[Random.Range(0, this.metalhardhits.Count)];
		case GroundCol.SoundMaterial.plastic:
			return this.plastichardhits[Random.Range(0, this.plastichardhits.Count)];
		case GroundCol.SoundMaterial.furniture:
			return this.furniturehardhits[Random.Range(0, this.furniturehardhits.Count)];
		case GroundCol.SoundMaterial.snow:
			return this.snowhardhits[Random.Range(0, this.snowhardhits.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return this.cardboardhardhits[Random.Range(0, this.cardboardhardhits.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		case GroundCol.SoundMaterial.snake:
			return this.snakehardhits[Random.Range(0, this.snakehardhits.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return this.solidmetalhardhits[Random.Range(0, this.solidmetalhardhits.Count)];
		default:
			return this.rockhardhits[Random.Range(0, this.rockhardhits.Count)];
		}
	}

	// Token: 0x06000190 RID: 400 RVA: 0x0000F6A4 File Offset: 0x0000D8A4
	public AudioClip GetScrape(GroundCol.SoundMaterial mat)
	{
		switch (mat)
		{
		case GroundCol.SoundMaterial.rock:
			return this.rockscrapes[Random.Range(0, this.rockscrapes.Count)];
		case GroundCol.SoundMaterial.wood:
			return this.woodscrapes[Random.Range(0, this.woodscrapes.Count)];
		case GroundCol.SoundMaterial.metal:
			return this.metalscrapes[Random.Range(0, this.metalscrapes.Count)];
		case GroundCol.SoundMaterial.plastic:
			return this.plasticscrapes[Random.Range(0, this.plasticscrapes.Count)];
		case GroundCol.SoundMaterial.furniture:
			return this.furniturescrapes[Random.Range(0, this.furniturescrapes.Count)];
		case GroundCol.SoundMaterial.snow:
			return this.snowscrapes[Random.Range(0, this.snowscrapes.Count)];
		case GroundCol.SoundMaterial.cardboard:
			return this.cardboardscrapes[Random.Range(0, this.cardboardscrapes.Count)];
		case GroundCol.SoundMaterial.none:
			return null;
		case GroundCol.SoundMaterial.snake:
			return this.snakescrapes[Random.Range(0, this.snakescrapes.Count)];
		case GroundCol.SoundMaterial.solidmetal:
			return this.solidmetalscrapes[Random.Range(0, this.solidmetalscrapes.Count)];
		default:
			return this.rockscrapes[Random.Range(0, this.rockscrapes.Count)];
		}
	}

	// Token: 0x04000270 RID: 624
	public List<AudioClip> rockhits;

	// Token: 0x04000271 RID: 625
	public List<AudioClip> woodhits;

	// Token: 0x04000272 RID: 626
	public List<AudioClip> metalhits;

	// Token: 0x04000273 RID: 627
	public List<AudioClip> plastichits;

	// Token: 0x04000274 RID: 628
	public List<AudioClip> furniturehits;

	// Token: 0x04000275 RID: 629
	public List<AudioClip> snowhits;

	// Token: 0x04000276 RID: 630
	public List<AudioClip> cardboardhits;

	// Token: 0x04000277 RID: 631
	public List<AudioClip> snakehits;

	// Token: 0x04000278 RID: 632
	public List<AudioClip> solidmetalhits;

	// Token: 0x04000279 RID: 633
	public List<AudioClip> rockhardhits;

	// Token: 0x0400027A RID: 634
	public List<AudioClip> woodhardhits;

	// Token: 0x0400027B RID: 635
	public List<AudioClip> metalhardhits;

	// Token: 0x0400027C RID: 636
	public List<AudioClip> plastichardhits;

	// Token: 0x0400027D RID: 637
	public List<AudioClip> furniturehardhits;

	// Token: 0x0400027E RID: 638
	public List<AudioClip> snowhardhits;

	// Token: 0x0400027F RID: 639
	public List<AudioClip> cardboardhardhits;

	// Token: 0x04000280 RID: 640
	public List<AudioClip> snakehardhits;

	// Token: 0x04000281 RID: 641
	public List<AudioClip> solidmetalhardhits;

	// Token: 0x04000282 RID: 642
	public List<AudioClip> rockscrapes;

	// Token: 0x04000283 RID: 643
	public List<AudioClip> woodscrapes;

	// Token: 0x04000284 RID: 644
	public List<AudioClip> metalscrapes;

	// Token: 0x04000285 RID: 645
	public List<AudioClip> plasticscrapes;

	// Token: 0x04000286 RID: 646
	public List<AudioClip> furniturescrapes;

	// Token: 0x04000287 RID: 647
	public List<AudioClip> snowscrapes;

	// Token: 0x04000288 RID: 648
	public List<AudioClip> cardboardscrapes;

	// Token: 0x04000289 RID: 649
	public List<AudioClip> snakescrapes;

	// Token: 0x0400028A RID: 650
	public List<AudioClip> solidmetalscrapes;
}
