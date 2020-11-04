using UnityEngine;
using Extensions;

namespace SpaceMayhem
{
	public class LevelMap : MonoBehaviour
	{
		public LevelButton[] levelButtons = new LevelButton[0];
		public const string REPLACE_INDICATOR = "_";
		
		void Start ()
		{
			GameManager.GetSingleton<Player>().gameObject.SetActive(false);
			LevelButton previousLevelButton = null;
			for (int i = 0; i < levelButtons.Length; i ++)
			{
				LevelButton levelButton = levelButtons[i];
				if (levelButton.level.IsComplete)
					levelButton.text.text = levelButton.text.text.ReplaceFirst(REPLACE_INDICATOR, "" + levelButton.level.Score);
				else
				{
					if (levelButton.level is Survival)
						levelButton.text.text = levelButton.text.text.ReplaceFirst(REPLACE_INDICATOR, "Not attempted");
					else
						levelButton.text.text = levelButton.text.text.ReplaceFirst(REPLACE_INDICATOR, "Not completed");
				}
				if (i > 0 && previousLevelButton.level.IsComplete)
					levelButton.button.interactable = true;
				previousLevelButton = levelButton;
			}
		}
	}
}