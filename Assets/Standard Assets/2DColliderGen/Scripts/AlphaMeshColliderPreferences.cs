#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define UNITY_4_3_AND_LATER
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//-------------------------------------------------------------------------
/// <summary>
/// Class to read and write AlphaMeshCollider relevant editor preference values.
/// </summary>
public class AlphaMeshColliderPreferences
{

#if UNITY_EDITOR
	//-------------------------------------------------------------------------
	/// <summary> The singleton instance. </summary>
	static AlphaMeshColliderPreferences mInstance = null;	
	public static AlphaMeshColliderPreferences Instance {
		get {
			if (mInstance == null) {
				mInstance = new AlphaMeshColliderPreferences();
				mInstance.ReadAllParams();
			}
			return mInstance;
		}
	}
	
	const string INITIAL_COLLIDER_PATH = "Assets/Colliders/Generated";
	//-------------------------------------------------------------------------	
	string mDefaultColliderDirectory = INITIAL_COLLIDER_PATH;
	bool mDefaultLiveUpdate;
    float mDefaultAlphaOpaqueThreshold;
	int mDefaultColliderPointCount;
	int mColliderPointCountSliderMaxValue;
	bool mDefaultConvex;
	float mDefaultAbsoluteColliderThickness;
#if UNITY_4_3_AND_LATER
	AlphaMeshCollider.TargetColliderType mDefaultTargetColliderType = AlphaMeshCollider.TargetColliderType.PolygonCollider2D;
#endif
    bool mDefaultFlipNormals;
	
	//-------------------------------------------------------------------------
	public string DefaultColliderDirectory {
		get {
			return mDefaultColliderDirectory;
		}
		set {
			if (mDefaultColliderDirectory != value) {
				string correctedPath = value;
				if (correctedPath.Length==0) {
					correctedPath = INITIAL_COLLIDER_PATH;
				}
				mDefaultColliderDirectory = correctedPath;
				EditorPrefs.SetString("AlphaMeshCollider_DefaultColliderDirectory", mDefaultColliderDirectory);
			}
		}
	}
	public bool DefaultLiveUpdate {
		get {
			return mDefaultLiveUpdate;
		}
		set {
			if (mDefaultLiveUpdate != value) {
				mDefaultLiveUpdate = value;
				EditorPrefs.SetBool("AlphaMeshCollider_DefaultLiveUpdate", mDefaultLiveUpdate);
			}
		}
	}
    public float DefaultAlphaOpaqueThreshold {
        get {
            return mDefaultAlphaOpaqueThreshold;
        }
        set {
            if (mDefaultAlphaOpaqueThreshold != value) {
                mDefaultAlphaOpaqueThreshold = value;
                EditorPrefs.SetFloat("AlphaMeshCollider_DefaultAlphaOpaqueThreshold", mDefaultAlphaOpaqueThreshold);
            }
        }
    }
	public int DefaultColliderPointCount {
		get {
			return mDefaultColliderPointCount;
		}
		set {
			if (mDefaultColliderPointCount != value) {
				mDefaultColliderPointCount = value;
				if (mDefaultColliderPointCount < 2)
					mDefaultColliderPointCount = 2;
				EditorPrefs.SetInt("AlphaMeshCollider_DefaultColliderPointCount", mDefaultColliderPointCount);
			}
		}
	}
	public int ColliderPointCountSliderMaxValue {
		get {
			return mColliderPointCountSliderMaxValue;
		}
		set {
			if (mColliderPointCountSliderMaxValue != value)  {
				mColliderPointCountSliderMaxValue = value;
				if (mColliderPointCountSliderMaxValue < 4)
					mColliderPointCountSliderMaxValue = 4;
				EditorPrefs.SetInt("AlphaMeshCollider_ColliderPointCountSliderMaxValue", mColliderPointCountSliderMaxValue);
			}
		}
	}
	public bool DefaultConvex {
		get {
			return mDefaultConvex;
		}
		set {
			if (mDefaultConvex != value) {
				mDefaultConvex = value;
				EditorPrefs.SetBool("AlphaMeshCollider_DefaultConvex", mDefaultConvex);
			}
		}
	}

    public bool DefaultFlipNormals {
        get {
            return mDefaultFlipNormals;
        }
        set {
            if (mDefaultFlipNormals != value) {
                mDefaultFlipNormals = value;
                EditorPrefs.SetBool("AlphaMeshCollider_DefaultFlipNormals", mDefaultFlipNormals);
            }
        }
    }
	
	public float DefaultAbsoluteColliderThickness {
		get {
			return mDefaultAbsoluteColliderThickness;
		}
		set {
			if (mDefaultAbsoluteColliderThickness != value) {
				mDefaultAbsoluteColliderThickness = value;
				EditorPrefs.SetFloat("AlphaMeshCollider_DefaultAbsoluteColliderThickness", mDefaultAbsoluteColliderThickness);
			}
		}
	}

#if UNITY_4_3_AND_LATER
	public AlphaMeshCollider.TargetColliderType DefaultTargetColliderType {
		get {
			return mDefaultTargetColliderType;
		}
		set {
			if (mDefaultTargetColliderType != value) {
				mDefaultTargetColliderType = value;
				EditorPrefs.SetInt("AlphaMeshCollider_DefaultTargetColliderType", (int)mDefaultTargetColliderType);
			}
		}
	}
#endif
	//-------------------------------------------------------------------------
	
	//-------------------------------------------------------------------------
	void ReadAllParams()
	{
		mDefaultColliderDirectory = EditorPrefs.GetString("AlphaMeshCollider_DefaultColliderDirectory", INITIAL_COLLIDER_PATH);
		mDefaultLiveUpdate = EditorPrefs.GetBool("AlphaMeshCollider_DefaultLiveUpdate", true);
        mDefaultAlphaOpaqueThreshold = EditorPrefs.GetFloat("AlphaMeshCollider_DefaultAlphaOpaqueThreshold", 0.1f);
		mDefaultColliderPointCount = EditorPrefs.GetInt("AlphaMeshCollider_DefaultColliderPointCount", 20);
		mColliderPointCountSliderMaxValue = EditorPrefs.GetInt("AlphaMeshCollider_ColliderPointCountSliderMaxValue", 100);
		mDefaultAbsoluteColliderThickness = EditorPrefs.GetFloat("AlphaMeshCollider_DefaultAbsoluteColliderThickness", 2.0f);
#if UNITY_4_3_AND_LATER
		mDefaultTargetColliderType = (AlphaMeshCollider.TargetColliderType) EditorPrefs.GetInt("AlphaMeshCollider_DefaultTargetColliderType", 1); // defaults to 1 == PolygonCollider2D
#endif
        mDefaultConvex = EditorPrefs.GetBool("AlphaMeshCollider_DefaultConvex", false);
        mDefaultFlipNormals = EditorPrefs.GetBool("AlphaMeshCollider_DefaultFlipNormals", false);
	}
	
	//-------------------------------------------------------------------------
	public void WriteAllParams()
	{
		EditorPrefs.SetString("AlphaMeshCollider_DefaultColliderDirectory", mDefaultColliderDirectory);
		EditorPrefs.SetBool("AlphaMeshCollider_DefaultLiveUpdate", mDefaultLiveUpdate);
        EditorPrefs.SetFloat("AlphaMeshCollider_DefaultAlphaOpaqueThreshold", mDefaultAlphaOpaqueThreshold);
		EditorPrefs.SetInt("AlphaMeshCollider_DefaultColliderPointCount", mDefaultColliderPointCount);
		EditorPrefs.SetInt("AlphaMeshCollider_ColliderPointCountSliderMaxValue", mColliderPointCountSliderMaxValue);
		
		EditorPrefs.SetFloat("AlphaMeshCollider_DefaultAbsoluteColliderThickness", mDefaultAbsoluteColliderThickness);
        EditorPrefs.SetBool("AlphaMeshCollider_DefaultConvex", mDefaultConvex);
        EditorPrefs.SetBool("AlphaMeshCollider_DefaultFlipNormals", mDefaultFlipNormals);
	}
#endif // UNITY_EDITOR
}

