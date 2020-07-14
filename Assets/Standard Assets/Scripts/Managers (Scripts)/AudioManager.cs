using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceshipGame
{
	public class AudioManager : MonoBehaviour, ISaveableAndLoadable
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
		public static List<SoundEffect> soundEffects = new List<SoundEffect>();
		[SaveAndLoadValue(false)]
		public float volume;
		[SaveAndLoadValue(false)]
		public bool mute;
		public SoundEffect soundEffectPrefab;
		public AudioSource[] musics;
		public GameObject audioOnGo;
		public GameObject audioOffGo;
		public int uniqueId;
		AudioSource currentMusic;
		public float musicDelay;
		
		public void Awake ()
		{
			soundEffects.Clear();
			UpdateAudioListener ();
			if (AudioListener.volume == 0)
			{
				audioOnGo.SetActive(true);
				audioOffGo.SetActive(false);
			}
		}

		void ChangeMusic ()
		{
			List<AudioSource> otherMusics = new List<AudioSource>();
			otherMusics.AddRange(musics);
			otherMusics.Remove(currentMusic);
			currentMusic = otherMusics[Random.Range(0, otherMusics.Count)];
			currentMusic.Play();
			EventManager.events.Add(new EventManager.Event(ChangeMusic, Time.time + currentMusic.time + musicDelay));
		}

		void UpdateAudioListener ()
		{
			if (mute)
				AudioListener.volume = 0;
			else
				AudioListener.volume = volume;
		}

		public void SetVolume (float volume)
		{
			if (GameManager.GetSingleton<AudioManager>() != this)
			{
				GameManager.GetSingleton<AudioManager>().SetVolume (volume);
				return;
			}
			this.volume = volume;
			UpdateAudioListener ();
		}

		public void SetMute (bool mute)
		{
			if (GameManager.GetSingleton<AudioManager>() != this)
			{
				GameManager.GetSingleton<AudioManager>().SetMute (mute);
				return;
			}
			this.mute = mute;
			UpdateAudioListener ();

		}

		public void ToggleMute ()
		{
			SetMute (!mute);
		}
		
		public static SoundEffect PlaySoundEffect (SoundEffect soundEffectPrefab, SoundEffect.Settings settings, Vector2 position = default(Vector2), Quaternion rotation = default(Quaternion), Transform parent = null)
		{
			SoundEffect output = GameManager.GetSingleton<ObjectPool>().SpawnComponent<SoundEffect>(soundEffectPrefab.prefabIndex, position, rotation, parent);
			output.audioSource.clip = settings.clip;
			output.audioSource.volume = settings.volume;
			output.audioSource.pitch = settings.pitch;
			output.audioSource.Play();
			GameManager.GetSingleton<ObjectPool>().DelayDespawn (output.prefabIndex, output.gameObject, output.trs, settings.clip.length);
			soundEffects.Add(output);
			return output;
		}
	}
}