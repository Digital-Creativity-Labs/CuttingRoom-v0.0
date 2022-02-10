using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class ScriptableObjectUtilities
{
	public static void Create<T>(string path, string fileName, Action<T> onCreateAsset) where T : ScriptableObject
	{
		// Create an instance of the object.
		T asset = ScriptableObject.CreateInstance<T>();

		if (string.IsNullOrEmpty(path))
		{
			throw new System.Exception("No path was specified to save ScriptableObject asset instance.");
		}
		else if (!string.IsNullOrEmpty(Path.GetExtension(path)))
		{
			path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
		}

		string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, fileName + ".asset"));

		AssetDatabase.CreateAsset(asset, assetPath);

		// Call callback for creation to enable asset manipulation before serialization.
		onCreateAsset?.Invoke(asset);

		AssetDatabase.SaveAssets();

		AssetDatabase.Refresh();

		EditorUtility.FocusProjectWindow();

		Selection.activeObject = asset;
	}
}
