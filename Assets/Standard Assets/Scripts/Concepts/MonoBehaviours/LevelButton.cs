using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SpaceshipGame
{
	public class LevelButton : MonoBehaviour
	{
		public Level level;
		public Button button;
		public Tooltip tooltip;

		public void PlayLevel ()
		{
			GameManager.GetSingleton<GameManager>().LoadScene ("Level");
			GameManager.GetSingleton<GameManager>().StartCoroutine(PlayLevelRoutine ());
		}

		IEnumerator PlayLevelRoutine ()
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			Instantiate(level);
			GameManager.GetSingleton<GameManager>().PauseGame (true);
			GameManager.GetSingleton<Player>().trs.position = GameManager.GetSingleton<Level>().playerSpawnPoint.position;
			GameManager.GetSingleton<Player>().gameObject.SetActive(true);
			GameManager.GetSingleton<Player>().enabled = true;
		}
	}
}