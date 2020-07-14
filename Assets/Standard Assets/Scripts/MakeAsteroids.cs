using UnityEngine;
using System.Collections;
using Extensions;

namespace SpaceshipGame
{
	public class MakeAsteroids : MonoBehaviour
	{
		public FloatRange asteroidCreateRate;
		public Asteroid asteroidPrefab;
		public Sprite[] asteroidSprites;

		void Start ()
		{
			float asteroidCreateDelay = Random.Range(asteroidCreateRate.min, asteroidCreateRate.max);
			EventManager.events.Add(new EventManager.Event(MakeAsteroid, Time.time + asteroidCreateDelay));
		}
		
		void MakeAsteroid ()
		{
			Asteroid asteroid = GameManager.GetSingleton<ObjectPool>().SpawnComponent<Asteroid>(asteroidPrefab, GameManager.GetSingleton<Level>().asteroidSpawnArea.bounds.ToRect().RandomPoint(), Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
			asteroid.spriteRenderer.sprite = asteroidSprites[Random.Range(0, asteroidSprites.Length)];
			float asteroidCreateDelay = Random.Range(asteroidCreateRate.min, asteroidCreateRate.max);
			EventManager.events.Add(new EventManager.Event(MakeAsteroid, Time.time + asteroidCreateDelay));
		}
	}
}