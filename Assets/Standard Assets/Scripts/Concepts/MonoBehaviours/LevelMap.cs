using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SpaceshipGame
{
	public class LevelMap : MonoBehaviour
	{
		public LevelButton[] levelButtons = new LevelButton[0];
		
		void Start ()
		{
			GameManager.GetSingleton<Player>().gameObject.SetActive(false);
			LevelButton previousLevelButton = null;
			for (int i = 0; i < levelButtons.Length; i ++)
			{
				LevelButton levelButton = levelButtons[i];
				if (levelButton.level.IsComplete)
					levelButton.tooltip.text.text = "Best score: " + levelButton.level.Score;
				else
					levelButton.tooltip.text.text = "Best score: Need to complete level";
				if (i > 0 && previousLevelButton.level.IsComplete)
					levelButton.button.interactable = true;
				previousLevelButton = levelButton;
			}
		}
	}
}