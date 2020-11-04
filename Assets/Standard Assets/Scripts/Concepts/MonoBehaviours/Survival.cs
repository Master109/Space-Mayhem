using UnityEngine;
using Extensions;

namespace SpaceMayhem
{
	public class Survival : Level, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public float[] enemyCreateRates;
		float[] enemyCreateTimers;
		public float enemyCreateRateMultiplier = .975f;
		public float minEnemyCreateRate = .25f;
		
		public override void Start ()
		{
			enemyCreateTimers = new float[enemyCreateRates.Length];
			enemyCreateTimers[0] = enemyCreateRates[0];
			GameManager.updatables = GameManager.updatables.Add(this);
		}
		
		public void DoUpdate ()
		{
			for (int i = 0; i < enemies.Length; i ++)
			{
				enemyCreateTimers[i] += Time.deltaTime;
				if (enemyCreateTimers[i] > enemyCreateRates[i])
				{
					BoxCollider2D enemyCreateArea = enemyCreateAreas[Random.Range(0, (int) enemyCreateAreas.Length)];
					Ship enemy = (Ship) Instantiate(enemies[i], enemyCreateArea.bounds.ToRect().RandomPoint(), enemies[i].transform.rotation);
					enemy.waypoints = new Vector2[2];
					enemy.waypoints[0] = enemyStayArea.bounds.ToRect().RandomPoint();
					BoxCollider2D exitWaypointCreateArea = exitWaypointCreateAreas[Random.Range(0, (int) exitWaypointCreateAreas.Length)];
					enemy.waypoints[1] = exitWaypointCreateArea.bounds.ToRect().RandomPoint();
					enemy.waypointWaitTimes = new float[2];
					enemy.waypointWaitTimes[0] = enemyStayDurations[i];
					enemyCreateTimers[i] = 0;
					enemyCreateRates[i] *= enemyCreateRateMultiplier;
					if (enemyCreateRates[i] < minEnemyCreateRate)
						enemyCreateRates[i] = minEnemyCreateRate;
				}
			}
		}

		void OnDestroy ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
	}
}