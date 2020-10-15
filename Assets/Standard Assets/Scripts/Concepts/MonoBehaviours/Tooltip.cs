using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using SpaceMayhem;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode]
public class Tooltip : MonoBehaviour, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}
	public RectTransform rectTrs;
	public RectTransform canvasRectTrs;
	public TMP_Text text;

	void Awake ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		rectTrs.SetParent(canvasRectTrs);
	}

	void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (rectTrs == null)
				rectTrs = GetComponent<RectTransform>();
			if (text == null)
				text = rectTrs.GetChild(0).GetComponentInChildren<TMP_Text>();
			return;
		}
#endif
		GameManager.updatables = GameManager.updatables.Add(this);
	}

	public void DoUpdate ()
	{
		rectTrs.anchoredPosition = InputManager.MousePosition;
	}

	void OnDisable ()
	{
		GameManager.updatables = GameManager.updatables.Remove(this);
	}
}
