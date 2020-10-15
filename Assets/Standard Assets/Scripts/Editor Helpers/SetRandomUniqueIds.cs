#if UNITY_EDITOR
using UnityEngine;
using SpaceMayhem;

public class SetRandomUniqueIds : EditorScript
{
	public SaveAndLoadObject[] saveAndLoadObjects = new SaveAndLoadObject[0];

	public override void DoEditorUpdate ()
	{
		foreach (SaveAndLoadObject saveAndLoadObject in saveAndLoadObjects)
			saveAndLoadObject.uniqueId = Random.Range(int.MinValue, int.MaxValue);
		saveAndLoadObjects = new SaveAndLoadObject[0];
	}
}
#else
using UnityEngine;

public class SetRandomUniqueIds : MonoBehaviour
{
}
#endif