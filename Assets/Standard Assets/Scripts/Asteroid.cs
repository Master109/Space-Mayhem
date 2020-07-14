using UnityEngine;
using System.Collections;
using Extensions;

public class Asteroid : Spawnable
{
	public FloatRange alphaRange;
	public float speed;
	public SpriteRenderer spriteRenderer;
	public Rigidbody2D rigid;

	void Start ()
	{
		spriteRenderer.color = spriteRenderer.color.SetAlpha(Random.Range(alphaRange.min, alphaRange.max));
		rigid.velocity = Vector3.left * spriteRenderer.color.a * speed;
	}
}
