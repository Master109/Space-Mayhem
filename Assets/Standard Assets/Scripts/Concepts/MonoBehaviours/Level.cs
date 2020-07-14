using UnityEngine;
using System.Collections;
using Extensions;
using System.Collections.Generic;
using TMPro;

namespace SpaceshipGame
{
	public class Level : MonoBehaviour
	{
		public int Score
		{
			get
			{
				return PlayerPrefs.GetInt("Level score: " + name, 0);
			}
			set
			{
				PlayerPrefs.SetInt("Level score: " + name, value);
			}
		}
		public bool IsComplete
		{
			get
			{
				return PlayerPrefs.GetInt("Level complete: " + name, 0) == 1;
			}
			set
			{
				PlayerPrefs.SetInt("Level complete: " + name, value.GetHashCode());
			}
		}
		public Ship[] enemies;
		public float[] enemyCreateTimes;
		public float[] enemyStayDurations;
		public BoxCollider2D[] enemyCreateAreas;
		public BoxCollider2D[] exitWaypointCreateAreas;
		public BoxCollider2D enemyStayArea;
		public BoxCollider2D asteroidSpawnArea;
		public Transform playerSpawnPoint;
		public TMP_Text healthText;
		public TMP_Text scoreText;
		int currentEnemy = 0;

		public virtual void Start ()
		{
			enemyCreateTimes[currentEnemy] -= Time.deltaTime;
			if (enemyCreateTimes[currentEnemy] < 0)
			{
				BoxCollider2D enemyCreateArea = enemyCreateAreas[Random.Range(0, (int) enemyCreateAreas.Length)];
				Enemy enemy = (Enemy) Instantiate(enemies[currentEnemy], enemyCreateArea.bounds.ToRect().RandomPoint(), enemies[currentEnemy].transform.rotation);
				enemy.waypoints = new Vector2[2];
				enemy.waypoints[0] = enemyStayArea.bounds.ToRect().RandomPoint();
				BoxCollider2D exitWaypointCreateArea = exitWaypointCreateAreas[Random.Range(0, (int) exitWaypointCreateAreas.Length)];
				enemy.waypoints[1] = exitWaypointCreateArea.bounds.ToRect().RandomPoint();
				enemy.waypointWaitTimes = new float[2];
				enemy.waypointWaitTimes[0] = enemyStayDurations[currentEnemy];
				currentEnemy ++;
				if (currentEnemy == enemies.Length)
					StartCoroutine(EndRoutine ());
			}
		}

		IEnumerator EndRoutine ()
		{
			do
			{
				if (GameManager.GetSingleton<Enemy>() == null)
				{
					WinLevel ();
					yield break;
				}
				yield return new WaitForEndOfFrame();
			} while (true);
		}

		public virtual void WinLevel ()
		{
			if (Player.score > Score)
			{
				Player.gold += (int) Player.score - Score;
				Score = (int) Player.score;
				IsComplete = true;
			}
			GameManager.GetSingleton<GameManager>().LoadScene ("Hangar");
		}
	}
}