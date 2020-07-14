using UnityEngine;
using System.Collections;

namespace SpaceshipGame
{
	public class MapBounds : MonoBehaviour
	{
		void OnTriggerEnter2D (Collider2D other)
		{
			Ship ship = other.transform.root.GetComponent<Ship>();
			if (ship != null)
				ship.canShoot = true;
		}

		void OnTriggerExit2D (Collider2D other)
		{
			Destroy(other.transform.root.gameObject);
		}
	}
}