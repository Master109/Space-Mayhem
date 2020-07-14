using UnityEngine;
using System.Collections;
using Extensions;
using UnityEngine.Serialization;

namespace SpaceshipGame
{
	public class Weapon : MonoBehaviour, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Transform trs;
		public float shootRate;
		public float shootTimer;
		public bool aim;
		public float shootAngle;
		public float shootAnglesChange;
		// public float shootAnglesMin;
		// public float shootAnglesMax;
		public FloatRange shootAngleRange;
		public Transform shootSpawnTrs;
		public Bullet bulletPrefab;
		public AudioClip shootSound;
		public int bulletSpeed;
		public Transform muzzleFlashTrs;
		public Spawnable muzzleFlash;
		Vector2 toPlayer;
		int shootAnglesChangesMultiplier = 1;

		void OnEnable ()
		{
			// if (Random.value > .5f)
			// 	shootAnglesChangesMultiplier *= -1;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public void DoUpdate ()
		{
			toPlayer = GameManager.GetSingleton<Player>().trs.position - trs.position;
			shootTimer += Time.deltaTime;
			if (shootAngle <= shootAngleRange.min || shootAngle >= shootAngleRange.max)
				shootAnglesChangesMultiplier *= -1;
			shootAngle += shootAnglesChange * shootAnglesChangesMultiplier * Time.deltaTime;
			if (aim)
			{
				trs.rotation = Quaternion.LookRotation(Vector3.forward, toPlayer);
				trs.eulerAngles += Vector3.forward * shootAngle;
			}
			else
				trs.localEulerAngles = Vector3.forward * shootAngle;
		}

		public void Shoot ()
		{
			if (shootTimer > shootRate)
			{
				if (muzzleFlash != null)
					GameManager.GetSingleton<ObjectPool>().SpawnComponent<Spawnable>(muzzleFlash.prefabIndex, muzzleFlashTrs.position, muzzleFlashTrs.rotation);
				Bullet bullet = GameManager.GetSingleton<ObjectPool>().SpawnComponent<Bullet>(bulletPrefab.prefabIndex, shootSpawnTrs.position, shootSpawnTrs.rotation);
				shootTimer = 0;
				if (shootSound != null)
					AudioManager.PlaySoundEffect (GameManager.GetSingleton<AudioManager>().soundEffectPrefab, new SoundEffect.Settings(shootSound), trs.position, trs.rotation, trs);
			}
		}

		void OnDisable ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
	}
}