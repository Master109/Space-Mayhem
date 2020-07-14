using UnityEngine;
using System.Collections;
using Extensions;
using UnityEngine.SceneManagement;

namespace SpaceshipGame
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
					Ship e = (Ship) Instantiate(enemies[i], enemyCreateArea.bounds.ToRect().RandomPoint(), enemies[i].transform.rotation);
					e.waypoints = new Vector2[2];
					e.waypoints[0] = enemyStayArea.bounds.ToRect().RandomPoint();
					BoxCollider2D exitWaypointCreateArea = exitWaypointCreateAreas[Random.Range(0, (int) exitWaypointCreateAreas.Length)];
					e.waypoints[1] = exitWaypointCreateArea.bounds.ToRect().RandomPoint();
					e.waypointWaitTimes = new float[2];
					e.waypointWaitTimes[0] = enemyStayDurations[i];
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