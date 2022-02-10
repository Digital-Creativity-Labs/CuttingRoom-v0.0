using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[CustomEditor(typeof(GameObjectGuid))]
public class GameObjectGuidInspector : Editor
{
	public override void OnInspectorGUI()
	{
		GameObjectGuid cGameObjectGuid = target as GameObjectGuid;

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		if (!string.IsNullOrEmpty(cGameObjectGuid.Guid))
		{
			EditorGUILayout.LabelField("Guid: " + cGameObjectGuid.Guid);
		}
		else
		{
			EditorGUILayout.LabelField("Guid: Not set");
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Set Guid"))
		{
			SetGameObjectGuid(cGameObjectGuid, false);
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
	}

	static int SetGameObjectGuid(GameObjectGuid cGameObjectGuid, bool bResetWithoutDialog, bool bMultiple = false)
	{
		if (string.IsNullOrEmpty(cGameObjectGuid.Guid))
		{
			Undo.RecordObject(cGameObjectGuid, "Set Guid");
			cGameObjectGuid.GenerateGuid();

			return 0;
		}
		else
		{
			int nSelection = -1;

			if (bResetWithoutDialog)
			{
				nSelection = 0;
			}
			else if (bMultiple)
			{
				nSelection = EditorUtility.DisplayDialogComplex("Reset Guid?", "Are you sure you want to regenerate the Guid on GameObject named " + cGameObjectGuid.gameObject.name + "?", "Yes", "Yes To All", "No");

				if (nSelection != 2)
				{
					Undo.RecordObject(cGameObjectGuid, "Set Guid");
					cGameObjectGuid.GenerateGuid();
				}
			}
			else
			{
				if (EditorUtility.DisplayDialog("Reset Guid?", "Are you sure you want to regenerate the Guid on GameObject named " + cGameObjectGuid.gameObject.name + "?", "Yes", "No"))
				{
					Undo.RecordObject(cGameObjectGuid, "Set Guid");
					cGameObjectGuid.GenerateGuid();
				}
			}

			return nSelection;
		}
	}

	[PostProcessScene]
	static void OnPostprocessScene()
	{
		GameObjectGuid[] acGameObjectGuids = FindObjectsOfType<GameObjectGuid>();

		for (int nGameObjectGuid = 0; nGameObjectGuid < acGameObjectGuids.Length; nGameObjectGuid++)
		{
			if (string.IsNullOrEmpty(acGameObjectGuids[nGameObjectGuid].Guid))
			{
				//Debug.LogError("GameObjectGuid on GameObject named " + acGameObjectGuids[nGameObjectGuid].gameObject.name + " does not have a Guid set. Please set this in the inspector via the \"Set Guid\" button.");
			}
		}
	}

	[MenuItem("Tools/GameObjectGuidSystem/Set Guids")]
	static void SetGuids()
	{
		GameObjectGuid[] acGameObjectGuids = FindObjectsOfType(typeof(GameObjectGuid)) as GameObjectGuid[];

		bool bResetAll = false;

		for (int nGameObjectGuid = 0; nGameObjectGuid < acGameObjectGuids.Length; nGameObjectGuid++)
		{
			int nSelection = SetGameObjectGuid(acGameObjectGuids[nGameObjectGuid], bResetAll, acGameObjectGuids.Length > 1);

			if (!bResetAll)
			{
				bResetAll = nSelection == 1 ? true : false;
			}
		}
	}
}
