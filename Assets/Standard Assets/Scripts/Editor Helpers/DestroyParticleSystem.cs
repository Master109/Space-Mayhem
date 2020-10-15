using UnityEngine;

namespace SpaceMayhem
{
	public class DestroyParticleSystem : MonoBehaviour
	{
		void Start ()
		{
			if (Application.isPlaying)
			{
				Destroy(GetComponent<ParticleSystem>());
				Destroy(this);
			}
		}
	}
}