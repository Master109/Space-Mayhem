using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.UI;
using PlayerIOClient;

namespace SpaceMayhem
{
	//[ExecuteAlways]
	public class ArchivesManager : SingletonMonoBehaviour<ArchivesManager>, ISaveableAndLoadable
	{
		public const string EMPTY_ACCOUNT_INDICATOR = "␀";
		public const int MAX_ACCOUNTS = 5;
		public const string VALUE_SEPARATOR = "⧫";
		[SaveAndLoadValue(true)]
		public static string[] localAccountNames = new string[0];
		[SaveAndLoadValue(true)]
		public static string[] localAccountPasswords = new string[0];
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
		public int uniqueId;
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
		public Account[] localAccountsData = new Account[0];
		public Account newAccount;
		public static Account player1Account;
		public static Account player2Account;
		public static Account activeAccount;
		// public MenuOption addAccountOption;
		// public MenuOption[] accountOptions = new MenuOption[0];
		// public MenuOption[] viewInfoMenuOptions = new MenuOption[0];
		public static string ActivePlayerUsername
		{
			get
			{
				if (activeAccount == null)
					return EMPTY_ACCOUNT_INDICATOR;
				else
					return activeAccount.username;
			}
		}
		public GameObject deleteAccountScreen;
		public Text deleteAccountText;
		public static int indexOfCurrentAccountToDelete;
		public static Account currentAccountToDelete;
		public GameObject accountInfoScreen;
		public Text accountInfoTitleText;
		public Text accountInfoContentText;
		public static Account currentAccountToViewInfo;
		public Scrollbar accountInfoScrollbar;
		// public static MenuOption player1AccountAssigner;
		// public static MenuOption player2AccountAssigner;
		// public Transform trs;
		public static Account[] Accounts
		{
			get
			{
				List<Account> output = new List<Account>();
				output.Add(ArchivesManager.Instance.newAccount);
				// output.Add(new Account());
				// foreach (AccountSelectMenuOption accountSelectMenuOption in GameManager.GetSingleton<AccountSelectMenu>().menuOptions)
					// output.Add(accountSelectMenuOption.account);
				return output.ToArray();
			}
		}
		public static Account CurrentlyPlaying
		{
			get
			{
				if (currentAccountIndex == -1)
					return null;
				return Accounts[currentAccountIndex];
			}
		}
		public static int currentAccountIndex = -1;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (GameManager.GetSingleton<ArchivesManager>() != null && GameManager.GetSingleton<ArchivesManager>() != this)
			{
				UpdateMenus ();
				// GameManager.GetSingleton<ArchivesManager>().addAccountOption = addAccountOption;
				// GameManager.GetSingleton<ArchivesManager>().accountOptions = accountOptions;
				// GameManager.GetSingleton<ArchivesManager>().viewInfoMenuOptions = viewInfoMenuOptions;
				GameManager.GetSingleton<ArchivesManager>().deleteAccountScreen = deleteAccountScreen;
				GameManager.GetSingleton<ArchivesManager>().deleteAccountText = deleteAccountText;
				GameManager.GetSingleton<ArchivesManager>().accountInfoScreen = accountInfoScreen;
				GameManager.GetSingleton<ArchivesManager>().accountInfoTitleText = accountInfoTitleText;
				GameManager.GetSingleton<ArchivesManager>().accountInfoContentText = accountInfoContentText;
				GameManager.GetSingleton<ArchivesManager>().accountInfoScrollbar = accountInfoScrollbar;
				return;
			}
			base.Awake ();
			// trs.SetParent(null);
			// if (BuildManager.IsFirstStartup)
			// {
			// 	if (GameManager.GetSingleton<BuildManager>().clearDataOnFirstStartup)
			// 	{
			// 		SaveAndLoadManager.data.Clear();
			// 		if (GameManager.GetSingleton<SaveAndLoadManager>().usePlayerPrefs)
			// 			PlayerPrefs.DeleteAll();
			// 		else
			// 			File.Delete(GameManager.GetSingleton<SaveAndLoadManager>().saveFileFullPath);
			// 	}
			// 	else
			// 		GameManager.GetSingleton<SaveAndLoadManager>().Load ();
			// 	BuildManager.IsFirstStartup = false;
			// }
			// else
			// 	GameManager.GetSingleton<SaveAndLoadManager>().Load ();
			Connect ();
		}

		public virtual void Connect ()
		{
			GameManager.GetSingleton<NetworkManager>().Connect (OnAuthenticateSucess, OnAuthenticateFail);
		}

		public virtual void OnAuthenticateSucess (Client client)
		{
			Debug.Log("OnAuthenticateSucess");
			NetworkManager.client = client;
		}

		public virtual void OnAuthenticateFail (PlayerIOError error)
		{
			Debug.Log("OnAuthenticateFail: " + error.ToString());
			// Connect ();
		}

		public virtual void UpdateMenus ()
		{
			// if (accountOptions[0] == null)
			// 	return;
			// MenuOption accountOption;
			// for (int i = 0; i < MAX_ACCOUNTS; i ++)
			// {
			// 	if (LocalAccountNames.Length > i)
			// 	{
			// 		accountOption = accountOptions[i];
			// 		accountOption.enabled = true;
			// 		accountOption.textMesh.text = LocalAccountNames[i];
			// 		accountOption = viewInfoMenuOptions[i];
			// 		accountOption.enabled = true;
			// 		accountOption.textMesh.text = LocalAccountNames[i];
			// 	}
			// 	else
			// 	{
			// 		accountOption = accountOptions[i];
			// 		accountOption.enabled = false;
			// 		accountOption.textMesh.text = "Account " + (i + 1);
			// 		accountOption = viewInfoMenuOptions[i];
			// 		accountOption.enabled = false;
			// 		accountOption.textMesh.text = "Account " + (i + 1);
			// 	}
			// 	accountOptions[i].trs.GetChild(0).GetComponentInChildren<Menu>().options[0].enabled = true;
			// 	accountOptions[i].trs.GetChild(0).GetComponentInChildren<Menu>().options[1].enabled = true;
			// }
			// if (player1Account != null)
			// 	accountOptions[GameManager.GetSingleton<ArchivesManager>().localAccountsData.IndexOf(player1Account)].trs.GetChild(0).GetComponentInChildren<Menu>().options[0].enabled = false;
			// if (player2Account != null)
			// 	accountOptions[GameManager.GetSingleton<ArchivesManager>().localAccountsData.IndexOf(player2Account)].trs.GetChild(0).GetComponentInChildren<Menu>().options[1].enabled = false;
			// addAccountOption.enabled = LocalAccountNames.Length < MAX_ACCOUNTS;
		}
		
		public virtual void StartContinuousContinuousScrollAccountInfo (float velocity)
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().StartContinuousContinuousScrollAccountInfo (velocity);
				return;
			}
			StartCoroutine(ContinuousScrollAccountInfoRoutine (velocity));
		}

		public virtual IEnumerator ContinuousScrollAccountInfoRoutine (float velocity)
		{
			while (true)
			{
				ScrollAccountInfo (velocity);
				yield return new WaitForEndOfFrame();
			}
		}

		public virtual void ScrollAccountInfo (float velocity)
		{
			accountInfoScrollbar.value += velocity * Time.deltaTime;
		}

		public virtual void EndContinuousContinuousScrollAccountInfo ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().EndContinuousContinuousScrollAccountInfo ();
				return;
			}
			StopAllCoroutines();
		}

		public virtual void TryToSetNewAccountUsername ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().TryToSetNewAccountUsername ();
				return;
			}
			GameManager.GetSingleton<VirtualKeyboard>().DisableInput ();
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.go.SetActive(true);
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Loading...";
			string username = GameManager.GetSingleton<VirtualKeyboard>().outputToInputField.text;
			newAccount.username = username.Replace(" ", "");
			if (newAccount.username.Length == 0)
			{
				GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "The username must contain at least one non-space character";
				GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DoRoutine ());
				GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				return;
			}
			NetworkManager.client.BigDB.LoadMyPlayerObject(
				delegate (DatabaseObject dbObject)
				{
					if (dbObject.Count > 0)
					{
						GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "The username can't be used. It has already been registered online by someone else.";
						GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DoRoutine ());
						GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
						return;
					}
					else
					{
						GameManager.GetSingleton<NetworkManager>().notificationTextObject.go.SetActive(false);
						GameManager.GetSingleton<VirtualKeyboard>().trs.parent.gameObject.SetActive(false);
						GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
						NetworkManager.client.BigDB.LoadOrCreate("PlayerObjects", username, OnNewAccountDBObjectCreateSuccess, OnNewAccountDBObjectCreateFail);
					}
				},
				delegate (PlayerIOError error)
				{
					GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
					GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DoRoutine ());
					GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				}
			);
		}

		public virtual void OnNewAccountDBObjectCreateSuccess (DatabaseObject dbObject)
		{
			newAccount.username = dbObject.Key;
			GameManager.GetSingleton<VirtualKeyboard>().trs.parent.parent.GetChild(1).gameObject.SetActive(true);
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
		}

		public virtual void OnNewAccountDBObjectCreateFail (PlayerIOError error)
		{
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
			GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DoRoutine ());
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
		}

		public virtual void TryToSetNewAccountPassword ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().TryToSetNewAccountPassword ();
				return;
			}
			GameManager.GetSingleton<VirtualKeyboard>().DisableInput ();
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.go.SetActive(true);
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Loading...";
			newAccount.password = GameManager.GetSingleton<VirtualKeyboard>().outputToInputField.text;
			NetworkManager.client.BigDB.LoadMyPlayerObject(
				delegate (DatabaseObject dbObject)
				{
					if (dbObject.Count > 0)
					{
						GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "The username previously chosen can't be used. It has already been registered online by someone else.";
						GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DoRoutine ());
						GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
						return;
					}
					else
					{
						dbObject.Set("password", newAccount.password);
						dbObject.Save(true, false, OnNewAccountDBObjectSaveSuccess, OnNewAccountDBObjectSaveFail);
					}
				},
				delegate (PlayerIOError error)
				{
					GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
					GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DoRoutine ());
					GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				}
			);
		}

		public virtual void OnNewAccountDBObjectSaveSuccess ()
		{
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.go.SetActive(false);
			GameManager.GetSingleton<VirtualKeyboard>().trs.parent.gameObject.SetActive(false);
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
			localAccountsData[localAccountNames.Length].username = newAccount.username;
			localAccountsData[localAccountNames.Length].password = newAccount.password;
			foreach (Account Account in localAccountsData)
				Account.UpdateData ();
			localAccountNames = localAccountNames.Add(newAccount.username);
			UpdateMenus ();
		}

		public virtual void OnNewAccountDBObjectSaveFail (PlayerIOError error)
		{
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
			GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DoRoutine ());
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
		}

		public virtual void DeleteAccount ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().DeleteAccount ();
				return;
			}
			NetworkManager.client.BigDB.DeleteKeys("PlayerObjects", new string[1] { localAccountNames[indexOfCurrentAccountToDelete] }, OnDelteAccountDBObjectSuccess, OnDeleteAccountDBObjectFail);
		}

		public virtual void OnDelteAccountDBObjectSuccess ()
		{
			// foreach (string key in SaveAndLoadManager.data.Keys)
			// {
			// 	if (key.StartsWith(LocalAccountNames[indexOfCurrentAccountToDelete] + VALUE_SEPARATOR))
			// 		SaveAndLoadManager.data.Remove(key);
			// }
			localAccountsData[indexOfCurrentAccountToDelete].Reset ();
			localAccountNames = localAccountNames.RemoveAt(indexOfCurrentAccountToDelete);
			localAccountPasswords = localAccountPasswords.RemoveAt(indexOfCurrentAccountToDelete);
			foreach (Account Account in localAccountsData)
				Account.UpdateData ();
			if (activeAccount == localAccountsData[indexOfCurrentAccountToDelete])
				activeAccount = null;
			if (player1Account == localAccountsData[indexOfCurrentAccountToDelete])
				player1Account = null;
			else if (player2Account == localAccountsData[indexOfCurrentAccountToDelete])
				player2Account = null;
			GameManager.GetSingleton<SaveAndLoadManager>().SaveToCurrentAccount ();
			UpdateMenus ();
		}

		public virtual void OnDeleteAccountDBObjectFail (PlayerIOError error)
		{
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
			GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DoRoutine ());
		}

		public virtual void UpdateAccount (Account Account)
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().UpdateAccount (Account);
				return;
			}
		}
	}
}
