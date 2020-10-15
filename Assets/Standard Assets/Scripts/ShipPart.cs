using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpaceMayhem
{
	[ExecuteInEditMode]
	public class ShipPart : MonoBehaviour
	{
		public static ShipPart selected;
		// public static Dictionary<string, Sprite> spritesDict = new Dictionary<string, Sprite>();
		public Transform trs;
		public SpriteRenderer spriteRenderer;
		public int cost;
		public int speed;
		public float damageMultiplier = 1;
		public float grazeHeal;
		public int weight = 25;
		public int effectAngle = 45;
		public new ParticleSystem particleSystem;
		public PolygonCollider2D polygonCollider;

		void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (spriteRenderer == null)
					spriteRenderer = GetComponent<SpriteRenderer>();
				if (particleSystem == null)
					particleSystem = GetComponentInChildren<ParticleSystem>();
				if (polygonCollider == null)
					polygonCollider = GetComponent<PolygonCollider2D>();
				// yield break;
				return;
			}
#endif
			int indexOfCloneIndicator = name.IndexOf("(Clone)");
			if (indexOfCloneIndicator == -1)
			{
				// yield return new WaitForEndOfFrame();
				// if (!spritesDict.ContainsKey(name))
				// 	spritesDict.Add(name, spriteRenderer.sprite);
				// else
				// 	spritesDict[name] = spriteRenderer.sprite;
			}
			else
			{
				if (speed == 0)
					enabled = false;
				GameObject cloneGo = GameObject.Find(name.Remove(indexOfCloneIndicator));
				if (cloneGo != null)
					spriteRenderer.sprite = cloneGo.GetComponent<SpriteRenderer>().sprite;
			}
		}

		void Update ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (!name.Contains("(Clone)"))
			{
				gameObject.SetActive(false);
				return;
			}
			particleSystem = GetComponentInChildren<ParticleSystem>();
			if (particleSystem != null)
			{
				StartCoroutine(TestRoutine ());
			}
		}

		IEnumerator TestRoutine ()
		{
			for (int i = 0; i < 100; i ++)
				yield return new WaitForEndOfFrame();
			DestroyImmediate(particleSystem.gameObject);
			particleSystem = Instantiate(GameManager.GetSingleton<Hangar>().thrusterParitcleSystem, trs);
			enabled = false;
		}

		// void OnApplicationQuit ()
		// {
		// 	spritesDict.Clear();
		// }
	}
}