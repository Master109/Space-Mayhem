using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Extensions;
using Utf8Json;
using System;
using Random = UnityEngine.Random;
using System.IO;
using System.Collections;

namespace SpaceMayhem
{
	//[ExecuteInEditMode]
	public class SaveAndLoadManager : SingletonMonoBehaviour<SaveAndLoadManager>
	{
		// [HideInInspector]
		public List<SaveAndLoadObject> saveAndLoadObjects = new List<SaveAndLoadObject>();
		public static SaveEntry[] saveEntries = new SaveEntry[0];
		// public static Dictionary<string, SaveAndLoadObject> saveAndLoadObjectTypeDict = new Dictionary<string, SaveAndLoadObject>();
		// public TemporaryActiveText displayOnSave;
		public int keepPastSavesCount;
		public bool usePlayerPrefs;
		[Multiline]
		public string savedData;
		public bool overwriteSaves;
		
#if UNITY_EDITOR
		public virtual void OnEnable ()
		{
			if (Application.isPlaying)
			{
				// displayOnSave.go.SetActive(false);
				return;
			}
			saveAndLoadObjects.Clear();
			saveAndLoadObjects.AddRange(FindObjectsOfType<SaveAndLoadObject>());
			foreach (SaveAndLoadObject saveAndLoadObject in saveAndLoadObjects)
			{
				saveAndLoadObject.saveables = saveAndLoadObject.GetComponentsInChildren<ISaveableAndLoadable>();
				if (saveAndLoadObject.uniqueId == MathfExtensions.NULL_INT || saveAndLoadObject.uniqueId == 0)
					saveAndLoadObject.uniqueId = Random.Range(int.MinValue, int.MaxValue);
				foreach (ISaveableAndLoadable saveableAndLoadable in saveAndLoadObject.saveables)
				{
					if (saveableAndLoadable.UniqueId == MathfExtensions.NULL_INT || saveableAndLoadable.UniqueId == 0)
						saveableAndLoadable.UniqueId = Random.Range(int.MinValue, int.MaxValue);
				}
			}
		}
#endif
		
		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
			if (!usePlayerPrefs)
				print(Application.persistentDataPath);
#endif
			Setup ();
		}

		public virtual void Setup ()
		{
			saveAndLoadObjects.Clear();
			saveAndLoadObjects.AddRange(FindObjectsOfType<SaveAndLoadObject>());
			// saveAndLoadObjectTypeDict.Clear();
			SaveAndLoadObject saveAndLoadObject;
			List<SaveEntry> _saveEntries = new List<SaveEntry>();
			for (int i = 0; i < saveAndLoadObjects.Count; i ++)
			{
				saveAndLoadObject = saveAndLoadObjects[i];
				saveAndLoadObject.Setup ();
				_saveEntries.AddRange(saveAndLoadObject.saveEntries);
			}
			saveEntries = _saveEntries.ToArray();
		}
		
		public virtual void SaveToCurrentAccount ()
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().SaveToCurrentAccount ();
				return;
			}
			Save (ArchivesManager.currentAccountIndex);
		}
		
		public virtual void Save (int accountIndex)
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().SaveToCurrentAccount ();
				return;
			}
			if (accountIndex != -1)
			{
				if (overwriteSaves)
					Save (accountIndex, ArchivesManager.Accounts[ArchivesManager.currentAccountIndex].MostRecentlyUsedSaveIndex + 1);
				else
					Save (accountIndex, ArchivesManager.Accounts[ArchivesManager.currentAccountIndex].LastSaveIndex + 1);
			}
			else
				Save (-1, -1);
		}
		
		public virtual void Save (int accountIndex, int saveIndex)
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().SaveToCurrentAccount ();
				return;
			}
			OnAboutToSave ();
			// Setup ();
			savedData = "";
			if (!usePlayerPrefs)
			{
				if (!File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "Saved Data.txt"))
					File.CreateText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Saved Data.txt");
				else
				{
					savedData = File.ReadAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Saved Data.txt");
					string[] valueGroups = savedData.Split(new string[] { SaveEntry.VALUE_GROUP_SEPERATOR }, StringSplitOptions.None);
					for (int i = 0; i < valueGroups.Length; i += 2)
					{
						string valueGroup = valueGroups[i];
						if (valueGroup.StartsWith("" + accountIndex))
							savedData = savedData.RemoveEach(valueGroup + SaveEntry.VALUE_GROUP_SEPERATOR + valueGroups[i + 1] + SaveEntry.VALUE_GROUP_SEPERATOR);
					}
				}
			}
			if (accountIndex != -1)
			{
				Account account = ArchivesManager.Accounts[accountIndex];
				account.MostRecentlyUsedSaveIndex = saveIndex;
				if (account.MostRecentlyUsedSaveIndex > ArchivesManager.CurrentlyPlaying.LastSaveIndex)
					account.LastSaveIndex ++;
				for (int i = 0; i < saveEntries.Length; i ++)
				{
					SaveEntry saveEntry = saveEntries[i];
					if (ArchivesManager.CurrentlyPlaying.MostRecentlyUsedSaveIndex > keepPastSavesCount)
						saveEntry.Delete (accountIndex, ArchivesManager.CurrentlyPlaying.LastSaveIndex - keepPastSavesCount - 1);
					saveEntry.Save (accountIndex, saveIndex);
				}
			}
			else
			{
				for (int i = 0; i < saveEntries.Length; i ++)
					saveEntries[i].Save (-1, -1);
			}
			if (!usePlayerPrefs)
				File.WriteAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Saved Data.txt", savedData);
			// displayOnSave.Do ();
		}

		void OnAboutToSave ()
		{
			// Player.instance.AddMoney (Player.addToMoneyOnSave);
			// Player.addToMoneyOnSave = 0;
			// Player.instance.DisplayMoney ();
			// for (int i = 0; i < Collectible.instances.Length; i ++)
			// {
			//     Collectible collectible = Collectible.instances[i];
			//     if (collectible.collected)
			// 		collectible.collectedAndSaved = true;
			// }
		}
		
		public virtual void LoadFromCurrentAccount ()
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().LoadFromCurrentAccount ();
				return;
			}
			Load (ArchivesManager.currentAccountIndex);
		}
		
		public virtual void Load (int accountIndex)
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().Load (accountIndex);
				return;
			}
			if (!usePlayerPrefs)
			{
				if (File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "Saved Data.txt"))
					savedData = File.ReadAllText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Saved Data.txt");
				else
					File.CreateText(Application.persistentDataPath + Path.DirectorySeparatorChar + "Saved Data.txt");
			}
			StartCoroutine(LoadRoutine (accountIndex));
			// if (accountIndex != -1)
			// 	Load (accountIndex, ArchivesManager.CurrentlyPlaying.MostRecentlyUsedSaveIndex);
			// else
			// 	Load (-1, -1);
		}

		IEnumerator LoadRoutine (int accountIndex)
		{
			if (accountIndex != -1)
				yield return StartCoroutine(LoadRoutine (accountIndex, ArchivesManager.CurrentlyPlaying.MostRecentlyUsedSaveIndex));
			else
				yield return StartCoroutine(LoadRoutine (-1, -1));
		}

		IEnumerator LoadRoutine (int accountIndex, int saveIndex)
		{
			yield return new WaitForEndOfFrame();
			Setup ();
			for (int i = 0; i < saveEntries.Length; i ++)
				saveEntries[i].Load (accountIndex, saveIndex);
			OnLoaded ();
		}

		// void Load (int accountIndex, int saveIndex)
		// {
		// 	Setup ();
		// 	for (int i = 0; i < saveEntries.Length; i ++)
		// 		saveEntries[i].Load (accountIndex, saveIndex);
		// 	OnLoaded ();
		// }

		public virtual void OnLoaded ()
		{
			// Player.instance.trs.position = Player.instance.spawnPosition;
			// GameManager.GetSingleton<GameCamera>().Awake ();
			// GameManager.GetSingleton<World>().Init ();
			GameManager.GetSingleton<AudioManager>().Awake ();
			// AccountSelectMenu.Init ();
			// for (int i = 0; i < Collectible.instances.Length; i ++)
			// {
			//     Collectible collectible = Collectible.instances[i];
			// 	if (collectible.collectedAndSaved)
			// 		collectible.OnCollected ();
			// }
			// Obelisk.instances = FindObjectsOfType<Obelisk>();
			// for (int i = 0; i < Obelisk.instances.Length; i ++)
			// {
			//     Obelisk obelisk = Obelisk.instances[i];
			//     if (obelisk.found)
			// 		obelisk.foundIndicator.SetActive(true);
			// }
			// Perk.instances = FindObjectsOfType<Perk>();
			// for (int i = 0; i < Perk.instances.Length; i++)
			// {
			//     Perk perk = Perk.instances[i];
			//     perk.Init ();
			// }
			// if (ArchivesManager.currentAccountIndex != -1)
			// 	Player.instance.DisplayMoney ();
			GameManager.GetSingleton<GameManager>().SetGosActive ();
			// GameManager.GetSingleton<WorldMap>().Init ();
			// if (GameManager.GetSingleton<GameManager>().movementJumpingShootingTutorialConversation.gameObject.activeSelf)
			// 	GameManager.GetSingleton<DialogManager>().StartConversation (GameManager.GetSingleton<GameManager>().movementJumpingShootingTutorialConversation);
		}

		public virtual void DeleteCurrentAccount ()
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().DeleteCurrentAccount ();
				return;
			}
			Delete (ArchivesManager.currentAccountIndex);
		}

		public virtual void Delete (int accountIndex)
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().Delete (accountIndex);
				return;
			}
			if (accountIndex != -1)
			{
				Account account = ArchivesManager.Accounts[accountIndex];
				for (int saveIndex = account.LastSaveIndex - keepPastSavesCount; saveIndex <= account.LastSaveIndex; saveIndex ++)
					Delete (accountIndex, saveIndex);
			}
			else
				Delete (-1, -1);
		}

		public virtual void Delete (int accountIndex, int saveIndex)
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().Delete (accountIndex, saveIndex);
				return;
			}
			for (int i = 0; i < saveEntries.Length; i ++)
				saveEntries[i].Delete (accountIndex, saveIndex);
			if (accountIndex != -1 && ArchivesManager.currentAccountIndex == accountIndex && saveIndex == ArchivesManager.CurrentlyPlaying.MostRecentlyUsedSaveIndex)
				ResetPersistantValues ();
			// Save (accountIndex, saveIndex);
		}

		public static void ResetPersistantValues ()
		{
			GameManager.enabledGosString = "";
			GameManager.disabledGosString = "";
			// WorldMap.exploredCellPositions.Clear();
			// WorldMap.exploredCellPositionsAtLastTimeOpened.Clear();
			// Player.addToMoneyOnSave = 0;
			ArchivesManager.currentAccountIndex = -1;
		}

		public virtual void DeleteAll ()
		{
			PlayerPrefs.DeleteAll();
			for (int accountIndex = -1; accountIndex < ArchivesManager.Accounts.Length; accountIndex ++)
				Delete (accountIndex);
		}

		public static string Serialize (object value, Type type)
		{
			return JsonSerializer.NonGeneric.ToJsonString(type, value);
		}

		public static object Deserialize (string serializedState, Type type)
		{
			return JsonSerializer.NonGeneric.Deserialize(type, serializedState);
		}
		
		public class SaveEntry
		{
			public ISaveableAndLoadable saveableAndLoadable;
			public MemberEntry[] memberEntries = new MemberEntry[0];
			public const string VALUE_SEPERATOR = "Ⅰ";
			public const string VALUE_GROUP_SEPERATOR = "@";
			
			public SaveEntry ()
			{
			}
			
			public virtual void Save (int accountIndex, int saveIndex)
			{
				foreach (MemberEntry memberEntry in memberEntries)
				{
					if (!memberEntry.isField)
					{
						PropertyInfo property = memberEntry.member as PropertyInfo;
						if (GameManager.GetSingleton<SaveAndLoadManager>().usePlayerPrefs)
						{
							string data = Serialize(property.GetValue(saveableAndLoadable), property.PropertyType);
							PlayerPrefs.SetString(GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry), data);
							GameManager.GetSingleton<SaveAndLoadManager>().savedData += data;
						}
						else
							GameManager.GetSingleton<SaveAndLoadManager>().savedData += GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry) + VALUE_GROUP_SEPERATOR + Serialize(property.GetValue(saveableAndLoadable), property.PropertyType) + VALUE_GROUP_SEPERATOR;
					}
					else
					{
						FieldInfo field = memberEntry.member as FieldInfo;
						if (GameManager.GetSingleton<SaveAndLoadManager>().usePlayerPrefs)
						{
							string data = Serialize(field.GetValue(saveableAndLoadable), field.FieldType);
							PlayerPrefs.SetString(GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry), data);
							GameManager.GetSingleton<SaveAndLoadManager>().savedData += data;
						}
						else
							GameManager.GetSingleton<SaveAndLoadManager>().savedData += GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry) + VALUE_GROUP_SEPERATOR + Serialize(field.GetValue(saveableAndLoadable), field.FieldType) + VALUE_GROUP_SEPERATOR;
					}
				}
			}
			
			public virtual void Load (int accountIndex, int saveIndex)
			{
				object value;
				foreach (MemberEntry memberEntry in memberEntries)
				{
					if (!memberEntry.isField)
					{
						PropertyInfo property = memberEntry.member as PropertyInfo;
						if (GameManager.GetSingleton<SaveAndLoadManager>().usePlayerPrefs)
						{
							value = Deserialize(PlayerPrefs.GetString(GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry), Serialize(property.GetValue(saveableAndLoadable), property.PropertyType)), property.PropertyType);
							property.SetValue(saveableAndLoadable, value);
						}
						else
						{
							string[] valueGroups = GameManager.GetSingleton<SaveAndLoadManager>().savedData.Split(new string[] { VALUE_GROUP_SEPERATOR }, StringSplitOptions.None);
							for (int i = 0; i < valueGroups.Length; i += 2)
							{
								string valueGroup = valueGroups[i];
								if (valueGroup == GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry))
								{
									valueGroup = valueGroups[i + 1];
									value = Deserialize(valueGroup, property.PropertyType);
									property.SetValue(saveableAndLoadable, value);
								}
							}
						}
					}
					else
					{
						FieldInfo field = memberEntry.member as FieldInfo;
						if (GameManager.GetSingleton<SaveAndLoadManager>().usePlayerPrefs)
						{
							value = Deserialize(PlayerPrefs.GetString(GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry), Serialize(field.GetValue(saveableAndLoadable), field.FieldType)), field.FieldType);
							field.SetValue(saveableAndLoadable, value);
						}
						else
						{
							string[] valueGroups = GameManager.GetSingleton<SaveAndLoadManager>().savedData.Split(new string[] { VALUE_GROUP_SEPERATOR }, StringSplitOptions.None);
							for (int i = 0; i < valueGroups.Length; i += 2)
							{
								string valueGroup = valueGroups[i];
								if (valueGroup == GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry))
								{
									valueGroup = valueGroups[i + 1];
									value = Deserialize(valueGroup, field.FieldType);
									field.SetValue(saveableAndLoadable, value);
								}
							}
						}
					}
				}
			}

			public virtual void Delete (int accountIndex, int saveIndex)
			{
				foreach (MemberEntry memberEntry in memberEntries)
				{
					if (GameManager.GetSingleton<SaveAndLoadManager>().usePlayerPrefs)
						PlayerPrefs.DeleteKey(GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry));
					else
					{
						string[] valueGroups = GameManager.GetSingleton<SaveAndLoadManager>().savedData.Split(new string[] { VALUE_GROUP_SEPERATOR }, StringSplitOptions.None);
						for (int i = 0; i < valueGroups.Length; i += 2)
						{
							string valueGroup = valueGroups[i];
							if (valueGroup == GetKeyNameForMemberEntry(accountIndex, saveIndex, memberEntry))
								GameManager.GetSingleton<SaveAndLoadManager>().savedData = GameManager.GetSingleton<SaveAndLoadManager>().savedData.RemoveEach(valueGroup + VALUE_GROUP_SEPERATOR + valueGroups[i + 1] + VALUE_GROUP_SEPERATOR);
						}
					}
				}
			}

			public virtual string GetKeyNameForMemberEntry (int accountIndex, int saveIndex, MemberEntry memberEntry)
			{
				// if (memberEntry.isShared)
				// 	return VALUE_SEPERATOR + saveableAndLoadable.UniqueId + VALUE_SEPERATOR + memberEntry.member.Name;
				// else
					return accountIndex + VALUE_SEPERATOR + saveIndex + VALUE_SEPERATOR + saveableAndLoadable.UniqueId + VALUE_SEPERATOR + memberEntry.member.Name;
			}

			public class MemberEntry
			{
				public MemberInfo member;
				public bool isField;
				// public bool isShared;
			}
		}
	}
}