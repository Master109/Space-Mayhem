using UnityEngine;
using System.Collections;
using Extensions;

namespace SpaceshipGame
{
	public class Ship : Destructable, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Rigidbody2D rigid;
		public int speed;
		public Vector2[] waypoints;
		public float[] waypointWaitTimes;
		public int scoreReward;
		public int hpReward;
		public bool canShoot;
		public Weapon[] weapons = new Weapon[0];
		Vector2 toPlayer;
		int currentWaypoint;
		public Explosion explosionPrefab;

		void OnEnable ()
		{
			GameManager.updatables = GameManager.updatables.Add(this);
		}
		
		public virtual void DoUpdate ()
		{
			if (GameManager.GetSingleton<Player>() == null)
				return;
			toPlayer = GameManager.GetSingleton<Player>().trs.position - trs.position;
			if (canShoot)
			{
				foreach (Weapon weapon in weapons)
					weapon.Shoot ();
			}
			FollowWaypoints ();
		}

		void FollowWaypoints ()
		{
			Vector2 toWaypoint = waypoints[currentWaypoint] - (Vector2) trs.position;
			float moveSpeed = Mathf.Clamp(toWaypoint.magnitude * 10, 0, speed);
			rigid.velocity = toWaypoint.normalized * moveSpeed;
			toWaypoint = waypoints[currentWaypoint] - (Vector2) trs.position;
			trs.rotation = Quaternion.LookRotation(Vector3.forward, toPlayer);
			if (toWaypoint.magnitude < 1)
			{
				waypointWaitTimes[currentWaypoint] -= Time.deltaTime;
				if (waypointWaitTimes[currentWaypoint] < 0)
				{
					currentWaypoint ++;
					if (currentWaypoint >= waypoints.Length)
						Destroy(gameObject);
				}
			}
		}

		public override void Death ()
		{
			base.Death ();
			GameManager.GetSingleton<Player>().GetComponent<Destructable>().TakeDamage (-hpReward);
			Player.score += scoreReward;
			GameManager.GetSingleton<Level>().scoreText.text = "Score: " + (int) Player.score;
			Explosion explosion = GameManager.GetSingleton<ObjectPool>().SpawnComponent<Explosion>(explosionPrefab, trs.position);
			Destroy(explosion.gameObject, explosion.anim.clip.length);
			Destroy(gameObject);
		}

		void OnDisable ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
	}
}