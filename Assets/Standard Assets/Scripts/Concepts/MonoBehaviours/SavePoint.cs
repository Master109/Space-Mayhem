using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceshipGame
{
	public class SavePoint : MonoBehaviour
	{
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
		public Transform trs;
	}
}