using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CuttingRoom;
using System.Linq;

public class GenerateMediaSourcesEditor : EditorWindow
{
	private static GenerateMediaSourcesEditor generateMediaSourcesEditor = null;

	[MenuItem("Cutting Room/Generate Media Sources")]
	private static void ShowGenerateMediaSourcesEditor()
	{
		generateMediaSourcesEditor = GetWindow<GenerateMediaSourcesEditor>("Generate Media Sources");
	}

	/// <summary>
	/// The media controller to apply to the generated media sources.
	/// </summary>
	private MediaController mediaControllerPrefab = null;

	/// <summary>
	/// The output directory for generating media sources.
	/// </summary>
	private string outputDirectory = string.Empty;

	private List<MediaSourceData> mediaSourceDataInstances = new List<MediaSourceData>();

	private Vector2 scrollPosition = Vector2.zero;

	private void OnGUI()
	{
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		EditorGUILayout.Space();

		RenderEditor();

		EditorGUILayout.EndScrollView();
	}

	private MediaSourceData AddMediaSourceData()
	{
		MediaSourceData mediaSourceData = new MediaSourceData();

		mediaSourceDataInstances.Add(mediaSourceData);

		return mediaSourceData;
	}

	private void RenderEditor()
	{
		EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("MediaController prefab:");

		mediaControllerPrefab = EditorGUILayout.ObjectField(mediaControllerPrefab, typeof(MediaController), false) as MediaController;

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Generated Media Source output directory:");

		outputDirectory = EditorLayoutUtilities.FolderPicker("Generated Media Source output directory", outputDirectory);

		if (mediaControllerPrefab != null)
		{
			EditorLayoutUtilities.HorizontalDivider();

			EditorGUILayout.LabelField("Media Source Data", EditorStyles.boldLabel);

			EditorGUILayout.Space();

			if (GUILayout.Button("Add New Media Source"))
			{
				AddMediaSourceData();
			}

			EditorGUILayout.Space();

			mediaControllerPrefab.RenderCreateMultipleFields(position, AddMediaSourceData);

			if (mediaSourceDataInstances.Count > 0)
			{
				EditorLayoutUtilities.HorizontalDivider();

				bool canGenerateMediaSources = mediaSourceDataInstances.Count > 0 && mediaControllerPrefab != null && !string.IsNullOrEmpty(outputDirectory);

				EditorGUI.BeginDisabledGroup(!canGenerateMediaSources);

				if (GUILayout.Button("Generate"))
				{
					if (EditorUtility.DisplayDialog("Generate Media Sources?", "Are you sure you want to generate media sources? This operation will add new assets to your project.", "Yes", "Cancel"))
					{
						try
						{
							for (int i = 0; i < mediaSourceDataInstances.Count; i++)
							{
								float progress = (float)i / (mediaSourceDataInstances.Count - 1);

								EditorUtility.DisplayProgressBar("Generating Media Sources", "", progress);

								string mediaSourceName = typeof(MediaSource).Name;

								if (mediaSourceDataInstances[i].objects != null && mediaSourceDataInstances[i].objects.Count > 0)
								{
									mediaSourceName = mediaSourceDataInstances[i].objects[0].name;
								}
								else if (mediaSourceDataInstances[i].strings != null && mediaSourceDataInstances[i].strings.Count > 0)
								{
									mediaSourceName = System.IO.Path.GetFileNameWithoutExtension(mediaSourceDataInstances[i].strings[0]);
								}

								ScriptableObjectUtilities.Create(outputDirectory, mediaSourceName,
									(MediaSource mediaSource) =>
									{
										// Link the media controller.
										mediaSource.mediaControllerPrefab = mediaControllerPrefab.gameObject;

										mediaSource.mediaSourceData = mediaSourceDataInstances[i];

										EditorUtility.SetDirty(mediaSource);
									});
							}

							EditorUtility.ClearProgressBar();
						}
						catch
						{
							// Hide progress bar.
							EditorUtility.ClearProgressBar();
						}
					}
				}

				EditorGUI.EndDisabledGroup();

				EditorGUILayout.Space();

				bool clearMediaSourceData = false;

				if (GUILayout.Button("Delete All"))
				{
					if (EditorUtility.DisplayDialog("Delete All", "Delete all Media Source Data?", "Yes", "Cancel"))
					{
						clearMediaSourceData = true;
					}
				}

				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Existing Media Source Data");

				EditorGUILayout.Space();

				List<int> deletedMediaSourceDataIndices = new List<int>();

				// Iterate existing media source data and render the inspector.
				for (int mediaSourceDataCount = 0; mediaSourceDataCount < mediaSourceDataInstances.Count; mediaSourceDataCount++)
				{
					// Render the inspector the selected media controller type.
					mediaControllerPrefab.RenderCreateFields(position, mediaSourceDataInstances[mediaSourceDataCount]);

					if (GUILayout.Button("-", GUILayout.MaxWidth(30.0f), GUILayout.MaxHeight(14.0f)))
					{
						deletedMediaSourceDataIndices.Add(mediaSourceDataCount);
					}
				}

				if (clearMediaSourceData)
				{
					mediaSourceDataInstances.Clear();
				}
				else
				{
					// Delete removed.
					for (int deletedMediaSourceIndexCount = deletedMediaSourceDataIndices.Count - 1; deletedMediaSourceIndexCount >= 0; deletedMediaSourceIndexCount--)
					{
						mediaSourceDataInstances.RemoveAt(deletedMediaSourceDataIndices[deletedMediaSourceIndexCount]);
					}
				}
			}
		}

		// Space at the bottom of the editor window to push the Generate button off the very bottom of the window (Apparent when the scroll view is visible after adding a load of media sources).
		EditorLayoutUtilities.Space(EditorLayoutUtilities.SpaceHeight.Medium);

		#region Debugging

		//EditorLayoutUtilities.HorizontalDivider();

		//EditorGUILayout.LabelField("Debugging", EditorStyles.boldLabel);

		//EditorGUILayout.Space();

		//if (GUILayout.Button("Test Load Media Sources"))
		//{
		//	string[] mediaSourceGuids = AssetDatabase.FindAssets("t:MediaSource", new string[] { outputDirectory });

		//	for (int mediaSourceGuidCount = 0; mediaSourceGuidCount < mediaSourceGuids.Length; mediaSourceGuidCount++)
		//	{
		//		string assetPath = AssetDatabase.GUIDToAssetPath(mediaSourceGuids[mediaSourceGuidCount]);

		//		MediaSource mediaSource = AssetDatabase.LoadAssetAtPath(assetPath, typeof(MediaSource)) as MediaSource;

		//		Debug.Log("Prefab exists: " + mediaSource.mediaControllerPrefab != null);
		//		Debug.Log("Data exists:" + mediaSource.mediaSourceData != null);
		//	}
		//}

		#endregion
	}
}
