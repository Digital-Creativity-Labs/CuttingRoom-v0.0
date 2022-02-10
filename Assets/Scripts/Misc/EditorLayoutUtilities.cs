#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;

public class EditorLayoutUtilities
{
	public enum SpaceHeight
	{
		Undefined = 0,
		Small = 4,
		Medium = 8,
		Large = 12,
	}

	public class DropZone
	{
		private bool mouseIsOver = false;

		public UnityEngine.Object[] Render(string title, int width, int height)
		{
			GUILayout.Box(title, GUILayout.Width(width), GUILayout.Height(height));

			bool isAccepted = false;

			Event currentEvent = Event.current;

			if (currentEvent.type == EventType.Repaint)
			{
				mouseIsOver = GUILayoutUtility.GetLastRect().Contains(currentEvent.mousePosition);
			}
			else if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (currentEvent.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();

					isAccepted = true;
				}

				Event.current.Use();
			}

			// Only return value if mouse is over the box and the drag drop is accepted.
			return isAccepted && mouseIsOver ? DragAndDrop.objectReferences : null;
		}
	}

    public static void HorizontalDivider()
    {
		EditorGUILayout.Space();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
	}

	public static void LabelField(string strLabelText, int nWidth)
	{
		EditorGUILayout.LabelField(strLabelText, GUILayout.MaxWidth(nWidth));
	}

	public static void Space(SpaceHeight eSpaceHeight)
	{
		GUILayout.Space((int)eSpaceHeight);
	}

	public static string FolderPicker(string strFolderPickerWindowTitle, string strStartingDirectoryPath)
	{
		EditorGUILayout.BeginHorizontal();

		EditorGUI.BeginDisabledGroup(true);

		EditorGUILayout.TextField(strStartingDirectoryPath);

		EditorGUI.EndDisabledGroup();

		// Get the project directory path of the unity project.
		string strProjectDirectoryPath = EditorUtilities.GetProjectDirectoryPath();

		// This is the absolute path used in the OpenFolderPanel call.
		string strOpenFolderPanelStartingDirectory = strProjectDirectoryPath + strStartingDirectoryPath;

		// This is the current relative path to the project directory.
		string strFolderPath = strStartingDirectoryPath;

		if (GUILayout.Button("...", GUILayout.MaxWidth(30.0f), GUILayout.MaxHeight(14.0f)))
		{
			// Open folder picker and get absolute path to build directory.
			string directoryPath = EditorUtility.OpenFolderPanel(strFolderPickerWindowTitle, strOpenFolderPanelStartingDirectory, string.Empty);

			// If not closed/cancelled, then set new path.
			if (directoryPath != string.Empty)
			{
				// If the selected directory is inside the assets folder.
				if (directoryPath.Contains(strProjectDirectoryPath))
				{
					string[] astrRelativePath = directoryPath.Split(new string[] { strProjectDirectoryPath }, StringSplitOptions.RemoveEmptyEntries);

					// If the user selected the project folder, this contains the data path but nothing else, so split will be empty.
					if (astrRelativePath.Length > 0)
					{
						strFolderPath = astrRelativePath[0];
					}
					else
					{
						strFolderPath = string.Empty;
					}
				}
				else
				{
					strFolderPath = string.Empty;
				}
			}
		}

		EditorGUILayout.EndHorizontal();

		return strFolderPath;
	}
}

#endif