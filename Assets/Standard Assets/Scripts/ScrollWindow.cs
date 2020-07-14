using UnityEngine;
using System.Collections;

public class ScrollWindow : MonoBehaviour
{
	public Vector3 scrollDirection;
	public float scrollSensitivity;
	Vector3 initPos;

	void Start ()
	{
		scrollSensitivity *= transform.root.lossyScale.x;
		initPos = transform.position;
	}

	public void SetScrollPosition (float scrollPos)
	{
		transform.position = initPos + (scrollDirection.normalized * scrollPos * scrollSensitivity);
	}
}