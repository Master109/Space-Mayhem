using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SpaceMayhem
{
	public class LevelButton : MonoBehaviour
	{
		public Level level;
		public Button button;
		public Text text;

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
			for (int i = 0; i < GameManager.GetSingleton<Player>().weapons.Length; i ++)
			{
				Weapon weapon = GameManager.GetSingleton<Player>().weapons[i];
				weapon.shootAngle = weapon.trs.eulerAngles.z;
				weapon.enabled = true;
			}
		}
	}
}