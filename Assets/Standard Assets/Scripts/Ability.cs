using UnityEngine;
using System.Collections;

public class Ability : MonoBehaviour
{
	public float cooldown;
	public AnimationClip animation;
	float cooldownTimer;
	public string hotkey;

	// Use this for initialization
	void Start ()
	{
		cooldownTimer = cooldown;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKey(hotkey) && cooldownTimer < 0)
			Use ();
	}

	void Use ()
	{
		cooldownTimer = cooldown;
		if (animation != null)
			GetComponent<Animation>().Play(animation.name);
	}
}
