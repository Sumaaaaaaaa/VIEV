using UnityEngine;

public class CommentObjectToolSetting : ScriptableObject {
	public bool saveCommentTextsInMultipleObject;
	public string commentTextsFolder;

	/// <summary>
	/// Default value:
	/// saveCommentTextsInMultipleObject = false;
	/// commentTextsFolder = "Assets/CommentObjectTool/Editor/Data";
	/// </summary>
	public CommentObjectToolSetting() {
		saveCommentTextsInMultipleObject = true;
		commentTextsFolder = "Assets/CommentObjectTool/Editor/Data";
	}
}
