using System.Collections;
using UnityEngine;

namespace SpaceMayhem
{
	[ExecuteInEditMode]
	public class ShipPart : MonoBehaviour
	{
		public static ShipPart selected;
		public Transform trs;
		public SpriteRenderer spriteRenderer;
		public int cost;
		public int speed;
		public float damageMultiplier = 1;
		public float grazeHeal;
		public int weight = 25;
		public int effectAngle = 45;
		public ParticleSystem particleSystem;
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
			if (indexOfCloneIndicator != -1)
			{
				if (speed == 0)
					enabled = false;
				GameObject cloneGo = GameObject.Find(name.Remove(indexOfCloneIndicator));
				if (cloneGo != null)
					spriteRenderer.sprite = cloneGo.GetComponent<SpriteRenderer>().sprite;
			}
		}
	}
}