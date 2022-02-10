#if UNITY_EDITOR

using UnityEngine;
using System.IO;

public class EditorUtilities
{
	public static string GetProjectDirectoryPath()
	{
		// Get the path for the project directory of the unity project.
		string strProjectDirectoryPath = Directory.GetParent(Application.dataPath).FullName + "/";

		// Replace the slashes between folders to match format that Unity uses.
		strProjectDirectoryPath = strProjectDirectoryPath.Replace("\\", "/");

		return strProjectDirectoryPath;
	}
}

#endif