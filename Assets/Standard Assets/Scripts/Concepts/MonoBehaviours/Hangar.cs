using UnityEngine;
using System.Collections.Generic;
using Extensions;
using TMPro;
using System;
using Random = UnityEngine.Random;

namespace SpaceMayhem
{
	public class Hangar : SaveAndLoadObject, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public LayerMask whatIsShipPart;
		public Transform hitboxTrsPrefab;
		public float distanceBetweenChecks = 3;
		public Player playerPrefab;
		public TMP_Text moneyText;
		public TemporaryActiveText noThrusterText;
		public TemporaryActiveText hasGapText;
		Transform hitboxTrs;
		Rect shipRect;
		Vector2 grabOffset;
		Vector2 grabPos;
		ShipPart[] shipParts = new ShipPart[0];
		List<Vector2> pointsToCheck = new List<Vector2>();
		List<Vector2> checkedPoints = new List<Vector2>();
		bool previousLeftClickInput;
		bool previousRightClickInput;
		public ParticleSystem thrusterParitcleSystem;
		[SaveAndLoadValue(false)]
		public PlayerEntry[] playerEntries = new PlayerEntry[0];
		public ShipPart[] shipPartPrefabs = new ShipPart[0];

		void Start ()
		{
			moneyText.text = "$" + Player.gold;
			GameObject hitboxGo = GameObject.Find("Hitbox(Clone)");
			if (hitboxGo != null)
				Destroy(hitboxGo);
			GameManager.updatables = GameManager.updatables.Add(this);
			GameManager.initialized = true;
		}

		void OnDestroy ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
		
		public void DoUpdate ()
		{
			bool leftClickInput = InputManager.LeftClickInput;
			bool rightClickInput = InputManager.RightClickInput;
			if (leftClickInput && !previousLeftClickInput)
			{
				Collider2D[] draggingColliders = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(InputManager.MousePosition), whatIsShipPart);
				if (draggingColliders.Length > 0)
				{
					int frontRendererId = 0;
					for (int i = 0; i < draggingColliders.Length; i ++)
					{
						if (draggingColliders[i].GetComponent<SpriteRenderer>().sortingOrder > draggingColliders[frontRendererId].GetComponent<SpriteRenderer>().sortingOrder)
							frontRendererId = i;
					}
					ShipPart.selected = draggingColliders[frontRendererId].GetComponent<ShipPart>();
					grabOffset = ShipPart.selected.trs.position - GameManager.GetSingleton<CameraScript>().camera.ScreenToWorldPoint(InputManager.MousePosition);
					while (ShipPart.selected.spriteRenderer.sortingOrder == 0)
						ShipPart.selected.spriteRenderer.sortingOrder = 1000 + Random.Range(0, 999);
				}
			}
			else if (leftClickInput && ShipPart.selected != null)
			{
				ShipPart.selected.transform.position = (Vector2) GameManager.GetSingleton<CameraScript>().camera.ScreenToWorldPoint(InputManager.MousePosition) + grabOffset;
			}
			else if (!leftClickInput && previousLeftClickInput)
			{
				ShipPart.selected = null;
			}
			if (rightClickInput && !previousRightClickInput)
			{
				Collider2D[] draggingColliders = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(InputManager.MousePosition), whatIsShipPart);
				if (draggingColliders.Length > 0)
				{
					int frontRendererId = 0;
					for (int i = 0; i < draggingColliders.Length; i ++)
					{
						if (draggingColliders[i].GetComponent<SpriteRenderer>().sortingOrder > draggingColliders[frontRendererId].GetComponent<SpriteRenderer>().sortingOrder)
							frontRendererId = i;
					}
					ShipPart.selected = draggingColliders[frontRendererId].GetComponent<ShipPart>();
					grabPos = (Vector2) Camera.main.ScreenToWorldPoint(InputManager.MousePosition);
					while (ShipPart.selected.spriteRenderer.sortingOrder == 0)
						ShipPart.selected.spriteRenderer.sortingOrder = 1000 + Random.Range(0, 999);
				}
			}
			else if (rightClickInput && ShipPart.selected != null)
			{
				float snapAngle = 11.25f;
				float angToCorrectFacing = Vector2.Angle(ShipPart.selected.transform.up, (Vector2) Camera.main.ScreenToWorldPoint(InputManager.MousePosition) - grabPos);
				if (Input.GetKey(KeyCode.LeftShift))
					angToCorrectFacing = Mathf.Round(Vector2.Angle(ShipPart.selected.transform.up, (Vector2) Camera.main.ScreenToWorldPoint(InputManager.MousePosition) - grabPos) / snapAngle) * snapAngle;
				Vector3 cross = Vector3.Cross(ShipPart.selected.transform.up, (Vector2) Camera.main.ScreenToWorldPoint(InputManager.MousePosition) - grabPos);
				if (Vector2.Distance(Camera.main.ScreenToWorldPoint(InputManager.MousePosition), grabPos) > 0)
				{
					ShipPart.selected.transform.RotateAround(grabPos, Vector3.forward, angToCorrectFacing * Mathf.Sign(cross.z));
					if (Input.GetKey(KeyCode.LeftShift))
					{
						ShipPart.selected.transform.eulerAngles = Vector3.forward * Mathf.Round(ShipPart.selected.transform.eulerAngles.z / snapAngle) * snapAngle;
					}
				}
			}
			else if (!rightClickInput && previousRightClickInput)
				ShipPart.selected = null;
			if (ShipPart.selected != null)
			{
				for (int i = 0; i <= 9; i ++)
					if (Input.GetKeyDown("" + i))
						ShipPart.selected.spriteRenderer.sortingOrder = i * 1000 + Random.Range(0, 999);
				if (Input.GetKeyDown(KeyCode.X))
					ShipPart.selected.transform.localScale = new Vector2(ShipPart.selected.transform.localScale.x * -1, ShipPart.selected.transform.localScale.y);
				else if (Input.GetKeyDown(KeyCode.Y))
					ShipPart.selected.transform.localScale = new Vector2(ShipPart.selected.transform.localScale.x, ShipPart.selected.transform.localScale.y * -1);
				else if (Input.GetKeyDown(KeyCode.S))
				{
					Player.gold += ShipPart.selected.cost / 1;
					moneyText.text = "$" + Player.gold;
					Destroy(ShipPart.selected.gameObject);
				}
			}
			previousLeftClickInput = leftClickInput;
			previousRightClickInput = rightClickInput;
		}

		public void BuyPart (ShipPart part)
		{
			if (Player.gold >= part.cost)
			{
				Player.gold -= part.cost;
				moneyText.text = "$" + Player.gold;
				ShipPart sp = (ShipPart) Instantiate(part);
				sp.gameObject.SetActive(true);
			}
		}

		public void FinishBuilding ()
		{
			bool hasThurster = false;
			ShipPart[] shipParts = FindObjectsOfType<ShipPart>();
			foreach (ShipPart part in shipParts)
			{
				if (part.speed > 0)
				{
					hasThurster = true;
					break;
				}
			}
			if (!hasThurster)
			{
				hasGapText.Stop ();
				noThrusterText.Do ();
				return;
			}
			SetHitbox ();
			bool hasGaps = CheckForGaps ();
			if (hasGaps)
			{
				noThrusterText.Stop ();
				hasGapText.Do ();
				return;
			}
			MakeShip ();
		}

		void MakeShip ()
		{
			if (GameManager.GetSingleton<Player>() == null)
				Instantiate(playerPrefab);
			GameManager.GetSingleton<Player>().trs.DetachChildren();
			GameManager.GetSingleton<Player>().trs.position = hitboxTrs.position;
			GameManager.GetSingleton<Player>().weight = 0;
			List<ShipPart> thrusters = new List<ShipPart>();
			foreach (ShipPart part in shipParts)
			{
				part.trs.SetParent(GameManager.GetSingleton<Player>().trs);
				if (part.speed > 0)
					thrusters.Add(part);
				GameManager.GetSingleton<Player>().weight += part.weight;
			}
			GameManager.GetSingleton<Player>().shipParts = shipParts;
			GameManager.GetSingleton<Player>().thrusters = thrusters.ToArray();
			GameManager.GetSingleton<Player>().weapons = GameManager.GetSingleton<Player>().GetComponentsInChildren<Weapon>();
			hitboxTrs.SetParent(GameManager.GetSingleton<Player>().trs);
			GameManager.HasShip = true;
			// LevelSerializer.SaveObjectTreeToFile("Player", GameManager.GetSingleton<Player>().gameObject);

			GameManager.GetSingleton<GameManager>().LoadScene ("Level Map");
		}

		void SetHitbox ()
		{
			shipParts = FindObjectsOfType<ShipPart>();
			Rect[] partRects = new Rect[shipParts.Length];
			for (int i = 0; i < shipParts.Length; i ++)
				partRects[i] = shipParts[i].polygonCollider.bounds.ToRect();
			shipRect = RectExtensions.Combine(partRects);
			GameObject hitboxGo = GameObject.Find("Hitbox(Clone)");
			if (hitboxGo == null)
				hitboxTrs = GameManager.GetSingleton<ObjectPool>().SpawnComponent(hitboxTrsPrefab, shipRect.center, Quaternion.identity);
			else
			{
				hitboxTrs = hitboxGo.GetComponent<Transform>();
				hitboxTrs.position = shipRect.center;
			}
		}

		bool CheckForGaps ()
		{
			List<Vector2> filledPoints = new List<Vector2>();
			for (float x = shipRect.xMin; x <= shipRect.xMax; x += distanceBetweenChecks)
			{
				for (float y = shipRect.yMin; y <= shipRect.yMax; y += distanceBetweenChecks)
				{
					if (Physics2D.OverlapPoint(new Vector2(x, y), whatIsShipPart) != null)
						filledPoints.Add(new Vector2(x, y));
				}
			}
			Vector2 currentPoint = filledPoints[0];
			pointsToCheck.Clear();
			pointsToCheck.Add(filledPoints[0]);
			do
			{
				currentPoint = pointsToCheck[0];
				if (filledPoints.Contains(currentPoint))
				{
					checkedPoints.Add(currentPoint);
					AddAdjacentPointsToCheck (currentPoint);
				}
				pointsToCheck.RemoveAt(0);
			} while (pointsToCheck.Count > 0);
			return checkedPoints.Count != filledPoints.Count;
		}

		void AddAdjacentPointsToCheck (Vector2 point)
		{
			if (!checkedPoints.Contains(point + new Vector2(0, distanceBetweenChecks)) && !pointsToCheck.Contains(point + new Vector2(0, distanceBetweenChecks)))
				pointsToCheck.Add(point + new Vector2(0, distanceBetweenChecks));
			if (!checkedPoints.Contains(point + new Vector2(distanceBetweenChecks, 0)) && !pointsToCheck.Contains(point + new Vector2(distanceBetweenChecks, 0)))
				pointsToCheck.Add(point + new Vector2(distanceBetweenChecks, 0));
			if (!checkedPoints.Contains(point + new Vector2(0, -distanceBetweenChecks)) && !pointsToCheck.Contains(point + new Vector2(0, -distanceBetweenChecks)))
				pointsToCheck.Add(point + new Vector2(0, -distanceBetweenChecks));
			if (!checkedPoints.Contains(point + new Vector2(-distanceBetweenChecks, 0)) && !pointsToCheck.Contains(point + new Vector2(-distanceBetweenChecks, 0)))
				pointsToCheck.Add(point + new Vector2(-distanceBetweenChecks, 0));
			if (!checkedPoints.Contains(point + new Vector2(distanceBetweenChecks, distanceBetweenChecks)) && !pointsToCheck.Contains(point + new Vector2(distanceBetweenChecks, distanceBetweenChecks)))
				pointsToCheck.Add(point + new Vector2(distanceBetweenChecks, distanceBetweenChecks));
			if (!checkedPoints.Contains(point + new Vector2(distanceBetweenChecks, -distanceBetweenChecks)) && !pointsToCheck.Contains(point + new Vector2(distanceBetweenChecks, -distanceBetweenChecks)))
				pointsToCheck.Add(point + new Vector2(distanceBetweenChecks, -distanceBetweenChecks));
			if (!checkedPoints.Contains(point + new Vector2(-distanceBetweenChecks, -distanceBetweenChecks)) && !pointsToCheck.Contains(point + new Vector2(-distanceBetweenChecks, -distanceBetweenChecks)))
				pointsToCheck.Add(point + new Vector2(-distanceBetweenChecks, -distanceBetweenChecks));
			if (!checkedPoints.Contains(point + new Vector2(-distanceBetweenChecks, distanceBetweenChecks)) && !pointsToCheck.Contains(point + new Vector2(-distanceBetweenChecks, distanceBetweenChecks)))
				pointsToCheck.Add(point + new Vector2(-distanceBetweenChecks, distanceBetweenChecks));
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
				return new PlayerEntry();
			}

			public Player MakeInstance ()
			{
				Player player = (Player) GameManager.Clone(GameManager.GetSingleton<Hangar>().playerPrefab);
				for (int i = 0; i < shipPartEntries.Length; i ++)
				{
					ShipPartEntry shipPartEntry = shipPartEntries[i];
					ShipPart shipPart = shipPartEntry.MakeInstance ();
					shipPart.trs.SetParent(player.trs);
				}
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
					for (int i = 0; i < GameManager.GetSingleton<Hangar>().shipPartPrefabs.Length; i ++)
					{
						ShipPart shipPartPrefab = GameManager.GetSingleton<Hangar>().shipPartPrefabs[i];
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
					ShipPart shipPart = (ShipPart) GameManager.Clone(GameManager.GetSingleton<Hangar>().shipPartPrefabs[partIndex]);
					shipPart.trs.position = position;
					shipPart.trs.eulerAngles = Vector3.forward * rotation;
					shipPart.spriteRenderer.sortingOrder = sortingOrder;
					return shipPart;
				}
			}
		}
	}
}