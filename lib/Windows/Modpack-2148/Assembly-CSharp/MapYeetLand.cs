using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x0200031B RID: 795
public class MapYeetLand : Map
{
	// Token: 0x06001B79 RID: 7033 RVA: 0x00084E58 File Offset: 0x00083058
	public bool Load()
	{
		try
		{
			GameObject gameObject = GameObject.Find("Deadtree");
			GameObject gameObject2 = GameObject.Find("Rope4");
			GameObject gameObject3 = GameObject.Find("tower+crane");
			GameObject gameObject4 = GameObject.Find("Rock");
			GameObject gameObject5 = GameObject.Find("Rock (7)");
			GameObject gameObject6 = GameObject.Find("Rock (1)");
			GameObject gameObject7 = GameObject.Find("Rock (1)AAA");
			GameObject gameObject8 = GameObject.Find("Rock (8)");
			GameObject gameObject9 = GameObject.Find("Rock (4)");
			GameObject gameObject10 = GameObject.Find("Rock (6)");
			GameObject gameObject11 = GameObject.Find("Coffee+Cup+Takeaway");
			GameObject.Find("Fire Bin");
			GameObject gameObject12 = GameObject.Find("Props").transform.Find("Orange").gameObject;
			GameObject.Find("Deadtree").transform.localScale = new Vector3(-1f, 1f, 1f);
			GameObject.Find("Snake").SetActive(false);
			GameObject.Find("OuterWall").SetActive(false);
			GameObject.Find("OuterWall (3)").SetActive(false);
			GameObject.Find("Orangetable").SetActive(false);
			GameObject.Find("Toilet").SetActive(false);
			GameObject.Find("French Chair").SetActive(false);
			GameObject.Find("Donut Lamp").SetActive(false);
			GameObject.Find("Desk").SetActive(false);
			GameObject.Find("Mantel").SetActive(false);
			GameObject.Find("Table").SetActive(false);
			GameObject.Find("old+wooden+table").SetActive(false);
			GameObject.Find("WhiteCouch").SetActive(false);
			GameObject.Find("White Couch").SetActive(false);
			GameObject.Find("Picnic Table").SetActive(false);
			GameObject.Find("WoodPillar").SetActive(false);
			GameObject.Find("ballok").SetActive(false);
			GameObject.Find("WoodenJetty").SetActive(false);
			GameObject.Find("SnowyRock").SetActive(false);
			GameObject.Find("Rocks").SetActive(false);
			GameObject.Find("Pillar").SetActive(false);
			GameObject.Find("Trashcan").SetActive(false);
			GameObject.Find("Armchair").SetActive(false);
			GameObject.Find("Shelving").SetActive(false);
			GameObject.Find("RoughTable").SetActive(false);
			GameObject.Find("SnowHat").transform.localPosition = new Vector3(64.2f, 242.3f, -0.2f);
			MapTools.createArrow(new Vector3(30f, 55.3f, 0f), 0f, false);
			MapTools.createArrow(new Vector3(-46f, 2.7f, 0f), 180f, true);
			MapTools.createArrow(new Vector3(1.6f, 102.5f, 0f), 135f, false);
			MapTools.createArrow(new Vector3(-6f, 164.5f, 0f), 0f, false);
			MapTools.createArrow(new Vector3(27f, 145.5f, 0f), 0f, false);
			MapTools.createArrow(new Vector3(36f, 170f, 0f), 90f, false);
			MapTools.createArrow(new Vector3(65f, 165f, 0f), 180f, false);
			MapTools.createArrow(new Vector3(75f, 226f, 0f), 90f, false);
			MapTools.createArrow(new Vector3(70f, 222f, 0f), 180f, false);
			MapTools.createArrow(new Vector3(45f, 220f, 0f), 90f, false);
			MapTools.createArrow(new Vector3(57f, 248f, 0f), 90f, false);
			MapTools.createArrow(new Vector3(16f, 282f, 0f), 0f, false);
			MapTools.createArrow(new Vector3(8f, 282f, 0f), 180f, false);
			MapTools.createArrow(new Vector3(54f, 326f, 0f), 90f, false);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-93f, 19.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(-100f, 52f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-66.5f, -8f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-108.2f, 24.1f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(-70f, 0f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject7, new Vector3(-90f, -10f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(-90f, 15f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(-105f, 30f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-107.5f, 44f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-96.5f, 43f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-85.5f, 42f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-89f, 3.6f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-86.8f, 0f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject7, new Vector3(-63f, 20f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject3, new Vector3(-58f, 18f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject7, new Vector3(-42f, 22f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject3, new Vector3(-36.5f, 20f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(-15f, 45f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(10f, 60f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-17f, 48f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-10f, 52f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject7, new Vector3(11f, 45.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject9, new Vector3(28.5f, 81.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject10, new Vector3(42f, 91f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(20f, 90f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(33f, 105f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(35f, 95f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(15f, 108f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject9, new Vector3(30f, 100f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject9, new Vector3(20f, 98f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject9, new Vector3(10f, 97f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject3, new Vector3(2.6f, 72.6f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject3, new Vector3(-1f, 70.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject10, new Vector3(4f, 116f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-18f, 111.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-19.5f, 121f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-6.5f, 118.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-6.5f, 123f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-13f, 129f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject10, new Vector3(1f, 143f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-14.5f, 137f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject10, new Vector3(-22f, 155f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-28.35f, 137.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-28.45f, 135f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-5.3f, 149f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-29f, 152f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-28.7f, 146.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject5, new Vector3(-11f, 147f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-8f, 104f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(-7.5f, 179f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(-16f, 175f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject10, new Vector3(-25f, 165f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject11, new Vector3(26.3f, 149.29f, 0f), Quaternion.Euler(0f, 0f, -90f)).GetComponent<Rigidbody2D>().isKinematic = true;
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(30.8f, 155f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(55f, 14.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject7, new Vector3(65.3f, 121.87f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(52.45f, 160.9f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(58f, 150f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(59f, 158f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(45.8f, 35.7f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject7, new Vector3(36f, 142f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(47f, 182.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(35f, 164f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(25.5f, 182f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(33f, 177f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject9, new Vector3(52.5f, 183.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject5, new Vector3(27.7f, 177f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(55.5f, 183f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(58f, 180f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(77.5f, 186f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(71f, 187f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(82.5f, 189f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(94f, 193f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(98f, 200f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(77.5f, 207f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(93f, 148f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject7, new Vector3(64f, 185f, 3f), Quaternion.identity).transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(70f, 165.25f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(89f, 230.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(91f, 240f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(93f, 252f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(73f, 195f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(60f, 276.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(65f, 344f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(59.5f, 287.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(62.5f, 357f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(51.8f, 188.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(50f, 237.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject7, new Vector3(-14.8f, 266.5f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(0f, 290f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-4.5f, 238f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(8.5f, 230.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(12f, 207f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.8f, 0.8f, 0.8f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-7.2f, 247.2f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-7.8f, 256.8f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-8.1f, 265f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-9f, 274.7f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-9.6f, 276.4f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-15.5f, 242.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(-18f, 244f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			GameObject gameObject13 = global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(-2f, 370f, 0f), Quaternion.identity);
			gameObject13.transform.localScale = new Vector3(2f, 2f, 2f);
			Rigidbody2D[] componentsInChildren = gameObject13.GetComponentsInChildren<Rigidbody2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].isKinematic = true;
			}
			GameObject gameObject14 = global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(8f, 372f, 0f), Quaternion.identity);
			gameObject14.transform.localScale = new Vector3(2f, 2f, 2f);
			Rigidbody2D[] componentsInChildren2 = gameObject14.GetComponentsInChildren<Rigidbody2D>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].isKinematic = true;
			}
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-6.5f, 311f, 0f), Quaternion.identity).transform.localScale = new Vector3(2.5f, 0.05f, 2f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(4f, 311.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.6f, 4f, 0.6f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(27f, 316f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.6f, 4f, 0.6f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-9.25f, 315f, 0f), Quaternion.identity).transform.localScale = new Vector3(3.2f, 0.05f, 2f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(10.5f, 319.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(16.5f, 319.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(22.5f, 319.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(13.5f, 327f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(19.5f, 327f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(10.5f, 334.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(16.5f, 334.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(22.5f, 334.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(13.5f, 342f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(19.5f, 342f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(16.5f, 350f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(35f, 303f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(37f, 306f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(39f, 309f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject10, new Vector3(58.5f, 347f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.7f, 1.8f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject10, new Vector3(45f, 347f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.7f, 1.8f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(60f, 319f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 0.6f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(46.5f, 298f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(57.5f, 302f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(57.5f, 302f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(0f, 49f, 0f), Quaternion.identity).transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(38f, 50f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(48f, 50f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(50.5f, 91f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(50.5f, 82f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(50.5f, 74.1f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(50.5f, 66f, 0f), Quaternion.identity).transform.localScale = new Vector3(-1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject12, new Vector3(48.2f, 96.45f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(48.5f, 96.3f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-20f, 254f, 0f), Quaternion.identity).transform.localScale = new Vector3(2f, 0.2f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-8.15f, 253.4f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(0f, 255.14f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(0.2f, 255.14f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject12, new Vector3(0.15f, 255.33f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-2.7f, 256f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.3f, 0.01f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-33f, 248f, 0f), Quaternion.identity).transform.localScale = new Vector3(2f, 0.2f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-11f, 251.9f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.15f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-7.92f, 250.9f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.15f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-7.5f, 252.65f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject12, new Vector3(-7.3f, 252.78f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-11f, 249.7f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.5f, 0.15f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-3.75f, 249f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.22f, 0.22f, 0.22f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-46f, 244f, 0f), Quaternion.identity).transform.localScale = new Vector3(2f, 0.2f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-31.8f, 245.8f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.3f, 0.01f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-29f, 245.135f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-29.2f, 245.135f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject12, new Vector3(-29.03f, 245.34f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-36.8f, 243.1f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-32f, 243.1f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-34.15f, 242f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.22f, 0.22f, 0.22f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-34.7f, 242f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(109f, 63f, 0f), Quaternion.identity).transform.localScale = new Vector3(-5.5f, 0.8f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject6, new Vector3(17f, 61.8f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(14.8f, 56.8f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(45f, 230f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(46.5f, 233.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 1f, 1f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(41.2f, 172.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(51.2f, 178f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject8, new Vector3(54f, 184f, 0f), Quaternion.identity).transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(46.5f, 216.5f, 0f), Quaternion.identity).transform.localScale = new Vector3(1f, 0.02f, 1f);
			global::UnityEngine.Object.Destroy(GameObject.Find("Cube"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Quad"));
			global::UnityEngine.Object.Destroy(GameObject.Find("tower+crane"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Invisible Walls"));
		}
		catch
		{
		}
		try
		{
			this.updateConfig(this.name);
		}
		catch
		{
		}
		this.bedded = false;
		this.chairded = false;
		this.rock069ded = false;
		this.rock3ded = false;
		this.cubeDeleted = false;
		this.quadDeleted = false;
		this.houseDeleted = false;
		this.lastDelete = 0f;
		return true;
	}

	// Token: 0x06001B7A RID: 7034 RVA: 0x000874E4 File Offset: 0x000856E4
	public MapYeetLand()
	{
		this.name = "YeetLand";
		this.version = "1.3";
		this.author = "Codyumm";
		this.web = "youtube.com/codyumm";
		this.splits = new Dictionary<string, Rect>();
		this.splits.Add("Start", new Rect(-101f, 48.5f, 12.5f, 4.5f));
		this.splits.Add("Balloons", new Rect(-20f, 48f, 4f, 10f));
		this.splits.Add("Highway", new Rect(0f, 100f, 2f, 4f));
		this.splits.Add("Slide", new Rect(-5f, 162f, 5f, 4f));
		this.splits.Add("Furniture", new Rect(27.5f, 140f, 2f, 17f));
		this.splits.Add("Buckets", new Rect(46.5f, 182f, 2f, 10f));
		this.splits.Add("Boulders", new Rect(84.5f, 217.5f, 2f, 10f));
		this.splits.Add("Anvil Jump", new Rect(54f, 249f, 21f, 40f));
		this.splits.Add("Multipath", new Rect(0f, 359f, 200f, 2f));
		this.splits.Add("Space", new Rect(0f, 470f, 200f, 2f));
		this.splitsSideways = new Dictionary<string, Rect>();
		this.splitsSideways.Add("Start", new Rect(-101f, 48.5f, 12.5f, 4.5f));
		this.splitsSideways.Add("Balloons", new Rect(-20f, 48f, 4f, 10f));
		this.splitsSideways.Add("Highway", new Rect(0f, 100f, 2f, 4f));
		this.splitsSideways.Add("Slide", new Rect(-5f, 162f, 5f, 4f));
		this.splitsSideways.Add("Furniture", new Rect(27.5f, 140f, 2f, 17f));
		this.splitsSideways.Add("Buckets", new Rect(46.5f, 182f, 2f, 10f));
		this.splitsSideways.Add("Boulders", new Rect(84.5f, 217.5f, 2f, 10f));
		this.splitsSideways.Add("Anvil Jump", new Rect(54f, 249f, 21f, 40f));
		this.splitsSideways.Add("Multipath", new Rect(0f, 359f, 200f, 2f));
		this.splitsSideways.Add("Space", new Rect(0f, 470f, 200f, 2f));
	}

	// Token: 0x06001B7B RID: 7035 RVA: 0x0000750B File Offset: 0x0000570B
	public bool UnLoad()
	{
		return true;
	}

	// Token: 0x170007AB RID: 1963
	// (get) Token: 0x06001B7C RID: 7036 RVA: 0x00013EDA File Offset: 0x000120DA
	public string name { get; }

	// Token: 0x170007AC RID: 1964
	// (get) Token: 0x06001B7D RID: 7037 RVA: 0x00013EE2 File Offset: 0x000120E2
	public string version { get; }

	// Token: 0x170007AD RID: 1965
	// (get) Token: 0x06001B7E RID: 7038 RVA: 0x00013EEA File Offset: 0x000120EA
	public string author { get; }

	// Token: 0x170007AE RID: 1966
	// (get) Token: 0x06001B7F RID: 7039 RVA: 0x00013EF2 File Offset: 0x000120F2
	public string web { get; }

	// Token: 0x06001B80 RID: 7040 RVA: 0x00087870 File Offset: 0x00085A70
	public string Update(float deltaTime, GameObject player)
	{
		string text = "";
		if (!this.chairded || !this.bedded || !this.rock3ded || !this.rock069ded || !this.cubeDeleted || !this.quadDeleted || !this.armchairDeleted)
		{
			try
			{
				this.lastDelete += deltaTime;
				if (this.lastDelete > 0.1f)
				{
					this.lastDelete = 0f;
					this.chair = GameObject.Find("Chair");
					this.bed = GameObject.Find("Bed");
					this.rock069 = GameObject.Find("Intresto_rock069_252vert_500face_scale_1_1");
					this.rock3 = GameObject.Find("Rock_3 (1)");
					if (this.chair != null)
					{
						global::UnityEngine.Object.Destroy(this.chair);
					}
					else
					{
						this.chairded = true;
					}
					if (this.bed != null)
					{
						global::UnityEngine.Object.Destroy(this.bed);
					}
					else
					{
						this.bedded = true;
					}
					if (this.rock069 != null)
					{
						global::UnityEngine.Object.Destroy(this.rock069);
					}
					else
					{
						this.rock069ded = true;
					}
					if (this.rock3 != null)
					{
						global::UnityEngine.Object.Destroy(this.rock3);
					}
					else
					{
						this.rock3ded = true;
					}
					if (GameObject.Find("Cube") != null)
					{
						global::UnityEngine.Object.Destroy(GameObject.Find("Cube"));
					}
					else
					{
						this.cubeDeleted = true;
					}
					if (GameObject.Find("Quad") != null)
					{
						global::UnityEngine.Object.Destroy(GameObject.Find("Quad"));
					}
					else
					{
						this.quadDeleted = true;
					}
					if (GameObject.Find("Armchair") != null)
					{
						global::UnityEngine.Object.Destroy(GameObject.Find("Armchair"));
					}
					else
					{
						this.armchairDeleted = true;
					}
					if (GameObject.Find("House") != null)
					{
						global::UnityEngine.Object.Destroy(GameObject.Find("House"));
					}
					else
					{
						this.houseDeleted = true;
					}
				}
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
			return text;
		}
		return text;
	}

	// Token: 0x170007AF RID: 1967
	// (get) Token: 0x06001B81 RID: 7041 RVA: 0x00013EFA File Offset: 0x000120FA
	public Dictionary<string, Rect> splits { get; }

	// Token: 0x170007B0 RID: 1968
	// (get) Token: 0x06001B82 RID: 7042 RVA: 0x00013F02 File Offset: 0x00012102
	public Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x170007B1 RID: 1969
	// (get) Token: 0x06001B83 RID: 7043 RVA: 0x00013F0A File Offset: 0x0001210A
	// (set) Token: 0x06001B84 RID: 7044 RVA: 0x00013F12 File Offset: 0x00012112
	public SaveState[] teleportSaves { get; set; }

	// Token: 0x170007B2 RID: 1970
	// (get) Token: 0x06001B85 RID: 7045 RVA: 0x00013F1B File Offset: 0x0001211B
	// (set) Token: 0x06001B86 RID: 7046 RVA: 0x00013F23 File Offset: 0x00012123
	public Dictionary<string, SaveState> teleportSaveStates { get; set; }

	// Token: 0x06001B87 RID: 7047 RVA: 0x00087A84 File Offset: 0x00085C84
	private void updateConfig(string level)
	{
		string text = string.Format("modpack\\maps\\{0}.mpc", level);
		if (File.Exists(text))
		{
			try
			{
				MapTools.MapConfig mapConfig = JsonConvert.DeserializeObject<MapTools.MapConfig>(File.ReadAllText(text));
				try
				{
					if (mapConfig.SaveStates != null && mapConfig.SaveStates.Count > 0)
					{
						this.teleportSaveStates = new Dictionary<string, SaveState>();
						foreach (KeyValuePair<string, MapTools.TeleportSaveState> keyValuePair in mapConfig.SaveStates)
						{
							this.teleportSaveStates.Add(keyValuePair.Key, new SaveState
							{
								hingePos = keyValuePair.Value.hingePos,
								hingeVel = 0f,
								sliderPos = keyValuePair.Value.sliderPos,
								sliderVel = 0f,
								camPos = new Vector3(keyValuePair.Value.camPos[0], keyValuePair.Value.camPos[1], keyValuePair.Value.camPos[2]),
								playerPos = new Vector3(keyValuePair.Value.playerPos[0], keyValuePair.Value.playerPos[1], keyValuePair.Value.playerPos[2]),
								playerRot = Quaternion.Euler(keyValuePair.Value.playerRot[0], keyValuePair.Value.playerRot[1], keyValuePair.Value.playerRot[2]),
								rbLinearVelocities = new Vector2[]
								{
									Vector2.zero,
									Vector2.zero,
									Vector2.zero,
									Vector2.zero,
									Vector2.zero,
									Vector2.zero
								},
								rbAngularVelocities = new float[6],
								rbPositions = new Vector2[]
								{
									new Vector2(keyValuePair.Value.rbPositions[0][0], keyValuePair.Value.rbPositions[0][1]),
									new Vector2(keyValuePair.Value.rbPositions[1][0], keyValuePair.Value.rbPositions[1][1]),
									new Vector2(keyValuePair.Value.rbPositions[2][0], keyValuePair.Value.rbPositions[2][1]),
									new Vector2(keyValuePair.Value.rbPositions[3][0], keyValuePair.Value.rbPositions[3][1]),
									new Vector2(keyValuePair.Value.rbPositions[4][0], keyValuePair.Value.rbPositions[4][1]),
									new Vector2(keyValuePair.Value.rbPositions[5][0], keyValuePair.Value.rbPositions[5][1])
								},
								rbAngles = keyValuePair.Value.rbAngles
							});
						}
					}
				}
				catch (Exception)
				{
				}
			}
			catch
			{
			}
		}
	}

	// Token: 0x06001B88 RID: 7048 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001B89 RID: 7049 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x04001285 RID: 4741
	public bool chairded;

	// Token: 0x04001286 RID: 4742
	public float lastDelete;

	// Token: 0x04001287 RID: 4743
	public GameObject chair;

	// Token: 0x04001288 RID: 4744
	public bool bedded;

	// Token: 0x04001289 RID: 4745
	public GameObject bed;

	// Token: 0x0400128A RID: 4746
	public GameObject bin;

	// Token: 0x0400128B RID: 4747
	public GameObject rock069;

	// Token: 0x0400128C RID: 4748
	public bool rock069ded;

	// Token: 0x0400128D RID: 4749
	public GameObject rock3;

	// Token: 0x0400128E RID: 4750
	public bool rock3ded;

	// Token: 0x0400128F RID: 4751
	private bool cubeDeleted;

	// Token: 0x04001290 RID: 4752
	private bool quadDeleted;

	// Token: 0x04001291 RID: 4753
	public bool armchairDeleted;

	// Token: 0x04001292 RID: 4754
	public bool houseDeleted;
}
