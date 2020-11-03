using UnityEngine;
using System.Collections;
using Extensions;

namespace SpaceMayhem
{
	public class Bullet : Spawnable
	{
		public Rigidbody2D rigid;
		public int speed;
		public int damage;

		void Start ()
		{
			rigid.velocity = trs.up * speed;
		}

		void OnCollisionEnter2D (Collision2D coll)
		{
			float damageMultiplier = 1;
			if (GameManager.GetSingleton<Player>().gameObject == coll.gameObject)
			{
				foreach (ShipPart part in GameManager.GetSingleton<Player>().GetComponentsInChildren<ShipPart>())
				{
					if (part.damageMultiplier != 1)
					{
						Vector2 toBullet = trs.position - GameManager.GetSingleton<Player>().trs.position;
						Vector2 toPart = part.trs.position - GameManager.GetSingleton<Player>().trs.position;
						if (Vector2.Angle(toBullet, toPart) < part.effectAngle)
							damageMultiplier *= part.damageMultiplier;
					}
				}
			}
			coll.transform.root.GetComponent<Ship>().TakeDamage (damage * damageMultiplier);
			GameManager.GetSingleton<ObjectPool>().Despawn (prefabIndex, gameObject, trs);
		}
	}
}