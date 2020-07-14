using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;

	public void SetNextSprite (Sprite sprite)
	{
		spriteRenderer.sprite = sprite;
	}
}
