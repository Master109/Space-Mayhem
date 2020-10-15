using UnityEngine;
using System.Collections;

namespace SpaceMayhem
{
	public class Destructable : MonoBehaviour
	{
		public Transform trs;
		[HideInInspector]
		public float hp;
		public int maxhp;
		public Timer invulnerableTimer;

		public virtual void Awake ()
		{
			hp = maxhp;
		}

		public virtual void TakeDamage (float amount)
		{
			if (amount > 0 && invulnerableTimer.isRunning)
				return;
			hp -= amount;
			hp = Mathf.Clamp(hp, 0, maxhp);
			if (hp <= 0)
				Death ();
		}

		public virtual void Death ()
		{
		}
	}
}