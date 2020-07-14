#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define UNITY_4_3_AND_LATER
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;

//-------------------------------------------------------------------------
/// <summary>
/// Editor window for the AlphaMeshCollider preference values.
/// </summary>
public class EditorScriptAlphaMeshColliderPreferencesWindow : EditorWindow {

	GUIContent mDefaultColliderDirectoryLabel = new GUIContent("Collider Directory", "Set the default output directory for generated collider mesh files.");
	GUIContent mDefaultLiveUpdateLabel = new GUIContent("Live Update", "Recalculate the collider mesh when changing parameters in the inspector.");
    GUIContent mDefaultAlphaOpaqueThresholdLabel = new GUIContent("Alpha Opaque Threshold", "Default alpha threshold value in [0..1] above which a pixel is treated as opaque and thus contributes to the outline shape.");
	GUIContent mDefaultColliderPointCountLabel = new GUIContent("Outline Vertex Count", "Default point count of the collider shape.");
	GUIContent mColliderPointCountSliderMaxValueLabel = new GUIContent("Vertex Count Slider Max", "Maximum value of the outline vertex count slider.");
	GUIContent mDefaultColliderThicknessLabel = new GUIContent("Z-Thickness", "Default thickness of a collider.");
#if UNITY_4_3_AND_LATER
	GUIContent mDefaultTargetColliderTypeLabel = new GUIContent("Collider Type", "Default output collider type - MeshCollider or PolygonCollider2D.");
#endif
    GUIContent mDefaultConvexLabel = new GUIContent("Force Convex", " Default value whether to create a convex collider or allow it to be concave.");
    GUIContent mDefaultFlipNormalsLabel = new GUIContent("Flip Normals", " Default value whether to flip the normals inside-out.");

	//-------------------------------------------------------------------------
	[MenuItem ("Component/2D ColliderGen/Collider Preferences", false, 1036)]
    [MenuItem ("Window/2D ColliderGen/Collider Preferences", false, 1036)]
	static void ColliderPreferences() {
		
		// Get existing open window or if none, make a new one:
		EditorScriptAlphaMeshColliderPreferencesWindow window = EditorWindow.GetWindow<EditorScriptAlphaMeshColliderPreferencesWindow>();
		window.title = "Default Values";
    }
	
	//-------------------------------------------------------------------------
	void OnGUI()
	{
		//EditorGUIUtility.LookLikeControls(150.0f);
		
		AlphaMeshColliderPreferences.Instance.DefaultColliderDirectory = EditorGUILayout.TextField(mDefaultColliderDirectoryLabel, AlphaMeshColliderPreferences.Instance.DefaultColliderDirectory);
		AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate = EditorGUILayout.Toggle(mDefaultLiveUpdateLabel, AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate);
        AlphaMeshColliderPreferences.Instance.DefaultAlphaOpaqueThreshold = EditorGUILayout.FloatField(mDefaultAlphaOpaqueThresholdLabel, AlphaMeshColliderPreferences.Instance.DefaultAlphaOpaqueThreshold);
		AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount = EditorGUILayout.IntField(mDefaultColliderPointCountLabel, AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount);
		AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue = EditorGUILayout.IntField(mColliderPointCountSliderMaxValueLabel, AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue);
		AlphaMeshColliderPreferences.Instance.DefaultAbsoluteColliderThickness = EditorGUILayout.FloatField(mDefaultColliderThicknessLabel, AlphaMeshColliderPreferences.Instance.DefaultAbsoluteColliderThickness);	
#if UNITY_4_3_AND_LATER
		AlphaMeshColliderPreferences.Instance.DefaultTargetColliderType = (AlphaMeshCollider.TargetColliderType) EditorGUILayout.EnumPopup(mDefaultTargetColliderTypeLabel, AlphaMeshColliderPreferences.Instance.DefaultTargetColliderType);
#endif
        AlphaMeshColliderPreferences.Instance.DefaultConvex = EditorGUILayout.Toggle(mDefaultConvexLabel, AlphaMeshColliderPreferences.Instance.DefaultConvex);
        AlphaMeshColliderPreferences.Instance.DefaultFlipNormals = EditorGUILayout.Toggle(mDefaultFlipNormalsLabel, AlphaMeshColliderPreferences.Instance.DefaultFlipNormals);	
	}
}
