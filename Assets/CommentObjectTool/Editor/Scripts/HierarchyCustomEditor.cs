using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class HierarchyCustomEditor {
	
	#region Comment setting to store and show texts on Hierarchy
	static readonly string defaultCommentValue = "//";
	static string defaultCommentSettingPath = "Assets/CommentObjectTool/Editor/Data/CommentData.asset";

	static string currentScene = string.Empty;
	static IDictionary<string, string> commentedValues = new Dictionary<string, string>();
	static HierarchyCommentSetting commentSetting = null;
	static CommentObjectToolSetting commentObjectToolSetting;

	static bool isInitialized = false;

	static HierarchyCustomEditor () {
		currentScene = EditorSceneManager.GetActiveScene().name;
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
		EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
		CommentObjectToolSettingEditor.onSettingChanged += OnCommentObjectToolSettingChanged;
	}

	static bool InitSetting(bool forceInit = false) {
		if (commentSetting == null || forceInit) {
			commentObjectToolSetting = CommentObjectToolSettingEditor.GetSetting;

			if (commentObjectToolSetting == null) {
				Debug.LogError("Cannot load setting");
				return false;
			}

			if (string.IsNullOrEmpty(currentScene)) {
				currentScene = EditorSceneManager.GetActiveScene().name;
				//Debug.LogError("Not init current scene name yet");
				return false;
			}

			if (commentObjectToolSetting.saveCommentTextsInMultipleObject) {
				defaultCommentSettingPath = commentObjectToolSetting.commentTextsFolder + "/" + currentScene + "_Comment.asset";

			} else {
				defaultCommentSettingPath = commentObjectToolSetting.commentTextsFolder + "/CommentSetting.asset";
			}

			try {
				commentSetting = AssetDatabase.LoadAssetAtPath<HierarchyCommentSetting>(defaultCommentSettingPath);

				if (commentSetting == null) {
					string assetPath = AssetDatabase.GenerateUniqueAssetPath(defaultCommentSettingPath);
					AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<HierarchyCommentSetting>(), assetPath);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					EditorUtility.FocusProjectWindow();
				}

				EditorUtility.SetDirty(commentSetting);
			} catch (System.Exception e) {
				Debug.LogError("Exception:  " + e.Message);
			}

			if (commentSetting == null) {
				Debug.LogError("Cannot load comment setting");
				return false;
			}

			//assign comment values from setting, if a comment is empty, delete from setting
			IList<string> blankCommentKey = null;
			foreach (KeyValuePair<string, string> pair in commentSetting.commentedValues) {
				//Debug.Log("Load value: " + pair.Key + " - " + pair.Value);
				if (string.IsNullOrEmpty(pair.Value) || string.Equals(defaultCommentValue, pair.Value)) {
					if (blankCommentKey == null) {
						blankCommentKey = new List<string>();
					}
					blankCommentKey.Add(pair.Key);
				} else {
					commentedValues[pair.Key] = pair.Value;
				}
			}

			if (blankCommentKey != null) {
				for (int i = blankCommentKey.Count - 1; i >= 0; i--) {
//					Debug.LogError("Delete blank comment: " + blankCommentKey[i]);
					commentSetting.commentedValues.Remove(blankCommentKey[i]);
				}
			}

			return true;
		}

		return true;
	}

	#region Check Object in scene changed
	static void OnCommentObjectToolSettingChanged() {
		InitSetting(true);
		Debug.LogWarning("Comment setting changed, data reloaded");
	}

	static void OnHierarchyChanged () {
		if (commentObjectToolSetting == null) {
			return;
		}

		if (commentSetting == null) {
			return;
		}

		if (commentSetting.enable == false) {
			return;
		}

		GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
		List<GameObject> allInScene = new List<GameObject>(10);
		for (int i = all.Length - 1; i >= 0; i--) {
			if (all[i].hideFlags == HideFlags.None) {
				allInScene.Add(all[i]);
			}
		}
		CheckObjectChanged(allInScene.ToArray());
	}

	//cache name of all objects in scene loaded
	static Dictionary<int, string> dicCurrentObjectsName = new Dictionary<int, string>(); 
	//used to check if an object was deleted
	static IList<int> objectIdDeletedCheck;

	/// <summary>
	/// Check all objects in current scene, if an object is renamed or deleted, update its data in comment setting
	/// </summary>
	/// <param name="currentObjectsInScene">All objects in scene after changed.</param>
	static void CheckObjectChanged(GameObject[] currentObjectsInScene) {
		//if load other scene, reset old data from previous scene loaded.
		if (string.Equals(currentScene, EditorSceneManager.GetActiveScene().name) == false) {
			currentScene = EditorSceneManager.GetActiveScene().name;
			dicCurrentObjectsName.Clear();

			if (commentObjectToolSetting.saveCommentTextsInMultipleObject) {
				InitSetting(true);
			}
		}
		objectIdDeletedCheck = new List<int>(dicCurrentObjectsName.Keys);//first of delete check, init by all keys of name cache
		string tempNameCheck = null;
		for (int i = 0; i < currentObjectsInScene.Length; i++) {


			//if object after changed still appears in cached, just check its name
			if (dicCurrentObjectsName.TryGetValue(currentObjectsInScene[i].GetInstanceID(), out tempNameCheck)) {
				if (string.Equals(tempNameCheck, currentObjectsInScene[i].name) == false) { //yeah! name changed
					string oldGuid = tempNameCheck + "_" + currentObjectsInScene[i].scene.name;
					//check if the renamed object has any comment
					if (commentedValues.ContainsKey(oldGuid)) {
						//set value of current key (that accessed by old name) to new key (accessed by new name)
						string newGuid = currentObjectsInScene[i].name + "_" + currentObjectsInScene[i].scene.name;
						commentedValues[newGuid] = commentedValues[oldGuid];
						commentSetting.commentedValues[newGuid] = commentSetting.commentedValues[oldGuid];

						//delete old key, value in comment setting
						commentedValues.Remove(oldGuid);
						commentSetting.commentedValues.Remove(oldGuid);
					}

					//update new object name to name cache
					dicCurrentObjectsName[currentObjectsInScene[i].GetInstanceID()] = currentObjectsInScene[i].name;
				}

				//second of delete check, remove id of changed object that still contain in name cache
				objectIdDeletedCheck.Remove(currentObjectsInScene[i].GetInstanceID());
			} else {
				//add new object to name cache if not already exist
				dicCurrentObjectsName[currentObjectsInScene[i].GetInstanceID()] = currentObjectsInScene[i].name;
			}
		}

		//third of delete check, delete id, key... that still contain in cache but not exist in new changed objects list
		if (objectIdDeletedCheck.Count > 0) {
			string guid;
			for (int i = objectIdDeletedCheck.Count - 1; i >= 0; i--) {
				guid = dicCurrentObjectsName[objectIdDeletedCheck[i]] + "_" + currentScene;
				commentedValues.Remove(guid);
				commentSetting.commentedValues.Remove(guid);
				dicCurrentObjectsName.Remove(objectIdDeletedCheck[i]);
			}
		}
	}
	#endregion

	/// <summary>
	/// Show comment in Hierarchy
	/// </summary>
	static void OnHierarchyGUI (int selectedId, Rect selectedRect) {
		if (InitSetting() == false) {
			return;
		}

		if (commentSetting.enable == false) {
			return;
		}

		Rect buttonRect = new Rect(selectedRect);
		buttonRect.x = buttonRect.width + buttonRect.x - 20;
		buttonRect.width = 20;

		string commentValue = null;
		GameObject objTemp = (GameObject)EditorUtility.InstanceIDToObject(selectedId);
		if (objTemp == null) {
			return;
		}
		string guid = objTemp.name + "_" + objTemp.scene.name;
		if (commentedValues.TryGetValue(guid,out commentValue)) {
			if (string.IsNullOrEmpty(commentValue) == false) {
				GUI.DrawTexture(new Rect(buttonRect.x, buttonRect.y, 15, buttonRect.height)
					, EditorGUIUtility.FindTexture("lightMeter/greenLight"));
			}
		}


		if (Selection.activeGameObject == null) {
			return;
		}

		if (Selection.activeGameObject.GetInstanceID() != selectedId) {
			return;
		}

		if (string.IsNullOrEmpty(commentValue) == false) {
			GUILayout.BeginArea(new Rect(buttonRect.x - 120, buttonRect.y + 10, 250, 150), string.Empty);
			commentedValues[guid] = GUILayout.TextArea(commentValue, GUILayout.Width(120f), GUILayout.MinHeight(100));
			GUILayout.EndArea();
			commentSetting.commentedValues[guid] = commentedValues[guid];

			//empty comment if object is deleted by 'Delete' button icon
			if (GUI.Button(buttonRect, EditorGUIUtility.FindTexture("lightMeter/redLight"))) {
				commentedValues.Remove(guid);
				commentSetting.commentedValues.Remove(guid);
//				Debug.LogError("Deleted by button: " + guid);
			}
		} else {
			//(Texture)EditorGUIUtility.Load("CommentObjectTool/EditIcon.png");
			if (GUI.Button(buttonRect, EditorGUIUtility.FindTexture("TextAsset Icon"))) {
				commentedValues[guid] = defaultCommentValue;
			}
		}

		EditorStyles.textArea.stretchHeight = true;
		EditorStyles.textArea.wordWrap = false;
	}
	#endregion

	#region Show texts on Scene view
	static string textShownOnObject = string.Empty;
	static GUIStyle commentTextStyle = new GUIStyle();

	[DrawGizmo(GizmoType.Active | GizmoType.NonSelected)]
	static void DrawHandlesText(Transform drawnTarget, GizmoType gizmoType) {
		if (isHideAll) {//
			return;
		}

		commentTextStyle.normal.textColor = Color.white;

		if (commentedValues.TryGetValue(drawnTarget.gameObject.name + "_" + currentScene, out textShownOnObject)) {
			Handles.Label(drawnTarget.position, textShownOnObject, commentTextStyle);
		}
	}

	/// <summary>
	/// Draws the handles text on UI component
	/// </summary>
	/// <param name="drawnTarget">Drawn target.</param>
	/// <param name="gizmoType">Gizmo type.</param>
	[DrawGizmo(GizmoType.Active | GizmoType.NonSelected)]
	static void DrawHandlesText(RectTransform drawnTarget, GizmoType gizmoType) {
		if (isHideAll) {
			return;
		}

		commentTextStyle.normal.textColor = Color.white;

		if (commentedValues.TryGetValue(drawnTarget.gameObject.name + "_" + currentScene, out textShownOnObject)) {
			Handles.Label(drawnTarget.position, textShownOnObject, commentTextStyle);
		}
	}
	#endregion

	#region Menu controls
	static bool isHideAll;

	[MenuItem("Tools/Comment Object Tool/Toggle all comment visible %&c")]
	private static void ShowAllCommentInScene() {
		isHideAll = !isHideAll;
		SceneView.RepaintAll();
	}

	[MenuItem("Tools/Comment Object Tool/Open current scene comment %&v")]
	static void OpenCurrentSceneComment() {
		if (commentObjectToolSetting.saveCommentTextsInMultipleObject) {

		} else {

		}
		Selection.activeObject = commentSetting;;
	}
	#endregion
}
