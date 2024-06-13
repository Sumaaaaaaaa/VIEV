using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CommentObjectToolSetting))]
public class CommentObjectToolSettingEditor : Editor {
	private static CommentObjectToolSetting setting;
	private static string settingPath = "Assets/CommentObjectTool/Editor/Setting/CommentObjectToolSetting.asset";

	public delegate void OnSettingChanged();
	/// <summary>
	/// Occurs when on setting changed.
	/// </summary>
	public static event OnSettingChanged onSettingChanged;

//	void OnEnable() {
//		setting = (CommentObjectToolSetting)target;
//	}

	public static CommentObjectToolSetting GetSetting {
		get {
			if (setting == null) {
				setting = AssetDatabase.LoadAssetAtPath<CommentObjectToolSetting>(settingPath);
				if (setting == null) {
					Debug.Log("Setting null");
					setting = ScriptableObject.CreateInstance<CommentObjectToolSetting>();
					string assetPath = AssetDatabase.GenerateUniqueAssetPath(settingPath);
					AssetDatabase.CreateAsset(setting, assetPath);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				} 

				EditorUtility.SetDirty(setting);
			}
			return setting;
		}
	}

	public override void OnInspectorGUI () {
		EditorGUI.BeginChangeCheck();

		setting.saveCommentTextsInMultipleObject =
			EditorGUILayout.Toggle("Save data in multiple object",setting.saveCommentTextsInMultipleObject);

		setting.commentTextsFolder =
			EditorGUILayout.TextField("Data folder", setting.commentTextsFolder);

		if (EditorGUI.EndChangeCheck()) {
			if (onSettingChanged != null) {
				onSettingChanged.Invoke();
			}
		}
	}

	[MenuItem("Tools/Comment Object Tool/Open setting")]
	static void OpenSetting() {
		Selection.activeObject = GetSetting;;
	}
}
