﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using Extensions;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SpaceMayhem
{
	//[ExecuteInEditMode]
	public class GameManager : SingletonMonoBehaviour<GameManager>, ISaveableAndLoadable
	{
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		public int UniqueId
		{
			get
			{
				return uniqueId;
			}
			set
			{
				uniqueId = value;
			}
		}
		public static bool HasPlayedBefore
		{
			get
			{
				return PlayerPrefs.GetInt("Has played before ", 0) == 1;
			}
			set
			{
				PlayerPrefs.SetInt("Has played before ", value.GetHashCode());
			}
		}
		public static float UnscaledDeltaTime
		{
			get
			{
				if (paused || framesSinceLoadedScene <= LAG_FRAMES_AFTER_LOAD_SCENE)
					return 0;
				else
					return Time.unscaledDeltaTime;
			}
		}
		public static bool HasShip
		{
			get
			{
				return PlayerPrefs.GetInt("Has ship", 0) == 1;
			}
			set
			{
				PlayerPrefs.SetInt("Has ship", value.GetHashCode());
			}
		}
#if UNITY_EDITOR
		public static int[] UniqueIds
		{
			get
			{
				int[] output = new int[0];
				string[] uniqueIdsString = EditorPrefs.GetString("Unique ids").Split(UNIQUE_ID_SEPERATOR);
				int uniqueIdParsed;
				foreach (string uniqueIdString in uniqueIdsString)
				{
					if (int.TryParse(uniqueIdString, out uniqueIdParsed))
						output = output.Add(uniqueIdParsed);
				}
				return output;
			}
			set
			{
				string uniqueIdString = "";
				foreach (int uniqueId in value)
					uniqueIdString += uniqueId + UNIQUE_ID_SEPERATOR;
				EditorPrefs.SetString("Unique ids", uniqueIdString);
			}
		}
#endif
		public const char UNIQUE_ID_SEPERATOR = ',';
		public const int LAG_FRAMES_AFTER_LOAD_SCENE = 2;
		public const string STRING_SEPERATOR = "|";
		public const float WORLD_SCALE = .866f;
		public const float WORLD_SCALE_SQR = WORLD_SCALE * WORLD_SCALE;
		public static int framesSinceLoadedScene;
		public static bool paused;
		public static bool initialized;
		[SaveAndLoadValue(false)]
		public static string enabledGosString = "";
		[SaveAndLoadValue(false)]
		public static string disabledGosString = "";
		public static Dictionary<string, CursorEntry> cursorEntriesDict = new Dictionary<string, CursorEntry>();
		public static CursorEntry activeCursorEntry;
		public static Dictionary<string, GameModifier> gameModifierDict = new Dictionary<string, GameModifier>();
		public static IUpdatable[] updatables = new IUpdatable[0];
		public static IUpdatable[] pausedUpdatables = new IUpdatable[0];
		public static Dictionary<Type, object> singletons = new Dictionary<Type, object>();
		public static Vector2 previousMousePosition;
		public static bool isFocused;
		public delegate void OnGameScenesLoaded();
		public static event OnGameScenesLoaded onGameScenesLoaded;
		static Vector3 lowPassValue;
#if UNITY_EDITOR
		public bool doEditorUpdates;
#endif
		public int uniqueId;
		public float timeScale;
		public Team[] teams;
		public GameObject[] registeredGos = new GameObject[0];
		public Animator screenEffectAnimator;
		public CursorEntry[] cursorEntries;
		public RectTransform cursorCanvas;
		public GameModifier[] gameModifiers;
		public Timer hideCursorTimer;
		public GameScene[] gameScenes;
		public Canvas[] canvases = new Canvas[0];
		public TemporaryActiveText notificationText;
		[SaveAndLoadValue(false)]
		public static PlayerEntry[] playerEntries = new PlayerEntry[0];
		// public float accelerometerUpdateInterval = 1.0f / 60.0f;
		// public float lowPassKernelWidthInSeconds = 1.0f;
		// public float shakeDetectionThreshold = 2.0f;
		// static float lowPassFilterFactor;
		// Vector3 acceleration;
		// Vector2 moveInput;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				framesSinceLoadedScene = 0;
				Transform[] transforms = FindObjectsOfType<Transform>();
				IIdentifiable[] identifiables = new IIdentifiable[0];
				foreach (Transform trs in transforms)
				{
					identifiables = trs.GetComponents<IIdentifiable>();
					foreach (IIdentifiable identifiable in identifiables)
					{
						if (!UniqueIds.Contains(identifiable.UniqueId))
							UniqueIds = UniqueIds.Add(identifiable.UniqueId);
					}
				}
				return;
			}
			// else
			// {
			// 	for (int i = 0; i < gameScenes.Length; i ++)
			// 	{
			// 		if (!gameScenes[i].use)
			// 		{
			// 			gameScenes = gameScenes.RemoveAt(i);
			// 			i --;
			// 		}
			// 	}
			// }
#endif
			base.Awake ();
			singletons.Remove(GetType());
			singletons.Add(GetType(), this);
			// InitCursor ();
			// ArchivesManager.currentAccountIndex = 0;
			// if (SceneManager.GetActiveScene().name == "Init")
			// 	LoadGameScenes ();
			// else if (GetSingleton<GameCamera>() != null)
				StartCoroutine(OnGameSceneLoadedRoutine ());
			// lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
			// shakeDetectionThreshold *= shakeDetectionThreshold;
			// lowPassValue = InputManager.Acceleration;
		}

		void Init ()
		{
			if (!HasShip)
				return;
			if (GetSingleton<Player>() == null)
			{
				// LevelSerializer.LoadObjectTreeFromFile("Player");
				if (playerEntries.Length > 0)
				{
					Player player = playerEntries[0].MakeInstance();
					player.enabled = false;
				}
				// GetSingleton<Player>().enabled = false;
			}
			else
				GetSingleton<Player>().gameObject.SetActive(true);
			initialized = true;
		}
		public IEnumerator OnGameSceneLoadedRoutine ()
		{
			gameModifierDict.Clear();
			foreach (GameModifier gameModifier in gameModifiers)
				gameModifierDict.Add(gameModifier.name, gameModifier);
			hideCursorTimer.onFinished += HideCursor;
			if (screenEffectAnimator != null)
				screenEffectAnimator.Play("None");
			// GetSingleton<PauseMenu>().Hide ();
			if (ArchivesManager.currentAccountIndex != -1)
			{
				// GetSingleton<AccountSelectMenu>().gameObject.SetActive(false);
				PauseGame (false);
			}
			// canvases = FindObjectsOfType<Canvas>();
			// foreach (Canvas canvas in canvases)
			// 	canvas.worldCamera = GetSingleton<GameCamera>().camera;
			if (onGameScenesLoaded != null)
			{
				onGameScenesLoaded ();
				onGameScenesLoaded = null;
			}
			yield return StartCoroutine(LoadRoutine ());
			yield return new WaitForEndOfFrame();
			Init ();
			yield break;
		}

		void Update ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
				if (!initialized)
					return;
			// try
			// {
				InputSystem.Update ();
				foreach (IUpdatable updatable in updatables)
					updatable.DoUpdate ();
				Physics2D.Simulate(Time.deltaTime);
				GetSingleton<ObjectPool>().DoUpdate ();
				// GetSingleton<GameCamera>().DoUpdate ();
				// acceleration = InputManager.Acceleration;
				// lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
				// if (((acceleration - lowPassValue).sqrMagnitude >= shakeDetectionThreshold || Input.GetKeyDown(KeyCode.Escape)) && !paused)
				// if (Input.GetKeyDown(KeyCode.Escape) && !paused)
				// 	GetSingleton<PauseMenu>().Open ();
				framesSinceLoadedScene ++;
				previousMousePosition = InputManager.MousePosition;
			// }
			// catch (Exception e)
			// {
			// 	Debug.Log(e.Message + "\n" + e.StackTrace);
			// }
		}

		public virtual void InitCursor ()
		{
			cursorEntriesDict.Clear();
			foreach (CursorEntry cursorEntry in cursorEntries)
			{
				cursorEntriesDict.Add(cursorEntry.name, cursorEntry);
				cursorEntry.rectTrs.gameObject.SetActive(false);
			}
			// Cursor.visible = false;
			activeCursorEntry = null;
			// cursorEntriesDict["Default"].SetAsActive ();
		}

		public virtual IEnumerator LoadRoutine ()
		{
			// yield return new WaitForEndOfFrame();
			GetSingleton<SaveAndLoadManager>().Setup ();
			if (!HasPlayedBefore)
			{
				GetSingleton<SaveAndLoadManager>().DeleteAll ();
				HasPlayedBefore = true;
				GetSingleton<SaveAndLoadManager>().OnLoaded ();
			}
			else
				GetSingleton<SaveAndLoadManager>().LoadFromCurrentAccount ();
			// GetSingleton<AdsManager>().ShowAd ();
			Init ();
			yield break;
		}

		public virtual void HideCursor (params object[] args)
		{
			activeCursorEntry.rectTrs.gameObject.SetActive(false);
		}

		public virtual void LoadScene (string name)
		{
			if (GetSingleton<GameManager>() != this)
			{
				GetSingleton<GameManager>().LoadScene (name);
				return;
			}
			Bullet[] bullets = FindObjectsOfType<Bullet>();
			foreach (Bullet bullet in bullets)
				Destroy(bullet.gameObject);
			framesSinceLoadedScene = 0;
			SceneManager.LoadScene(name);
		}

		public virtual void LoadSceneAdditive (string name)
		{
			if (GetSingleton<GameManager>() != this)
			{
				GetSingleton<GameManager>().LoadSceneAdditive (name);
				return;
			}
			SceneManager.LoadScene(name, LoadSceneMode.Additive);
		}

		public virtual void LoadScene (int index)
		{
			LoadScene (SceneManager.GetSceneByBuildIndex(index).name);
		}

		public virtual void UnloadScene (string name)
		{
			AsyncOperation unloadGameScene = SceneManager.UnloadSceneAsync(name);
			unloadGameScene.completed += OnGameSceneUnloaded;
		}

		public virtual void OnGameSceneUnloaded (AsyncOperation unloadGameScene)
		{
			unloadGameScene.completed -= OnGameSceneUnloaded;
		}

		public virtual void ReloadActiveScene ()
		{
			LoadScene (SceneManager.GetActiveScene().name);
		}

		public virtual void LoadGameScenes ()
		{
			if (GetSingleton<GameManager>() != this)
			{
				GetSingleton<GameManager>().LoadGameScenes ();
				return;
			}
			initialized = false;
			StopAllCoroutines ();
			if (SceneManager.GetSceneByName(gameScenes[0].name).isLoaded)
			{
				// UnloadScene ("Game");
				// LoadSceneAdditive ("Game");
				return;
			}
			LoadScene (gameScenes[0].name);
			GameScene gameScene;
			for (int i = 1; i < gameScenes.Length; i ++)
			{
				gameScene = gameScenes[i];
				if (gameScene.use)
					LoadSceneAdditive (gameScene.name);
			}
		}

		public virtual void PauseGame (bool pause)
		{
			paused = pause;
			Time.timeScale = timeScale * (1 - paused.GetHashCode());
			// AudioListener.pause = paused;
		}

		public virtual void Quit ()
		{
			Application.Quit();
		}

		public virtual void OnApplicationQuit ()
		{
			PauseGame (true);
			if (ArchivesManager.currentAccountIndex != -1)
				ArchivesManager.CurrentlyPlaying.PlayTime += Time.time;
			GetSingleton<SaveAndLoadManager>().SaveToCurrentAccount ();
		}

		public virtual void OnApplicationFocus (bool isFocused)
		{
			GameManager.isFocused = isFocused;
			if (isFocused)
			{
				foreach (IUpdatable pausedUpdatable in pausedUpdatables)
					updatables = updatables.Add(pausedUpdatable);
				pausedUpdatables = new IUpdatable[0];
				foreach (Timer runningTimer in Timer.runningInstances)
					runningTimer.pauseIfCan = false;
				foreach (TemporaryActiveGameObject tempActiveGo in TemporaryActiveGameObject.activeInstances)
					tempActiveGo.Do ();
			}
			else
			{
				IUpdatable updatable;
				for (int i = 0; i < updatables.Length; i ++)
				{
					updatable = updatables[i];
					if (updatable.PauseWhileUnfocused)
					{
						pausedUpdatables = pausedUpdatables.Add(updatable);
						updatables = updatables.RemoveAt(i);
						i --;
					}
				}
				foreach (Timer runningTimer in Timer.runningInstances)
					runningTimer.pauseIfCan = true;
				foreach (TemporaryActiveGameObject tempActiveGo in TemporaryActiveGameObject.activeInstances)
					tempActiveGo.Do ();
			}
		}

		public virtual void SetGosActive ()
		{
			if (GetSingleton<GameManager>() != this)
			{
				GetSingleton<GameManager>().SetGosActive ();
				return;
			}
			string[] stringSeperators = { STRING_SEPERATOR };
			if (enabledGosString == null)
				enabledGosString = "";
			string[] enabledGos = enabledGosString.Split(stringSeperators, StringSplitOptions.None);
			foreach (string goName in enabledGos)
			{
				for (int i = 0; i < registeredGos.Length; i ++)
				{
					if (goName == registeredGos[i].name)
					{
						registeredGos[i].SetActive(true);
						break;
					}
				}
			}
			if (disabledGosString == null)
				disabledGosString = "";
			string[] disabledGos = disabledGosString.Split(stringSeperators, StringSplitOptions.None);
			foreach (string goName in disabledGos)
			{
				GameObject go = GameObject.Find(goName);
				if (go != null)
					go.SetActive(false);
			}
		}
		
		public virtual void ActivateGoForever (GameObject go)
		{
			go.SetActive(true);
			ActivateGoForever (go.name);
		}
		
		public virtual void DeactivateGoForever (GameObject go)
		{
			go.SetActive(false);
			DeactivateGoForever (go.name);
		}
		
		public virtual void ActivateGoForever (string goName)
		{
			disabledGosString = disabledGosString.Replace(STRING_SEPERATOR + goName, "");
			if (!enabledGosString.Contains(goName))
				enabledGosString += STRING_SEPERATOR + goName;
		}
		
		public virtual void DeactivateGoForever (string goName)
		{
			enabledGosString = enabledGosString.Replace(STRING_SEPERATOR + goName, "");
			if (!disabledGosString.Contains(goName))
				disabledGosString += STRING_SEPERATOR + goName;
		}

		public virtual void SetGameObjectActive (string name)
		{
			GameObject.Find(name).SetActive(true);
		}

		public virtual void SetGameObjectInactive (string name)
		{
			GameObject.Find(name).SetActive(false);
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (GetSingleton<GameManager>() != this)
				return;
			StopAllCoroutines();
			for (int i = 0; i < Timer.runningInstances.Length; i ++)
			{
				Timer timer = Timer.runningInstances[i];
				timer.Stop ();
				i --;
			}
			hideCursorTimer.onFinished -= HideCursor;
			// SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		public virtual void _Log (object o)
		{
			print(o);
		}

		public static void Log (object o)
		{
			print(o);
		}

		public void SetPause (bool pause)
		{
			if (pause)
				Time.timeScale = 0;
			else
				Time.timeScale = 1;
		}

		public void DeleteData ()
		{
			PlayerPrefs.DeleteAll();
			Player.Gold = Player.INIT_GOLD;
			if (GetSingleton<Player>() != null)
				Destroy(GetSingleton<Player>().gameObject);
			SceneManager.LoadScene(0);
		}

		public static Object Clone (Object obj)
		{
			return Instantiate(obj);
		}

		public static Object Clone (Object obj, Transform parent)
		{
			return Instantiate(obj, parent);
		}

		public static Object Clone (Object obj, Vector3 position, Quaternion rotation)
		{
			return Instantiate(obj, position, rotation);
		}

		public static void _Destroy (Object obj)
		{
			Destroy(obj);
		}

		public static void _DestroyImmediate (Object obj)
		{
			DestroyImmediate(obj);
		}

		public virtual void ToggleGo (GameObject go)
		{
			go.SetActive(!go.activeSelf);
		}

		public virtual void PressButton (Button button)
		{
			button.onClick.Invoke();
		}

		public static T GetSingleton<T> ()
		{
			object obj = default(T);
			if (!singletons.TryGetValue(typeof(T), out obj))
				obj = GetSingleton<T>(FindObjectsOfType<Object>());
			return (T) obj;
		}

		public static T GetSingleton<T> (Object[] objects)
		{
			if (typeof(T).IsSubclassOf(typeof(Object)))
			{
				for (int i = 0; i < objects.Length; i ++)
				{
					Object obj = objects[i];
					if (obj is T)
					{
						if (singletons.ContainsKey(typeof(T)))
							singletons[typeof(T)] = obj;
						else
							singletons.Add(typeof(T), obj);
						return (T) (object) obj;
					}
				}
			}
			return default(T);
		}

		// public static T GetSingletonIncludeAssets<T> ()
		// {
		// 	if (!singletons.ContainsKey(typeof(T)))
		// 		return GetSingletonIncludeAssets<T>(FindObjectsOfTypeIncludingAssets(typeof(T)));
		// 	else
		// 	{
		// 		if (singletons[typeof(T)] == null || singletons[typeof(T)].Equals(default(T)))
		// 		{
		// 			T singleton = GetSingletonIncludeAssets<T>(FindObjectsOfTypeIncludingAssets(typeof(T)));
		// 			singletons[typeof(T)] = singleton;
		// 			return singleton;
		// 		}
		// 		else
		// 			return (T) singletons[typeof(T)];
		// 	}
		// }

		// public static T GetSingletonIncludeAssets<T> (object[] objects)
		// {
		// 	if (typeof(T).IsSubclassOf(typeof(object)))
		// 	{
		// 		foreach (Object obj in objects)
		// 		{
		// 			if (obj is T)
		// 			{
		// 				singletons.Remove(typeof(T));
		// 				singletons.Add(typeof(T), obj);
		// 				break;
		// 			}
		// 		}
		// 	}
		// 	if (singletons.ContainsKey(typeof(T)))
		// 		return (T) singletons[typeof(T)];
		// 	else
		// 		return default(T);
		// }

		public static bool ModifierIsActiveAndExists (string name)
		{
			GameModifier gameModifier;
			if (gameModifierDict.TryGetValue(name, out gameModifier))
				return gameModifier.isActive;
			else
				return false;
		}

		public static bool ModifierIsActive (string name)
		{
			return gameModifierDict[name].isActive;
		}

		public static bool ModifierExists (string name)
		{
			return gameModifierDict.ContainsKey(name);
		}

		[Serializable]
		public class CursorEntry
		{
			public string name;
			public RectTransform rectTrs;

			public virtual void SetAsActive ()
			{
				if (activeCursorEntry != null)
					activeCursorEntry.rectTrs.gameObject.SetActive(false);
				rectTrs.gameObject.SetActive(true);
				activeCursorEntry = this;
			}
		}

		[Serializable]
		public class GameModifier
		{
			public string name;
			public bool isActive;
		}

		[Serializable]
		public class GameScene
		{
			public string name;
			public bool use = true;
		}

		public struct PlayerEntry
		{
			public ShipPartEntry[] shipPartEntries;
			public const string NAME_AND_VALUE_SEPERATOR = ":";
			public const string COLLECTION_ELEMENT_SEPERATOR = ",";
			public const string NAME_AND_VALUE_PAIR_SEPERATOR = ";";

			public PlayerEntry (ShipPartEntry[] shipPartEntries)
			{
				this.shipPartEntries = shipPartEntries;
			}

			public override string ToString ()
			{
				string output = "";
				for (int i = 0; i < shipPartEntries.Length; i ++)
				{
					ShipPartEntry shipPartEntry = shipPartEntries[i];
					output += shipPartEntry + COLLECTION_ELEMENT_SEPERATOR;
				}
				return output;
			}

			public static PlayerEntry FromString (string data)
			{
				string[] nameAndValuePairs = data.Split(new string[1] { NAME_AND_VALUE_PAIR_SEPERATOR }, StringSplitOptions.None);
				string[] shipPartEntriesStrings = ExtractValue(nameAndValuePairs[0]).Split(new string [1] { COLLECTION_ELEMENT_SEPERATOR }, StringSplitOptions.RemoveEmptyEntries);
                List<ShipPartEntry> shipPartEntries = new List<ShipPartEntry>();
                for (int i = 0; i < shipPartEntriesStrings.Length; i ++)
                {
                    string shipPartEntryString = shipPartEntriesStrings[i];
                    shipPartEntries.Add(ShipPartEntry.FromString(shipPartEntryString));
                }
                return new PlayerEntry(shipPartEntries.ToArray());
			}

			static string ExtractValue (string nameAndValuePair)
			{
				return nameAndValuePair.Substring(nameAndValuePair.IndexOf(NAME_AND_VALUE_SEPERATOR) + NAME_AND_VALUE_SEPERATOR.Length);
			}

			public static PlayerEntry FromInstance (Player player)
			{
				ShipPartEntry[] shipPartEntries = new ShipPartEntry[player.shipParts.Length];
				for (int i = 0; i < shipPartEntries.Length; i ++)
				{
					ShipPartEntry shipPartEntry = shipPartEntries[i];
					shipPartEntry = ShipPartEntry.FromInstance(player.shipParts[i]);
					shipPartEntries[i] = shipPartEntry;
				}
				return new PlayerEntry(shipPartEntries);
			}

			public Player MakeInstance ()
			{
				Player player = Instantiate(GetSingleton<Hangar>().playerPrefab);
				for (int i = 0; i < shipPartEntries.Length; i ++)
				{
					ShipPartEntry shipPartEntry = shipPartEntries[i];
					ShipPart shipPart = shipPartEntry.MakeInstance ();
					shipPart.trs.SetParent(player.trs);
					if (shipPart.speed > 0)
						player.thrusters = player.thrusters.Add(shipPart);
					shipPart.gameObject.SetActive(true);
				}
				player.shipParts = player.GetComponentsInChildren<ShipPart>();
				player.weapons = player.GetComponentsInChildren<Weapon>();
				return player;
			}
			
			public struct ShipPartEntry
			{
				public int partIndex;
				public Vector2 position;
				public float rotation;
				public int sortingOrder;
				public const string NAME_AND_VALUE_SEPERATOR = ":";
				public const string NAME_AND_VALUE_PAIR_SEPERATOR = ";";

				public ShipPartEntry (int partIndex, Vector2 position, float rotation, int sortingOrder)
				{
					this.partIndex = partIndex;
					this.position = position;
					this.rotation = rotation;
					this.sortingOrder = sortingOrder;
				}

				public override string ToString ()
				{
					return nameof(partIndex) + NAME_AND_VALUE_SEPERATOR + partIndex + NAME_AND_VALUE_PAIR_SEPERATOR + 
						nameof(position.x) + NAME_AND_VALUE_SEPERATOR + position.x + NAME_AND_VALUE_PAIR_SEPERATOR + 
						nameof(position.y) + NAME_AND_VALUE_SEPERATOR + position.y + NAME_AND_VALUE_PAIR_SEPERATOR + 
						nameof(rotation) + NAME_AND_VALUE_SEPERATOR + rotation + NAME_AND_VALUE_PAIR_SEPERATOR + 
						nameof(sortingOrder) + NAME_AND_VALUE_SEPERATOR + sortingOrder + NAME_AND_VALUE_PAIR_SEPERATOR;
				}

				public static ShipPartEntry FromString (string data)
				{
					string[] nameAndValuePairs = data.Split(new string[1] { NAME_AND_VALUE_PAIR_SEPERATOR }, StringSplitOptions.None);
					int partIndex = int.Parse(ExtractValue(nameAndValuePairs[0]));
					float positionX = float.Parse(ExtractValue(nameAndValuePairs[1]));
					float positionY = float.Parse(ExtractValue(nameAndValuePairs[2]));
					float rotation = float.Parse(ExtractValue(nameAndValuePairs[3]));
					int sortingOrder = int.Parse(ExtractValue(nameAndValuePairs[4]));
					Vector2 position = new Vector2(positionX, positionY);
					return new ShipPartEntry(partIndex, position, rotation, sortingOrder);
				}

				static string ExtractValue (string nameAndValuePair)
				{
					return nameAndValuePair.Substring(nameAndValuePair.IndexOf(NAME_AND_VALUE_SEPERATOR) + NAME_AND_VALUE_SEPERATOR.Length);
				}

				public static ShipPartEntry FromInstance (ShipPart shipPart)
				{
					int partIndex = MathfExtensions.NULL_INT;
					for (int i = 0; i < GetSingleton<Hangar>().shipPartPrefabs.Length; i ++)
					{
						ShipPart shipPartPrefab = GetSingleton<Hangar>().shipPartPrefabs[i];
						if (shipPartPrefab.name == shipPart.name.Replace("(Clone)", ""))
						{
							partIndex = i;
							break;
						}
					}
					return new ShipPartEntry(partIndex, shipPart.trs.position, shipPart.trs.eulerAngles.z, shipPart.spriteRenderer.sortingOrder);
				}

				public ShipPart MakeInstance ()
				{
					ShipPart shipPart = Instantiate(GetSingleton<Hangar>().shipPartPrefabs[partIndex]);
					shipPart.trs.position = position;
					shipPart.trs.eulerAngles = Vector3.forward * rotation;
					shipPart.spriteRenderer.sortingOrder = sortingOrder;
					return shipPart;
				}
			}
		}
	}
}
