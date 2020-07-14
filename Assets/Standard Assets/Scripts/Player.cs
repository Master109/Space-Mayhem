using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Extensions;

namespace SpaceshipGame
{
	[DisallowMultipleComponent]
	public class Player : Ship
	{
		public static float score;
		public const int INIT_GOLD = 1000;
		public static int gold = INIT_GOLD;
		public float grazeHeal;
		public float grazeToScore;
		public ShipPart[] shipParts = new ShipPart[0];
		public ShipPart[] thrusters = new ShipPart[0];
		public float weight;
		bool isDead;
		Explosion explosion;

		public override void Awake ()
		{
			base.Awake ();
			DontDestroyOnLoad (gameObject);
		}

		void Start ()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}
		
		public override void DoUpdate ()
		{
			if (isDead)
				return;
			Vector2 input = InputManager.PlayerMovementInput;
			Vector2 velocity = new Vector2();
			if (input.sqrMagnitude > 0)
			{
				foreach (ShipPart part in thrusters)
				{
					Vector2 toPart = part.trs.position - trs.position;
					Vector2 partFacing = part.trs.up.Multiply(part.trs.lossyScale);
					if (Vector2.Angle(input, partFacing) < part.effectAngle)
					{
						velocity += partFacing * part.speed;
						part.particleSystem.Emit (1);
					}
				}
				if (weight * weight < velocity.sqrMagnitude)
					velocity -= velocity.normalized * weight;
				else
					velocity = Vector2.zero;
			}
			rigid.velocity = velocity;
			if (Input.GetButton("Fire"))
			{
				foreach (Weapon w in weapons)
					w.Shoot();
			}
		}

		void OnSceneLoaded (Scene scene = new Scene(), LoadSceneMode loadMod = LoadSceneMode.Single)
		{
			if (GameManager.GetSingleton<Level>() != null)
			{
				score = 0;
				isDead = false;
				GameManager.GetSingleton<Level>().scoreText.text = "Score: " + (int) Player.score;
				trs.position = GameManager.GetSingleton<Level>().playerSpawnPoint.position;
				enabled = true;
				foreach (ShipPart part in shipParts)
					part.polygonCollider.enabled = false;
				foreach (Weapon weapon in weapons)
				{
					weapon.shootAngle = weapon.trs.eulerAngles.z;
					weapon.enabled = true;
				}
				Awake ();
			}
			else
			{
				rigid.velocity = Vector2.zero;
				trs.position = Vector2.zero;
				enabled = false;
				foreach (ShipPart part in shipParts)
				{
					part.polygonCollider.enabled = true;
					part.spriteRenderer.enabled = true;
				}
				foreach (Weapon weapon in weapons)
					weapon.enabled = false;
			}
		}

		void OnTriggerStay2D (Collider2D other)
		{
			if (other.isTrigger)
				return;
			float grazeHealMultiplier = 1;
			Transform otherTrs = other.GetComponent<Transform>();
			foreach (ShipPart part in GetComponentsInChildren<ShipPart>())
			{
				if (part.grazeHeal > 0)
				{
					Vector2 toBullet = otherTrs.position - trs.position;
					Vector2 toPart = part.trs.position - trs.position;
					if (Vector2.Angle(toBullet, toPart) < part.effectAngle)
					{
						grazeHealMultiplier += part.grazeHeal * .01f;
					}
				}
			}
			TakeDamage (-grazeHeal * grazeHealMultiplier * Time.deltaTime * (1 / Vector2.Distance(otherTrs.position, trs.position)));
			score += grazeToScore * Time.deltaTime * (1 / Vector2.Distance(otherTrs.position, trs.position));
			GameManager.GetSingleton<Level>().scoreText.text = "Score: " + (int) Player.score;
		}

		public override void TakeDamage (float amount)
		{
			if (isDead)
				return;
			base.TakeDamage (amount);
			GameManager.GetSingleton<Level>().healthText.text = "Health: " + (int) hp;
		}

		public override void Death ()
		{
			isDead = true;
			gameObject.SetActive(false);
			GameManager.GetSingleton<Level>().scoreText.text = "Score: " + (int) score;
			explosion = GameManager.GetSingleton<ObjectPool>().SpawnComponent<Explosion>(explosionPrefab, trs.position);
			Destroy(explosion.gameObject, explosion.anim.clip.length);
			StartCoroutine (DeathRoutine ());
		}

		IEnumerator DeathRoutine ()
		{
			yield return new WaitUntil(() => (explosion == null));
			if (GameManager.GetSingleton<Survival>() != null)
				GameManager.GetSingleton<Survival>().WinLevel ();
			else
				GameManager.GetSingleton<GameManager>().LoadScene ("Hangar");
		}

		void OnDestroy ()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}
	}
}