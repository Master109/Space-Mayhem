using UnityEngine;
using System.Collections;
using Extensions;

namespace SpaceshipGame
{
	public class HomingBullet : Bullet, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Transform target;
		public float rotateRate;
		public CircleCollider2D followRangeCollider;
		public float deathTime;
		float followRange;
		Vector2 heading;
		Vector2 toTarget;

		void OnEnable ()
		{
			followRange = followRangeCollider.radius;
			GameManager.updatables = GameManager.updatables.Add(this);
			GameManager.GetSingleton<ObjectPool>().DelayDespawn (prefabIndex, gameObject, trs, deathTime);
		}
		
		public void DoUpdate ()
		{
			if (target == null)
				return;
			toTarget = target.position - trs.position;
			followRangeCollider.radius = toTarget.magnitude;
			// Vector2 headingToClosestOpponent = toTarget - heading;
			// heading += headingToClosestOpponent.normalized * rotateRate * Time.deltaTime;
			// heading = heading.normalized;
			trs.rotation = Quaternion.LookRotation(Vector3.forward, heading);
			rigid.velocity = trs.up * speed;
		}

		void OnTriggerStay2D (Collider2D other)
		{
			target = other.GetComponent<Transform>().root;
			toTarget = target.position - trs.position;
			followRangeCollider.radius = toTarget.magnitude;
		}

		void OnTriggerExit2D (Collider2D other)
		{
			followRangeCollider.radius = followRange;
		}

		void OnDisable ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
	}
}